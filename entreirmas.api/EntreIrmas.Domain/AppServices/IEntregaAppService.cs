using EntreIrmas.Domain.DTOs.Endereco;
using EntreIrmas.Domain.DTOs.Entrega;

namespace EntreIrmas.Domain.AppServices;

public interface IEntregaAppService
{
    /// <summary>"Calcular entrega" do checkout: a cotação mais barata para o endereço (ou o aviso de taxa combinada).</summary>
    Task<CotacaoEntregaDto> CotarAsync(EnderecoDto endereco, CancellationToken cancellationToken = default);
}
