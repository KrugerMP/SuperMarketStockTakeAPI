using SuperMarketStockTakeAPI.Domain.PlayerStats;
using SuperMarketStockTakeAPI.Domain.SpawnedProducts;
using SuperMarketStockTakeAPI.Domain.StoreProducts;

namespace SuperMarketStockTakeAPI.Domain.Http;

/// <summary>
/// Thread-safe snapshot for HTTP handlers (listener runs off the main thread).
/// Products JSON is refreshed on the Unity main thread; GET /products reads the cached string only.
/// </summary>
public sealed class LocalApiGameState
{
    private readonly object _lock = new();
    private float _cachedTime;
    private int _cachedFrame;
    private string _cachedLevelName = "";
    private string _cachedProductsJson = "[]";
    private string _cachedSpawnedProductsJson = "[]";

    public void UpdateStats(float time, int frame, string levelName)
    {
        lock (_lock)
        {
            _cachedTime = time;
            _cachedFrame = frame;
            _cachedLevelName = levelName ?? "";
        }
    }

    /// <summary>
    /// Call from the Unity main thread only (e.g. <c>MonoBehaviour.Update</c>).
    /// </summary>
    public void RefreshProductsJson()
    {
        string json = StoreProductsJsonBuilder.GetAllProductStats();
        lock (_lock)
        {
            _cachedProductsJson = json;
        }
    }

    /// <summary>
    /// Call from the Unity main thread only (e.g. <c>MonoBehaviour.Update</c>).
    /// </summary>
    public void RefreshSpawnedProductsJson()
    {
        string json = SpawnedProductsJsonBuilder.GetSpawnedProductsJson();
        lock (_lock)
        {
            _cachedSpawnedProductsJson = json;
        }
    }

    public string GetStatsJson()
    {
        lock (_lock)
        {
            return PlayerStatsApi.FormatJson(_cachedTime, _cachedFrame, _cachedLevelName);
        }
    }

    public string GetProductsJson()
    {
        lock (_lock)
        {
            return _cachedProductsJson;
        }
    }

    public string GetSpawnedProductsJson()
    {
        lock (_lock)
        {
            return _cachedSpawnedProductsJson;
        }
    }
}