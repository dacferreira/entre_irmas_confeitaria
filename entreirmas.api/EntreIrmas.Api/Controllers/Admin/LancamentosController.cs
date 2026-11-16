using EntreIrmas.Api.Filters;
using EntreIrmas.Domain.AppServices;
using EntreIrmas.Domain.DTOs.Lancamento;
using Microsoft.AspNetCore.Mvc;

namespace EntreIrmas.Api.Controllers.Admin;

/// <summary>Despesas e perdas (exige login).</summary>
[ApiController]
[Route("api/admin/lancamentos")]
public class LancamentosController : ControllerBase
{
    private readonly ILancamentoAppService _lancamentoAppService;

    public LancamentosController(ILancamentoAppService lancamentoAppService)
    {
        _lancamentoAppService = lancamentoAppService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<LancamentoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] FiltroLancamentoDto filtro, CancellationToken cancellationToken) =>
        Ok(await _lancamentoAppService.ListarAsync(filtro, cancellationToken));

    [HttpPost]
    [ProducesResponseType(typeof(LancamentoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] SalvarLancamentoDto dto, CancellationToken cancellationToken) =>
        StatusCode(StatusCodes.Status201Created, await _lancamentoAppService.CriarAsync(dto, cancellationToken));

    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(LancamentoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(long id, [FromBody] SalvarLancamentoDto dto, CancellationToken cancellationToken) =>
        Ok(await _lancamentoAppService.AtualizarAsync(id, dto, cancellationToken));

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(long id, CancellationToken cancellationToken)
    {
        await _lancamentoAppService.ExcluirAsync(id, cancellationToken);
        return NoContent();
    }
}
