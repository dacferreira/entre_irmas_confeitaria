using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EntreIrmas.Domain.DTOs.Endereco;
using EntreIrmas.Domain.EnviromentVariables;
using EntreIrmas.Domain.Exceptions;
using EntreIrmas.Domain.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EntreIrmas.ViaCepService;

/// <summary>
/// Consulta o ViaCEP (https://viacep.com.br/ws/{cep}/json/). CEP inexistente volta 200 com {"erro": "true"};
/// CEP mal formatado volta 400. Falha de rede, timeout ou resposta estranha viram <see cref="GatewayException"/>.
/// Os resultados ficam em memória (um CEP não muda de endereço), poupando o serviço gratuito.
/// </summary>
public class ViaCepService : IViaCepService
{
    public const string NomeDoCliente = "ViaCep";
    public const string NomeDoSistema = "ViaCEP";

    /// <summary>CEP inexistente fica menos tempo em memória (pode ser um CEP novo ainda não cadastrado no ViaCEP).</summary>
    internal static readonly TimeSpan TempoDoCacheDeNaoEncontrado = TimeSpan.FromHours(1);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly IViaCepEnvironmentVariables _variaveis;
    private readonly ILogger<ViaCepService> _logger;

    public ViaCepService(IHttpClientFactory httpClientFactory, IMemoryCache cache, IViaCepEnvironmentVariables variaveis,
        ILogger<ViaCepService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _variaveis = variaveis;
        _logger = logger;
    }

    public async Task<EnderecoCepDto?> ConsultarAsync(string cep, CancellationToken cancellationToken = default)
    {
        if (cep.Length != 8 || !cep.All(char.IsAsciiDigit))
            throw new ArgumentException("O CEP deve ter 8 dígitos.", nameof(cep));

        var chave = $"viacep:{cep}";
        if (_cache.TryGetValue(chave, out EnderecoCepDto? guardado))
            return guardado;

        // Falha (GatewayException) não entra no cache: a próxima consulta tenta de novo.
        var endereco = await BuscarAsync(cep, cancellationToken);
        _cache.Set(chave, endereco, endereco is null
            ? TempoDoCacheDeNaoEncontrado
            : TimeSpan.FromHours(Math.Max(1, _variaveis.VIACEP_CACHE_HORAS)));
        return endereco;
    }

    private async Task<EnderecoCepDto?> BuscarAsync(string cep, CancellationToken cancellationToken)
    {
        var cliente = _httpClientFactory.CreateClient(NomeDoCliente);
        RespostaViaCep? resposta;

        try
        {
            using var mensagem = await cliente.GetAsync($"{cep}/json/", cancellationToken);

            if (mensagem.StatusCode == HttpStatusCode.BadRequest)
                return null; // formato recusado pelo ViaCEP: para quem consulta, é um CEP que não existe

            mensagem.EnsureSuccessStatusCode();
            resposta = await mensagem.Content.ReadFromJsonAsync<RespostaViaCep>(cancellationToken);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Cancelado sem pedido de quem chamou: foi o timeout do HttpClient.
            _logger.LogWarning("ViaCEP não respondeu a tempo para o CEP {Cep}", cep);
            throw new GatewayException(NomeDoSistema, "O ViaCEP demorou para responder.");
        }
        catch (Exception erro) when (erro is HttpRequestException or JsonException or NotSupportedException)
        {
            _logger.LogWarning(erro, "Falha ao consultar o CEP {Cep} no ViaCEP", cep);
            throw new GatewayException(NomeDoSistema, "Não foi possível consultar o ViaCEP.", erro);
        }

        if (resposta is null || resposta.Erro)
            return null;

        return new EnderecoCepDto
        {
            Cep = new string((resposta.Cep ?? cep).Where(char.IsAsciiDigit).ToArray()),
            Logradouro = resposta.Logradouro?.Trim() ?? string.Empty,
            Complemento = resposta.Complemento?.Trim() ?? string.Empty,
            Bairro = resposta.Bairro?.Trim() ?? string.Empty,
            Cidade = resposta.Localidade?.Trim() ?? string.Empty,
            Uf = resposta.Uf?.Trim().ToUpperInvariant() ?? string.Empty
        };
    }

    /// <summary>Formato do ViaCEP. "erro" vem como texto ("true") ou booleano, conforme a versão.</summary>
    internal sealed class RespostaViaCep
    {
        [JsonPropertyName("cep")] public string? Cep { get; set; }
        [JsonPropertyName("logradouro")] public string? Logradouro { get; set; }
        [JsonPropertyName("complemento")] public string? Complemento { get; set; }
        [JsonPropertyName("bairro")] public string? Bairro { get; set; }
        [JsonPropertyName("localidade")] public string? Localidade { get; set; }
        [JsonPropertyName("uf")] public string? Uf { get; set; }

        [JsonPropertyName("erro")]
        [JsonConverter(typeof(BooleanoOuTextoConverter))]
        public bool Erro { get; set; }
    }

    /// <summary>Lê true, "true" e "True" como verdadeiro.</summary>
    internal sealed class BooleanoOuTextoConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            reader.TokenType switch
            {
                JsonTokenType.True => true,
                JsonTokenType.False => false,
                JsonTokenType.String => bool.TryParse(reader.GetString(), out var valor) && valor,
                _ => false
            };

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) =>
            writer.WriteBooleanValue(value);
    }
}
