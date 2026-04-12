namespace SuperMarketStockTakeAPI.Domain.Ping;

/// <summary>
/// GET /ping response body.
/// </summary>
public static class PingApi
{
    public static readonly string JsonBody =
        $"{{\"ping\":\"pong version {PluginInfo.Version}\"}}";
}