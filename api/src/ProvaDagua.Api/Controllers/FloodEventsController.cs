using Microsoft.AspNetCore.Mvc;
using ProvaDagua.Application.Queries;
using ProvaDagua.Domain.FloodEvents;

namespace ProvaDagua.Api.Controllers;

[ApiController]
[Route("flood-events")]
public sealed class FloodEventsController(IReadQueries queries) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<FloodEventDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] string? sensorId,
        [FromQuery] FloodEventStatus? status,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] string? street,
        CancellationToken ct) =>
        Ok(await queries.GetFloodEventsAsync(new FloodEventFilter(sensorId, status, from, to, street), ct));

    [HttpGet("{id:guid}")]
    [ProducesResponseType<FloodEventDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct) =>
        await queries.GetFloodEventAsync(id, ct) is { } floodEvent ? Ok(floodEvent) : NotFound();
}
