using Microsoft.EntityFrameworkCore;
using ProvaDagua.Application.Abstractions;
using ProvaDagua.Domain.Alerts;

namespace ProvaDagua.Infrastructure.Persistence.Repositories;

internal sealed class AlertRepository(AppDbContext db) : IAlertRepository
{
    public Task<bool> IsSubscribedAsync(Guid addressId, string phone, CancellationToken ct) =>
        db.AlertSubscriptions.AnyAsync(s => s.AddressId == addressId && s.Phone == phone, ct);

    public async Task<IReadOnlyList<Subscriber>> GetSubscribersBySensorAsync(string sensorId, CancellationToken ct)
    {
        var rows = await (
            from subscription in db.AlertSubscriptions
            join address in db.Addresses on subscription.AddressId equals address.Id
            where address.SensorId == sensorId
            select new { subscription, address.Street, address.Number }).ToListAsync(ct);

        return rows.Select(r => new Subscriber(r.subscription, $"{r.Street}, {r.Number}")).ToList();
    }

    public void Add(AlertSubscription subscription) => db.AlertSubscriptions.Add(subscription);

    public void Add(AlertNotification notification) => db.AlertNotifications.Add(notification);
}
