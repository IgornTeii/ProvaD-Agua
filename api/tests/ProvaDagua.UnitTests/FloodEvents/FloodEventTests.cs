using ProvaDagua.Domain.FloodEvents;

namespace ProvaDagua.UnitTests.FloodEvents;

public class FloodEventTests
{
    private static readonly DateTimeOffset Start = new(2026, 11, 15, 18, 0, 0, TimeSpan.FromHours(-3));

    private static FloodEvent NewSuspectedEvent() =>
        FloodEvent.Start("sensor-01", 20m, Start).Value!;

    [Fact]
    public void Close_WhenEventIsOnlySuspected_ReturnsFailure()
    {
        var floodEvent = NewSuspectedEvent();

        var result = floodEvent.Close(Start.AddHours(1));

        Assert.False(result.IsSuccess);
        Assert.Equal(FloodEventErrors.OnlyConfirmedCanBeClosed, result.Error);
        Assert.Equal(FloodEventStatus.Suspected, floodEvent.Status);
    }

    [Fact]
    public void Confirm_WhenEventIsSuspected_ChangesStatusToConfirmed()
    {
        var floodEvent = NewSuspectedEvent();

        var result = floodEvent.Confirm(Start.AddMinutes(10));

        Assert.True(result.IsSuccess);
        Assert.Equal(FloodEventStatus.Confirmed, floodEvent.Status);
    }

    [Fact]
    public void Confirm_WhenEventIsAlreadyConfirmed_ReturnsFailure()
    {
        var floodEvent = NewSuspectedEvent();
        floodEvent.Confirm(Start.AddMinutes(10));

        var result = floodEvent.Confirm(Start.AddMinutes(20));

        Assert.False(result.IsSuccess);
        Assert.Equal(FloodEventErrors.OnlySuspectedCanBeConfirmed, result.Error);
        Assert.Equal(FloodEventStatus.Confirmed, floodEvent.Status);
    }

    [Fact]
    public void Dismiss_WhenEventIsSuspected_ChangesStatusToDismissed()
    {
        var dismissedAt = Start.AddMinutes(10);
        var floodEvent = NewSuspectedEvent();

        var result = floodEvent.Dismiss(dismissedAt);

        Assert.True(result.IsSuccess);
        Assert.Equal(FloodEventStatus.Dismissed, floodEvent.Status);
        Assert.Equal(dismissedAt, floodEvent.DismissedAt);
    }

    [Fact]
    public void Dismiss_WhenEventIsAlreadyDismissed_ReturnsFailure()
    {
        var floodEvent = NewSuspectedEvent();
        floodEvent.Dismiss(Start.AddMinutes(10));

        var result = floodEvent.Dismiss(Start.AddMinutes(20));

        Assert.False(result.IsSuccess);
        Assert.Equal(FloodEventErrors.OnlySuspectedCanBeDismissed, result.Error);
        Assert.Equal(FloodEventStatus.Dismissed, floodEvent.Status);
    }

    [Fact]
    public void RecordWaterLevel_WhenLevelIsHigher_UpdatesPeak()
    {
        var floodEvent = NewSuspectedEvent();

        var result = floodEvent.RecordWaterLevel(25m);

        Assert.True(result.IsSuccess);
        Assert.Equal(25m, floodEvent.PeakWaterLevelCm);
    }

    [Fact]
    public void RecordWaterLevel_WhenLevelIsLower_KeepsPeak()
    {
        var floodEvent = NewSuspectedEvent();

        floodEvent.RecordWaterLevel(25m);
        var result = floodEvent.RecordWaterLevel(15m);

        Assert.True(result.IsSuccess);
        Assert.Equal(25m, floodEvent.PeakWaterLevelCm);
    }
}