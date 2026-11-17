using EntreIrmas.Api.Extensions;
using EntreIrmas.Api.Filters;
using EntreIrmas.Domain.AppServices;
using EntreIrmas.Domain.DTOs.Endereco;
using EntreIrmas.Domain.DTOs.Entrega;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EntreIrmas.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/entregas")]
public class EntregasController : ControllerBase
{
    private readonly IEntregaAppService _entregaAppService;

    public EntregasController(IEntregaAppService entregaAppService)
    {
        _entregaAppService = entregaAppService;
    }

    /// <summary>
    /// "Calcular entrega" do checkout: cota na Uber e na 99 e devolve a mais barata. Sem parceiro disponível,
    /// devolve 200 com disponivel = false e a mensagem de taxa combinada pelo WhatsApp.
    /// </summary>
    [HttpPost("cotacao")]
    [EnableRateLimiting(RateLimitExtension.PoliticaEntrega)]
    [ProducesResponseType(typeof(CotacaoEntregaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Cotar([FromBody] EnderecoDto endereco, CancellationToken cancellationToken) =>
        Ok(await _entregaAppService.CotarAsync(endereco, cancellationToken));
}
