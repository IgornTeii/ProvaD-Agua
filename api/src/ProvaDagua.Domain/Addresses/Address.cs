namespace ProvaDagua.Domain.Addresses;

public sealed class Address
{
    public Guid Id { get; private set; }
    public string Street { get; private set; } = default!;
    public string Number { get; private set; } = default!;
    public string Neighborhood { get; private set; } = default!;
    public string City { get; private set; } = default!;
    public string SensorId { get; private set; } = default!;

    public string FullAddress => $"{Street}, {Number} - {Neighborhood}, {City}";

    private Address() { } // EF Core
}