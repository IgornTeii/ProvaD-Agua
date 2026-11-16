using Microsoft.AspNetCore.Mvc;
using ProvaDagua.Application.Queries;

namespace ProvaDagua.Api.Controllers;

[ApiController]
[Route("addresses")]
public sealed class AddressesController(IReadQueries queries) : ControllerBase
{
    /// <summary>Busca endereços pelo nome da rua ou do bairro.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AddressDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] string? search, CancellationToken ct) =>
        Ok(await queries.SearchAddressesAsync(search, ct));
}