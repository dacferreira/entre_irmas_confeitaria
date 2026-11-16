using EntreIrmas.Domain.DTOs.Endereco;

namespace EntreIrmas.Domain.Services;

/// <summary>Consulta de endereço pelo CEP no ViaCEP (implementada em Gateways/EntreIrmas.ViaCepService).</summary>
public interface IViaCepService
{
    /// <summary>
    /// Endereço do CEP (8 dígitos) ou nulo quando o CEP não existe.
    /// Lança <see cref="Exceptions.GatewayException"/> se o ViaCEP estiver fora do ar, lento ou responder algo inesperado.
    /// </summary>
    Task<EnderecoCepDto?> ConsultarAsync(string cep, CancellationToken cancellationToken = default);
}
