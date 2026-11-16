using EntreIrmas.Api.Filters;
using EntreIrmas.Domain.AppServices;
using EntreIrmas.Domain.DTOs.Painel;
using Microsoft.AspNetCore.Mvc;

namespace EntreIrmas.Api.Controllers.Admin;

/// <summary>Painel de vendas, lucro e perdas (exige login).</summary>
[ApiController]
[Route("api/admin/painel")]
public class PainelController : ControllerBase
{
    private readonly IPainelAppService _painelAppService;

    public PainelController(IPainelAppService painelAppService)
    {
        _painelAppService = painelAppService;
    }

    /// <summary>Resumo do período (datas no formato yyyy-MM-dd). Sem datas, usa o mês corrente.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PainelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Obter([FromQuery] DateOnly? inicio, [FromQuery] DateOnly? fim,
        CancellationToken cancellationToken) =>
        Ok(await _painelAppService.ObterAsync(inicio, fim, cancellationToken));
}
