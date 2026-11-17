using EntreIrmas.Api.Filters;
using EntreIrmas.Domain.AppServices;
using EntreIrmas.Domain.DTOs.Comum;
using EntreIrmas.Domain.DTOs.Pedido;
using Microsoft.AspNetCore.Mvc;

namespace EntreIrmas.Api.Controllers.Admin;

/// <summary>Gestão dos pedidos (exige login).</summary>
[ApiController]
[Route("api/admin/pedidos")]
public class AdminPedidosController : ControllerBase
{
    private readonly IPedidoAppService _pedidoAppService;

    public AdminPedidosController(IPedidoAppService pedidoAppService)
    {
        _pedidoAppService = pedidoAppService;
    }

    /// <summary>Lista paginada, filtrada por data desejada, status e busca (código, nome ou telefone).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<PedidoResumoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] FiltroPedidoDto filtro, CancellationToken cancellationToken) =>
        Ok(await _pedidoAppService.ListarAsync(filtro, cancellationToken));

    /// <summary>
    /// Último pedido feito pelo site (204 se não houver). A área administrativa consulta de tempos em tempos
    /// e toca o aviso quando o id muda.
    /// </summary>
    [HttpGet("ultimo-do-site")]
    [ProducesResponseType(typeof(UltimoPedidoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UltimoDoSite(CancellationToken cancellationToken) =>
        await _pedidoAppService.ObterUltimoDoSiteAsync(cancellationToken) is { } ultimo ? Ok(ultimo) : NoContent();

    /// <summary>
    /// Pedidos do site com id maior que <paramref name="depoisDe"/> (do mais antigo para o mais novo, até 20),
    /// com os itens: a área administrativa avisa e imprime o recibo de cada um.
    /// </summary>
    [HttpGet("novos-do-site")]
    [ProducesResponseType(typeof(IReadOnlyList<PedidoDetalheDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> NovosDoSite([FromQuery] long depoisDe, CancellationToken cancellationToken) =>
        Ok(await _pedidoAppService.ListarNovosDoSiteAsync(depoisDe, cancellationToken));

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(PedidoDetalheDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obter(long id, CancellationToken cancellationToken) =>
        Ok(await _pedidoAppService.ObterAsync(id, cancellationToken));

    /// <summary>Registra um pedido recebido fora do site. Entra como confirmado e sem a regra de antecedência.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PedidoDetalheDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CriarPedidoDto dto, CancellationToken cancellationToken) =>
        StatusCode(StatusCodes.Status201Created, await _pedidoAppService.CriarManualAsync(dto, cancellationToken));

    [HttpPatch("{id:long}/status")]
    [ProducesResponseType(typeof(PedidoDetalheDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AlterarStatus(long id, [FromBody] AlterarStatusPedidoDto dto, CancellationToken cancellationToken) =>
        Ok(await _pedidoAppService.AlterarStatusAsync(id, dto.Status, cancellationToken));

    /// <summary>
    /// Chama de novo o entregador do parceiro (quando a chamada automática do "Pronto" falhou). O resultado
    /// (chamado ou o motivo da falha) volta no próprio pedido.
    /// </summary>
    [HttpPost("{id:long}/entregador")]
    [ProducesResponseType(typeof(PedidoDetalheDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChamarEntregador(long id, CancellationToken cancellationToken) =>
        Ok(await _pedidoAppService.ChamarEntregadorAsync(id, cancellationToken));

    /// <summary>Exclusão lógica (para pedido lançado por engano). Para desistência, use o status Cancelado.</summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(long id, CancellationToken cancellationToken)
    {
        await _pedidoAppService.ExcluirAsync(id, cancellationToken);
        return NoContent();
    }
}
