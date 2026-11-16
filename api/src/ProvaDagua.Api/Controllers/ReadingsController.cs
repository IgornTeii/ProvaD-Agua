using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ProvaDagua.Api.Auth;
using ProvaDagua.Application.Readings;

namespace ProvaDagua.Api.Controllers;

[ApiController]
[Route("readings")]
public sealed class ReadingsController(
    RegisterReadingHandler handler,
    IOptions<SensorAuthOptions> sensorAuth) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Register(
        [FromHeader(Name = "X-Sensor-Key")] string? sensorKey,
        [FromBody] RegisterReadingRequest request,
        CancellationToken ct)
    {
        if (sensorKey is null || !sensorAuth.Value.Keys.TryGetValue(sensorKey, out var sensorId))
            return Unauthorized();

        var command = new RegisterReadingCommand(
            sensorId, request.WaterLevelCm, request.RainfallMmPerHour, request.MeasuredAt);

        var result = await handler.HandleAsync(command, ct);

        return result.IsSuccess
            ? Accepted()
            : Problem(statusCode: StatusCodes.Status400BadRequest,
                    title: result.Error!.Code, detail: result.Error.Message);
    }
}

public sealed record RegisterReadingRequest(decimal WaterLevelCm, decimal RainfallMmPerHour, DateTimeOffset MeasuredAt);