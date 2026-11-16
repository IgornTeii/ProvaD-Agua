using Microsoft.AspNetCore.Mvc;
using ProvaDagua.Application.Alerts;
using ProvaDagua.Domain.Alerts;

namespace ProvaDagua.Api.Controllers;

[ApiController]
[Route("alerts")]
public sealed class AlertsController(SubscribeToAlertsHandler handler) : ControllerBase
{
    [HttpPost("subscriptions")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest request, CancellationToken ct)
    {
        var result = await handler.HandleAsync(new SubscribeToAlertsCommand(request.AddressId, request.Phone), ct);

        if (result.IsSuccess)
            return NoContent();

        var status = result.Error == AlertErrors.AddressNotFound
            ? StatusCodes.Status404NotFound
            : StatusCodes.Status400BadRequest;

        return Problem(statusCode: status, title: result.Error!.Code, detail: result.Error.Message);
    }
}

public sealed record SubscribeRequest(Guid AddressId, string? Phone);
