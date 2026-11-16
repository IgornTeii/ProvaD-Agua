using ProvaDagua.Domain.Common;

namespace ProvaDagua.Domain.FloodEvents;

public static class FloodEventErrors
{
    public static readonly Error SensorRequired = new("flood_event.sensor_required", "Sensor is required.");
    public static readonly Error InvalidWaterLevel = new("flood_event.invalid_water_level", "Water level cannot be negative.");
    public static readonly Error OnlySuspectedCanBeConfirmed = new("flood_event.only_suspected_can_be_confirmed", "Only a suspected event can be confirmed.");
    public static readonly Error OnlyConfirmedCanBeClosed = new("flood_event.only_confirmed_can_be_closed", "Only a confirmed event can be closed.");
    public static readonly Error ClosedBeforeStart = new("flood_event.closed_before_start", "An event cannot close before it started.");
    public static readonly Error EventAlreadyClosed = new("flood_event.already_closed", "A closed event cannot receive new readings.");
    public static readonly Error OnlySuspectedCanBeDismissed = new("flood_event.only_suspected_can_be_dismissed", "Only a suspected event can be dismissed.");
}