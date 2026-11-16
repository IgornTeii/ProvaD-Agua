using Microsoft.AspNetCore.Mvc;
using ProvaDagua.Application.Queries;

namespace ProvaDagua.Api.Controllers;

[ApiController]
[Route("certificates")]
public sealed class CertificatesController(IReadQueries queries) : ControllerBase
{
    /// <summary>Comprovações emitidas para um endereço.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CertificateDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListByAddress([FromQuery] Guid addressId, CancellationToken ct) =>
        Ok(await queries.GetCertificatesByAddressAsync(addressId, ct));

    /// <summary>Verifica a autenticidade de uma comprovação pelo código (ex.: PDA-7K3M-9XQ2).</summary>
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