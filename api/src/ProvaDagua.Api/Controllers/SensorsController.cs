using Microsoft.AspNetCore.Mvc;
using ProvaDagua.Application.Queries;

namespace ProvaDagua.Api.Controllers;

[ApiController]
[Route("sensors")]
public sealed class SensorsController(IReadQueries queries) : ControllerBase
{
    /// <summary>Situação agora: última leitura do sensor e o evento aberto, se houver.</summary>
    [HttpGet("{sensorId}/status")]
    [ProducesResponseType<SensorStatusDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatus(string sensorId, CancellationToken ct) =>
        Ok(await queries.GetSensorStatusAsync(sensorId, ct));
}