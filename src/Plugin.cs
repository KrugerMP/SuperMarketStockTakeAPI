using System.IO;
using BepInEx;
using BepInEx.Logging;
using SuperMarketStockTakeAPI.Domain.Http;
using SuperMarketStockTakeAPI.Services;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SuperMarketStockTakeAPI;

[BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
public class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource logger;

    private LocalApiGameState _apiState;
    private LocalHttpApiService _httpApi;
    private float _lastProductsJsonRefresh = float.NegativeInfinity;

    private void Update()
    {
        _apiState.UpdateStats(Time.time, Time.frameCount, SceneManager.GetActiveScene().name ?? "");

        if (Time.unscaledTime - _lastProductsJsonRefresh >= 1f)
        {
            _lastProductsJsonRefresh = Time.unscaledTime;
            _apiState.RefreshProductsJson();
            _apiState.RefreshSpawnedProductsJson();
        }

        if (Input.GetKeyDown(KeyCode.F8))
        {
            DumpStats();
        }
    }

    private void DumpStats()
    {
        // TODO: replace with actual game objects (use UnityExplorer to find them)
        string data = "Money: ???\nSales: ???\nTime: " + Time.time;
        File.WriteAllText(Path.Combine(Path.Combine(Application.dataPath, ".."), "supermarket_stats.txt"), data);
        logger.LogInfo("Stats dumped to supermarket_stats.txt");
    }

    private void Awake()
    {
        logger = base.Logger;
        logger.LogInfo($"Plugin {PluginInfo.GUID} is loaded!");

        _apiState = new LocalApiGameState();

        _httpApi = new LocalHttpApiService(logger, _apiState);
        _httpApi.Start();
    }

    private void OnDestroy()
    {
        _httpApi?.Stop();
    }
}