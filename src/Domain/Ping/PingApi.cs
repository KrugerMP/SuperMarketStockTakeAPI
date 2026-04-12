namespace MySupermarketDataMod.Domain.Ping;

/// <summary>
/// GET /ping response body.
/// </summary>
public static class PingApi
{
    public const string JsonBody = "{\"ping\":\"pong version 0.0.8\"}";
}