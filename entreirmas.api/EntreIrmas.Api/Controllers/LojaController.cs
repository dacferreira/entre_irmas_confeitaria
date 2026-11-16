using EntreIrmas.Domain.AppServices;
using EntreIrmas.Domain.DTOs.Loja;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EntreIrmas.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/loja")]
public class LojaController : ControllerBase
{
    private readonly ILojaAppService _lojaAppService;

    public LojaController(ILojaAppService lojaAppService)
    {
        _lojaAppService = lojaAppService;
    }

    /// <summary>Se a loja está aberta e o aviso de fechado. Fechada, o site só aceita encomendas.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(SituacaoLojaDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Obter(CancellationToken cancellationToken) =>
        Ok(await _lojaAppService.ObterSituacaoAsync(cancellationToken));
}
