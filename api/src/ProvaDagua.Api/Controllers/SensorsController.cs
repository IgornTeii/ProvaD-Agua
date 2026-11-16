using Microsoft.AspNetCore.Mvc;
using ProvaDagua.Application.Queries;

namespace ProvaDagua.Api.Controllers;

[ApiController]
[Route("sensors")]
public sealed class SensorsController(IReadQueries queries) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<SensorMapDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct) =>
        Ok(await queries.GetSensorsAsync(ct));

    [HttpGet("{sensorId}/status")]
    [ProducesResponseType<SensorStatusDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatus(string sensorId, CancellationToken ct) =>
        Ok(await queries.GetSensorStatusAsync(sensorId, ct));
}
