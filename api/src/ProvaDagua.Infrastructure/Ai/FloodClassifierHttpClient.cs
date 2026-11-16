using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ProvaDagua.Application.Abstractions;
using ProvaDagua.Domain.Readings;

namespace ProvaDagua.Infrastructure.Ai;

internal sealed class FloodClassifierHttpClient(
    HttpClient http,
    ILogger<FloodClassifierHttpClient> logger) : IFloodClassifier
{
    public async Task<FloodClassification> ClassifyAsync(IReadOnlyList<Reading> window, CancellationToken ct)
    {
        var request = new ClassifyRequest(window
            .Select(r => new ClassifyReading(r.WaterLevelCm, r.RainfallMmPerHour))
            .ToList());

        try
        {
            using var response = await http.PostAsJsonAsync("/classify", request, ct);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<ClassifyResponse>(ct);
            return body?.Label switch
            {
                "flood" => FloodClassification.Flood,
                "noise" => FloodClassification.Noise,
                _ => FloodClassification.Inconclusive
            };
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException
                                   || (ex is TaskCanceledException && !ct.IsCancellationRequested))
        {
            logger.LogWarning(ex, "Serviço de IA indisponível. Os eventos continuam Suspeitos.");
            return FloodClassification.Inconclusive;
        }
    }

    private sealed record ClassifyReading(decimal WaterLevelCm, decimal RainfallMmPerHour);
    private sealed record ClassifyRequest(IReadOnlyList<ClassifyReading> Readings);
    private sealed record ClassifyResponse(string Label, double? Confidence);
}