using EntreIrmas.Domain.DTOs.Endereco;

namespace EntreIrmas.Domain.AppServices;

public interface IEnderecoAppService
{
    /// <summary>
    /// Endereço do CEP (com ou sem máscara). Lança ObjectNotFoundException se o CEP não existir e
    /// ApiException 503 se o serviço de CEP estiver indisponível.
    /// </summary>
    Task<EnderecoCepDto> ConsultarCepAsync(string cep, CancellationToken cancellationToken = default);
}
