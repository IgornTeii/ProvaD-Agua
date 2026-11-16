using ProvaDagua.Domain.Alerts;

namespace ProvaDagua.Application.Abstractions;

public sealed record Subscriber(AlertSubscription Subscription, string Street);

public interface IAlertRepository
{
    Task<bool> IsSubscribedAsync(Guid addressId, string phone, CancellationToken ct);
    Task<IReadOnlyList<Subscriber>> GetSubscribersBySensorAsync(string sensorId, CancellationToken ct);
    void Add(AlertSubscription subscription);
    void Add(AlertNotification notification);
}
