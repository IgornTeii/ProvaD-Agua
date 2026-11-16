using ProvaDagua.Domain.Readings;

namespace ProvaDagua.Domain.FloodEvents;

public static class FloodRecessionPolicy
{
    public const int ReadingsRequired = 18;

    public static bool HasReceded(IReadOnlyList<Reading> latestReadings) =>
        latestReadings.Count >= ReadingsRequired &&
        latestReadings.All(r => r.WaterLevelCm < FloodEvent.SuspicionThresholdCm);
}