using System.Collections.Generic;
using System.Reflection;
using MySupermarketDataMod.Domain.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace MySupermarketDataMod.Domain.SpawnedProducts;

/// <summary>
/// Uses <see cref="ManagerBlackboard"/> (same object graph as <see cref="ProductListing"/> in-game): cargo queue, shopping list UI,
/// delivery boxes under <see cref="ManagerBlackboard.boxParent"/>, and optional private <c>GetProductsExistences</c> via reflection
/// (same approach as the LowCountProducts mod on GitHub).
/// </summary>
public static class SpawnedProductsJsonBuilder
{
    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        Formatting = Formatting.Indented,
    };

    private static MethodInfo _getProductsExistencesMethod;

    public static string GetSpawnedProductsJson(LocalApiGameState state)
    {
        ProductListing listing = ProductListing.Instance;
        if (listing == null)
        {
            return JsonConvert.SerializeObject(
                new { error = "ProductListing not available (not in a loaded game session)." },
                JsonSettings);
        }

        ManagerBlackboard blackboard = ResolveManagerBlackboard(listing);
        if (blackboard == null)
        {
            return JsonConvert.SerializeObject(
                new { error = "ManagerBlackboard not found (GameData / listing not ready)." },
                JsonSettings);
        }

        var payload = new
        {
            cargoSpawnQueue = BuildCargoQueue(blackboard, listing),
            shoppingList = BuildShoppingList(blackboard, listing),
            deliveryBoxes = BuildDeliveryBoxes(blackboard, listing),
            stockCounts = BuildStockCounts(blackboard, listing),
        };

        return JsonConvert.SerializeObject(payload, JsonSettings);
    }

    private static ManagerBlackboard ResolveManagerBlackboard(ProductListing listing)
    {
        ManagerBlackboard mb = listing.GetComponent<ManagerBlackboard>();
        if (mb != null)
        {
            return mb;
        }

        if (GameData.Instance != null)
        {
            return GameData.Instance.GetComponent<ManagerBlackboard>();
        }

        return null;
    }

    private static List<object> BuildCargoQueue(ManagerBlackboard blackboard, ProductListing listing)
    {
        List<object> list = new List<object>();
        if (blackboard.idsToSpawn == null || blackboard.idsToSpawn.Count == 0)
        {
            return list;
        }

        for (int i = 0; i < blackboard.idsToSpawn.Count; i++)
        {
            int productId = blackboard.idsToSpawn[i];
            TryGetCatalogFields(listing, productId, out string productName, out string brand);
            list.Add(
                new
                {
                    queueIndex = i,
                    productId,
                    productName,
                    productBrand = brand,
                    note = "Product IDs waiting for ServerCargoSpawner box drops (often only populated on host).",
                });
        }

        return list;
    }

    private static List<object> BuildShoppingList(ManagerBlackboard blackboard, ProductListing listing)
    {
        List<object> list = new List<object>();
        if (blackboard.shoppingListParent == null)
        {
            return list;
        }

        int index = 0;
        foreach (Transform child in blackboard.shoppingListParent.transform)
        {
            InteractableData data = child.GetComponent<InteractableData>();
            if (data == null)
            {
                continue;
            }

            int productId = data.thisSkillIndex;
            TryGetCatalogFields(listing, productId, out string productName, out string brand);
            list.Add(
                new
                {
                    listIndex = index++,
                    productId,
                    productName,
                    productBrand = brand,
                });
        }

        return list;
    }

    private static List<object> BuildDeliveryBoxes(ManagerBlackboard blackboard, ProductListing listing)
    {
        List<object> list = new List<object>();
        if (blackboard.boxParent == null)
        {
            return list;
        }

        int index = 0;
        foreach (Transform child in blackboard.boxParent)
        {
            BoxData box = child.GetComponent<BoxData>();
            if (box == null)
            {
                continue;
            }

            int productId = box.productID;
            int units = box.numberOfProducts;
            TryGetCatalogFields(listing, productId, out string productName, out string brand);
            Vector3 p = child.position;
            list.Add(
                new
                {
                    boxIndex = index++,
                    productId,
                    unitsInBox = units,
                    productName,
                    productBrand = brand,
                    position = new { x = p.x, y = p.y, z = p.z },
                });
        }

        return list;
    }

    private static List<object> BuildStockCounts(ManagerBlackboard blackboard, ProductListing listing)
    {
        List<object> list = new List<object>();
        MethodInfo method = GetGetProductsExistencesMethod();
        if (method == null || listing.availableProducts == null)
        {
            return list;
        }

        foreach (int productId in listing.availableProducts)
        {
            int[] q = InvokeGetProductsExistences(method, blackboard, productId);
            if (q == null || q.Length < 3)
            {
                continue;
            }

            TryGetCatalogFields(listing, productId, out string productName, out string brand);
            list.Add(
                new
                {
                    productId,
                    productName,
                    productBrand = brand,
                    onShelves = q[0],
                    inStorage = q[1],
                    inBoxes = q[2],
                });
        }

        return list;
    }

    private static MethodInfo GetGetProductsExistencesMethod()
    {
        if (_getProductsExistencesMethod != null)
        {
            return _getProductsExistencesMethod;
        }

        _getProductsExistencesMethod = typeof(ManagerBlackboard).GetMethod(
            "GetProductsExistences",
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            new[] { typeof(int) },
            null);
        return _getProductsExistencesMethod;
    }

    private static int[] InvokeGetProductsExistences(MethodInfo method, ManagerBlackboard blackboard, int productId)
    {
        try
        {
            return (int[])method.Invoke(blackboard, new object[] { productId });
        }
        catch
        {
            return new int[3];
        }
    }

    private static void TryGetCatalogFields(
        ProductListing listing,
        int productId,
        out string productName,
        out string brand)
    {
        productName = null;
        brand = null;
        if (productId < 0 || productId >= listing.productsData.Length)
        {
            return;
        }

        ProductListing.ProductData pd = listing.productsData[productId];
        brand = pd.productBrand;
        if (pd.productPrefab != null)
        {
            productName = pd.productPrefab.name;
        }
    }
}