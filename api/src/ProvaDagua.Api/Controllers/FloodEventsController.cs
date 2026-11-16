using Microsoft.AspNetCore.Mvc;
using ProvaDagua.Application.Queries;
using ProvaDagua.Domain.FloodEvents;

namespace ProvaDagua.Api.Controllers;

[ApiController]
[Route("flood-events")]
public sealed class FloodEventsController(IReadQueries queries) : ControllerBase
{
    /// <summary>Lista eventos de alagamento, com filtro opcional por sensor e status.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<FloodEventDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] string? sensorId, [FromQuery] FloodEventStatus? status, CancellationToken ct) =>
        Ok(await queries.GetFloodEventsAsync(sensorId, status, ct));
}