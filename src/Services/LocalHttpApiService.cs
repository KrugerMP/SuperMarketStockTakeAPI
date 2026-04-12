using System;
using System.Net;
using System.Text;
using System.Threading;
using BepInEx.Logging;
using MySupermarketDataMod.Domain.Http;
using MySupermarketDataMod.Domain.Ping;

namespace MySupermarketDataMod.Services;

/// <summary>
/// HTTP listener on localhost; routes GET requests to domain handlers.
/// </summary>
public sealed class LocalHttpApiService
{
    private readonly ManualLogSource _logger;
    private readonly LocalApiGameState _gameState;

    private HttpListener _listener;
    private volatile bool _listenerRunning;
    private Thread _listenerThread;

    public LocalHttpApiService(ManualLogSource logger, LocalApiGameState gameState)
    {
        _logger = logger;
        _gameState = gameState;
    }

    public void Start()
    {
        try
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:8080/");
            _listener.Start();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to start local API: {ex.Message}");
            return;
        }

        _listenerRunning = true;
        _listenerThread = new Thread(ListenerLoop) { IsBackground = true };
        _listenerThread.Start();
        _logger.LogInfo("Local API: GET /ping, GET /stats, GET /products, GET /spawnedProducts (see README)");
    }

    public void Stop()
    {
        _listenerRunning = false;
        if (_listener != null && _listener.IsListening)
        {
            _listener.Stop();
            _listener.Close();
        }

        _listener = null;
    }

    private void ListenerLoop()
    {
        while (_listenerRunning && _listener != null && _listener.IsListening)
        {
            try
            {
                HttpListenerContext context = _listener.GetContext();
                HandleRequest(context);
            }
            catch (HttpListenerException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Local API request error: {ex.Message}");
            }
        }
    }

    private void HandleRequest(HttpListenerContext context)
    {
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;
        try
        {
            string path = request.Url.AbsolutePath.TrimEnd('/');
            if (request.HttpMethod != "GET")
            {
                response.StatusCode = 405;
                return;
            }

            if (string.Equals(path, "/ping", StringComparison.OrdinalIgnoreCase))
            {
                WriteJson(response, PingApi.JsonBody);
                return;
            }

            if (string.Equals(path, "/stats", StringComparison.OrdinalIgnoreCase))
            {
                WriteJson(response, _gameState.GetStatsJson());
                return;
            }

            if (string.Equals(path, "/products", StringComparison.OrdinalIgnoreCase))
            {
                WriteJson(response, _gameState.GetProductsJson());
                return;
            }

            if (string.Equals(path, "/spawnedProducts", StringComparison.OrdinalIgnoreCase))
            {
                WriteJson(response, _gameState.GetSpawnedProductsJson());
                return;
            }

            response.StatusCode = 404;
        }
        finally
        {
            response.OutputStream.Close();
        }
    }

    private static void WriteJson(HttpListenerResponse response, string json)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(json);
        response.ContentType = "application/json; charset=utf-8";
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
    }
}