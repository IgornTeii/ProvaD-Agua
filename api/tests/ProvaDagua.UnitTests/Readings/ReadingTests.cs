using ProvaDagua.Domain.Readings;

namespace ProvaDagua.UnitTests.Readings;

public class ReadingTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 15, 18, 0, 0, TimeSpan.FromHours(-3));

    [Fact]
    public void Create_WhenWaterLevelIsNegative_ReturnsInvalidWaterLevelFailure()
    {
        var result = Reading.Create("sensor-01", -1m, 0m, Now, Now);

        Assert.False(result.IsSuccess);
        Assert.Equal(ReadingErrors.InvalidWaterLevel, result.Error);
    }
}
