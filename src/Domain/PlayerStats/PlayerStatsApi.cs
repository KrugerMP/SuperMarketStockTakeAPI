using System.Globalization;
using MySupermarketDataMod.Domain.Json;

namespace MySupermarketDataMod.Domain.PlayerStats;

/// <summary>
/// GET /stats JSON built from a main-thread snapshot.
/// </summary>
public static class PlayerStatsApi
{
    public static string FormatJson(float time, int frame, string levelName)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "{{\"time\":{0},\"frame\":{1},\"level\":{2}}}",
            time,
            frame,
            JsonStringHelper.JsonString(levelName ?? ""));
    }
}