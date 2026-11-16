using ProvaDagua.Domain.Common;

namespace ProvaDagua.Domain.Readings;

public static class ReadingErrors
{
    public static readonly Error SensorRequired    = new("reading.sensor_required", "Sensor is required.");
    public static readonly Error InvalidWaterLevel = new("reading.invalid_water_level", "Water level is outside the physically possible range.");
    public static readonly Error InvalidRainfall   = new("reading.invalid_rainfall", "Rainfall cannot be negative.");
    public static readonly Error MeasuredInFuture  = new("reading.measured_in_future", "Measurement date is in the future.");
}