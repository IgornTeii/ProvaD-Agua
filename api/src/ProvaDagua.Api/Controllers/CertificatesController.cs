using Microsoft.AspNetCore.Mvc;
using ProvaDagua.Application.Queries;

namespace ProvaDagua.Api.Controllers;

[ApiController]
[Route("certificates")]
public sealed class CertificatesController(IReadQueries queries) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CertificateDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery] Guid? addressId, [FromQuery] Guid? floodEventId, CancellationToken ct)
    {
        if (addressId is null && floodEventId is null)
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                           title: "certificate.filter_required",
                           detail: "Informe o endereço ou o alagamento.");

        return Ok(await queries.GetCertificatesAsync(addressId, floodEventId, ct));
    }

    [HttpGet("{code}/verify")]
    [ProducesResponseType<CertificateDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Verify(string code, CancellationToken ct)
    {
        var certificate = await queries.GetCertificateByCodeAsync(code, ct);

        return certificate is null
            ? Problem(statusCode: StatusCodes.Status404NotFound,
                      title: "certificate.not_found",
                      detail: "Nenhuma comprovação encontrada com esse código.")
            : Ok(certificate);
    }
}