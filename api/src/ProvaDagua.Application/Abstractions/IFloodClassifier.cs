using ProvaDagua.Domain.Readings;

namespace ProvaDagua.Application.Abstractions;

public enum FloodClassification
{
    Flood,
    Noise,
    Inconclusive
}

public interface IFloodClassifier
{
    Task<FloodClassification> ClassifyAsync(IReadOnlyList<Reading> window, CancellationToken ct);
}