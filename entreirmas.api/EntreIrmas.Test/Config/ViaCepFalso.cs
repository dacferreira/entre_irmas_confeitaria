using EntreIrmas.Domain.DTOs.Endereco;
using EntreIrmas.Domain.Exceptions;
using EntreIrmas.Domain.Services;

namespace EntreIrmas.Test.Config;

/// <summary>ViaCEP de mentira para os testes de integração (nenhum teste sai para a internet).</summary>
internal sealed class ViaCepFalso : IViaCepService
{
    public const string CepExistente = "01001000";
    public const string CepInexistente = "99999999";
    public const string CepComFalha = "00000000";

    public Task<EnderecoCepDto?> ConsultarAsync(string cep, CancellationToken cancellationToken = default) => cep switch
    {
        CepExistente => Task.FromResult<EnderecoCepDto?>(new EnderecoCepDto
        {
            Cep = CepExistente,
            Logradouro = "Praça da Sé",
            Complemento = "lado ímpar",
            Bairro = "Sé",
            Cidade = "São Paulo",
            Uf = "SP"
        }),
        CepComFalha => throw new GatewayException("ViaCEP", "fora do ar"),
        _ => Task.FromResult<EnderecoCepDto?>(null)
    };
}
