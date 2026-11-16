using Microsoft.AspNetCore.Mvc;
using ProvaDagua.Application.Queries;

namespace ProvaDagua.Api.Controllers;

[ApiController]
[Route("addresses")]
public sealed class AddressesController(IReadQueries queries) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AddressDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] string? search, CancellationToken ct) =>
        Ok(await queries.SearchAddressesAsync(search, ct));

    [HttpGet("{addressId:guid}/notifications")]
    [ProducesResponseType<IReadOnlyList<AlertNotificationDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Notifications(Guid addressId, CancellationToken ct) =>
        Ok(await queries.GetNotificationsByAddressAsync(addressId, ct));
}
