namespace ProvaDagua.Infrastructure.Persistence.Seed;

internal static class SaoPauloSeed
{
    public sealed record SensorSeed(
        int Number, string Name, string Neighborhood, string Zone,
        double Latitude, double Longitude, string Street, string[] HouseNumbers)
    {
        public string Id => $"sensor-{Number:D2}";
    }

    public static readonly SensorSeed[] Sensors =
    [
        new(1, "Rua Amazonas", "São Miguel Paulista", "Leste", -23.4930, -46.4410, "Rua Amazonas", ["120", "245", "380"]),
        new(2, "Av. Luiz Ignácio de Anhaia Mello", "Vila Prudente", "Leste", -23.5843, -46.5810, "Avenida Luiz Ignácio de Anhaia Mello", ["1200", "1450", "1700"]),
        new(3, "Av. Aricanduva", "Aricanduva", "Leste", -23.5640, -46.5180, "Avenida Aricanduva", ["2100", "2350", "2600"]),
        new(4, "Av. Jacu-Pêssego", "Itaquera", "Leste", -23.5450, -46.4540, "Avenida Jacu-Pêssego", ["800", "1050", "1300"]),
        new(5, "Av. do Estado", "Cambuci", "Centro", -23.5560, -46.6200, "Avenida do Estado", ["4500", "4800", "5100"]),
        new(6, "Av. Pompeia", "Pompeia", "Oeste", -23.5310, -46.6860, "Avenida Pompeia", ["300", "520", "780"]),
        new(7, "Av. Ricardo Jafet", "Ipiranga", "Sul", -23.5900, -46.6095, "Avenida Doutor Ricardo Jafet", ["1000", "1300", "1600"]),
        new(8, "Av. Antônio Munhoz Bonilha", "Freguesia do Ó", "Norte", -23.4970, -46.6960, "Avenida Antônio Munhoz Bonilha", ["200", "450", "700"])
    ];

    public static Guid AddressId(int sensorNumber, int index) =>
        Guid.Parse($"00000000-0000-0000-{sensorNumber:D4}-{index + 1:D12}");
}
