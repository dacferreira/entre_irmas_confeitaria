using EntreIrmas.Api.Extensions;
using EntreIrmas.Api.Filters;
using EntreIrmas.Domain.AppServices;
using EntreIrmas.Domain.DTOs.Pedido;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EntreIrmas.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/pedidos")]
public class PedidosController : ControllerBase
{
    private readonly IPedidoAppService _pedidoAppService;

    public PedidosController(IPedidoAppService pedidoAppService)
    {
        _pedidoAppService = pedidoAppService;
    }

    /// <summary>
    /// Registra o pedido e devolve o código e o link do WhatsApp com a mensagem pronta.
    /// Os preços são sempre os do cardápio cadastrado.
    /// </summary>
    [HttpPost]
    [EnableRateLimiting(RateLimitExtension.PoliticaPedidos)]
    [ProducesResponseType(typeof(PedidoCriadoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Criar([FromBody] CriarPedidoDto dto, CancellationToken cancellationToken)
    {
        var pedido = await _pedidoAppService.CriarAsync(dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, pedido);
    }

    /// <summary>
    /// Etapa do pedido para o cliente acompanhar pelo código (com ou sem o "EI-").
    /// Não devolve telefone, endereço nem observação.
    /// </summary>
    [HttpGet("{codigo}")]
    [EnableRateLimiting(RateLimitExtension.PoliticaAcompanhamento)]
    [ProducesResponseType(typeof(AcompanhamentoPedidoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Acompanhar(string codigo, CancellationToken cancellationToken) =>
        Ok(await _pedidoAppService.AcompanharAsync(codigo, cancellationToken));
}
