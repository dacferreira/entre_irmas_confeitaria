using System.Net;
using System.Net.Http.Json;
using EntreIrmas.Domain.DTOs.Endereco;
using EntreIrmas.Test.Config;
using FluentAssertions;

namespace EntreIrmas.Test.Tests.Api;

[Collection(ApiCollection.Nome)]
public class EnderecosControllerTests
{
    private readonly HttpClient _cliente;

    public EnderecosControllerTests(ApiFixture fixture)
    {
        _cliente = fixture.Cliente;
    }

    [Fact]
    public async Task GetCep_Existente_SemLogin_RetornaOEndereco()
    {
        var endereco = await _cliente.GetFromJsonAsync<EnderecoCepDto>("/api/enderecos/cep/01001-000");

        endereco!.Logradouro.Should().Be("Praça da Sé");
        endereco.Cidade.Should().Be("São Paulo");
        endereco.Uf.Should().Be("SP");
    }

    [Theory]
    [InlineData(ViaCepFalso.CepInexistente, HttpStatusCode.NotFound, "CEP não encontrado")]
    [InlineData(ViaCepFalso.CepComFalha, HttpStatusCode.ServiceUnavailable, "Preencha o endereço manualmente")]
    [InlineData("123", HttpStatusCode.BadRequest, "8 números")]
    public async Task GetCep_ComProblema_RespondeComMensagemClara(string cep, HttpStatusCode status, string mensagem)
    {
        var resposta = await _cliente.GetAsync($"/api/enderecos/cep/{cep}");

        resposta.StatusCode.Should().Be(status);
        (await resposta.Content.ReadAsStringAsync()).Should().Contain(mensagem);
    }
}
