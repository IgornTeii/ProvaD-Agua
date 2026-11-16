namespace ProvaDagua.Domain.Sensors;

public sealed class Sensor
{
    public string Id { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string Neighborhood { get; private set; } = default!;
    public string Zone { get; private set; } = default!;
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }

    private Sensor() { }
}
