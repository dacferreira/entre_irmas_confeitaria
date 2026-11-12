using EntreIrmas.Domain.DTOs.Pedido;

namespace EntreIrmas.Domain.AppServices;

public interface IPedidoAppService
{
    Task<PedidoCriadoDto> CriarAsync(CriarPedidoDto dto, CancellationToken cancellationToken = default);
}
