using System.IO;
using BepInEx;
using BepInEx.Logging;
using MySupermarketDataMod.Domain.Http;
using MySupermarketDataMod.Domain.StoreProducts;
using MySupermarketDataMod.Services;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MySupermarketDataMod;

[BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

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
        Logger.LogInfo("Stats dumped to supermarket_stats.txt");
    }

    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {PluginInfo.GUID} is loaded!");

        _apiState = new LocalApiGameState();

        _httpApi = new LocalHttpApiService(Logger, _apiState);
        _httpApi.Start();
    }

    private void OnDestroy()
    {
        _httpApi?.Stop();
    }
}