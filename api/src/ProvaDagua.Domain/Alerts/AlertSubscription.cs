using ProvaDagua.Domain.Common;

namespace ProvaDagua.Domain.Alerts;

public sealed class AlertSubscription
{
    public Guid Id { get; private set; }
    public Guid AddressId { get; private set; }
    public string Phone { get; private set; } = default!;
    public DateTimeOffset CreatedAt { get; private set; }

    private AlertSubscription() { }

    public static Result<AlertSubscription> Create(Guid addressId, string? phone, DateTimeOffset now)
    {
        if (addressId == Guid.Empty)
            return Result<AlertSubscription>.Failure(AlertErrors.AddressRequired);

        var digits = new string((phone ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digits.Length is < 10 or > 11)
            return Result<AlertSubscription>.Failure(AlertErrors.InvalidPhone);

        return Result<AlertSubscription>.Success(new AlertSubscription
        {
            Id = Guid.NewGuid(),
            AddressId = addressId,
            Phone = digits,
            CreatedAt = now
        });
    }
}
