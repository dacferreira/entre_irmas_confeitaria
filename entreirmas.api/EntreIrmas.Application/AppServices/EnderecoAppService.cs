using EntreIrmas.Domain.AppServices;
using EntreIrmas.Domain.DTOs.Endereco;
using EntreIrmas.Domain.Exceptions;
using EntreIrmas.Domain.Services;
using EntreIrmas.Domain.Validations;

namespace EntreIrmas.Application.AppServices;

public class EnderecoAppService : IEnderecoAppService
{
    internal const string MensagemNaoEncontrado = "CEP não encontrado. Confira o número ou preencha o endereço.";
    internal const string MensagemIndisponivel = "Não foi possível buscar o CEP agora. Preencha o endereço manualmente.";
    private const int ServicoIndisponivel = 503;

    private readonly IViaCepService _viaCepService;

    public EnderecoAppService(IViaCepService viaCepService)
    {
        _viaCepService = viaCepService;
    }

    public async Task<EnderecoCepDto> ConsultarCepAsync(string cep, CancellationToken cancellationToken = default)
    {
        var digitos = CriarPedidoValidation.SomenteDigitos(cep);
        if (digitos.Length != 8)
            throw new DomainException("Informe um CEP com 8 números.");

        try
        {
            return await _viaCepService.ConsultarAsync(digitos, cancellationToken)
                ?? throw new ObjectNotFoundException(MensagemNaoEncontrado);
        }
        catch (GatewayException)
        {
            // A falha já foi registrada no gateway; aqui só vira uma mensagem que ajuda quem está comprando.
            throw new ApiException(ServicoIndisponivel, MensagemIndisponivel);
        }
    }
}
