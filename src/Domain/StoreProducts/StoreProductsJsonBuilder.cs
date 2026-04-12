using System.Collections.Generic;
using MySupermarketDataMod.Domain.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace MySupermarketDataMod.Domain.StoreProducts;

/// <summary>
/// Builds JSON for GET /products on the main thread.
/// Shelf stock lives on <see cref="Data_Container"/> (<c>productInfoArray</c> pairs: productId, quantity), not in <see cref="ProductListing.productsData"/> (that is the static catalog).
/// <see cref="ProductListing.ProductData"/> has no dedicated display-name field; <c>productName</c> is taken from <see cref="ProductListing.ProductData.productPrefab"/><c>.name</c> when present.
/// </summary>
public static class StoreProductsJsonBuilder
{
    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        Formatting = Formatting.Indented,
        Converters = { new StringEnumConverter(new CamelCaseNamingStrategy()) },
    };

    public static string GetAllProductStats(LocalApiGameState state)
    {
        ProductListing listing = ProductListing.Instance;
        if (listing == null)
        {
            return JsonConvert.SerializeObject(
                new { error = "ProductListing not available (not in a loaded game session)." },
                JsonSettings);
        }

        Data_Container[] containers = Object.FindObjectsByType<Data_Container>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);

        List<object> shelfProducts = new List<object>();
        foreach (Data_Container container in containers)
        {
            int[] arr = container.productInfoArray;
            int slotCount = arr.Length / 2;
            for (int slot = 0; slot < slotCount; slot++)
            {
                int productId = arr[slot * 2];
                int quantity = arr[slot * 2 + 1];
                if (productId < 0 || quantity <= 0)
                {
                    continue;
                }

                string brand = null;
                string productName = null;
                if (productId < listing.productsData.Length)
                {
                    ProductListing.ProductData pd = listing.productsData[productId];
                    brand = pd.productBrand;
                    if (pd.productPrefab != null)
                    {
                        productName = pd.productPrefab.name;
                    }
                }

                ContainerClassKind containerClassKind = ContainerClassKindExtensions.FromGameClass(container.containerClass);
                shelfProducts.Add(
                    new
                    {
                        containerId = container.containerID,
                        containerClass = container.containerClass,
                        containerClassKind,
                        slotIndex = slot,
                        productId,
                        quantity,
                        productName,
                        productBrand = brand,
                    });
            }
        }

        return JsonConvert.SerializeObject(shelfProducts, JsonSettings);
    }
}