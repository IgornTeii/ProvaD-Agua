namespace ProvaDagua.Domain.Alerts;

public sealed class AlertNotification
{
    public Guid Id { get; private set; }
    public Guid SubscriptionId { get; private set; }
    public Guid AddressId { get; private set; }
    public Guid FloodEventId { get; private set; }
    public string Phone { get; private set; } = default!;
    public string Message { get; private set; } = default!;
    public DateTimeOffset SentAt { get; private set; }

    private AlertNotification() { }

    public static AlertNotification FloodConfirmed(
        AlertSubscription subscription, string street, Guid floodEventId, DateTimeOffset sentAt) => new()
    {
        Id = Guid.NewGuid(),
        SubscriptionId = subscription.Id,
        AddressId = subscription.AddressId,
        FloodEventId = floodEventId,
        Phone = subscription.Phone,
        Message = $"Prova d'Água: alagamento confirmado na {street}. Evite sair e proteja seus bens. " +
                  "Sua comprovação sai automaticamente quando a água baixar.",
        SentAt = sentAt
    };
}
