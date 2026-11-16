using EntreIrmas.Api.Extensions;
using EntreIrmas.Api.Filters;
using EntreIrmas.Domain.AppServices;
using EntreIrmas.Domain.DTOs.Autenticacao;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EntreIrmas.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/autenticacao")]
public class AutenticacaoController : ControllerBase
{
    private readonly IAutenticacaoAppService _autenticacaoAppService;

    public AutenticacaoController(IAutenticacaoAppService autenticacaoAppService)
    {
        _autenticacaoAppService = autenticacaoAppService;
    }

    /// <summary>Entra na área administrativa e devolve o token de acesso.</summary>
    [HttpPost("login")]
    [EnableRateLimiting(RateLimitExtension.PoliticaLogin)]
    [ProducesResponseType(typeof(SessaoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken cancellationToken) =>
        Ok(await _autenticacaoAppService.LoginAsync(dto, cancellationToken));
}
