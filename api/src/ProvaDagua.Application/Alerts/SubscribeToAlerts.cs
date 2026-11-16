using ProvaDagua.Application.Abstractions;
using ProvaDagua.Domain.Alerts;
using ProvaDagua.Domain.Common;

namespace ProvaDagua.Application.Alerts;

public sealed record SubscribeToAlertsCommand(Guid AddressId, string? Phone);

public sealed class SubscribeToAlertsHandler(
    IAddressRepository addresses,
    IAlertRepository alerts,
    IUnitOfWork unitOfWork,
    TimeProvider clock)
{
    public async Task<Result> HandleAsync(SubscribeToAlertsCommand command, CancellationToken ct)
    {
        var subscription = AlertSubscription.Create(command.AddressId, command.Phone, clock.GetUtcNow());
        if (!subscription.IsSuccess)
            return Result.Failure(subscription.Error!);

        if (await addresses.GetByIdAsync(command.AddressId, ct) is null)
            return Result.Failure(AlertErrors.AddressNotFound);

        if (await alerts.IsSubscribedAsync(command.AddressId, subscription.Value!.Phone, ct))
            return Result.Success();

        alerts.Add(subscription.Value!);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
