using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EntreIrmas.Domain.DTOs.Entrega;
using EntreIrmas.Domain.Entities.PedidoAggregate;
using EntreIrmas.Domain.EnviromentVariables;
using EntreIrmas.Domain.Exceptions;
using EntreIrmas.Domain.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EntreIrmas.UberDirectService;

/// <summary>
/// Uber Direct (https://developer.uber.com/docs/deliveries): token OAuth (client credentials, escopo
/// eats.deliveries, válido por 30 dias e guardado em memória), cotação em customers/{id}/delivery_quotes e
/// entrega em customers/{id}/deliveries. Valores em centavos; endereços como JSON em texto, no formato da Uber.
/// Falha de rede, timeout ou resposta inesperada viram <see cref="GatewayException"/>.
/// </summary>
public class UberDirectService : IEntregaParceiroService
{
    public const string NomeDoCliente = "UberDirect";
    public const string NomeDoClienteDeAutenticacao = "UberDirectAuth";
    public const string NomeDoSistema = "Uber";

    internal const string EscopoDeEntregas = "eats.deliveries";
    internal const string ChaveDoToken = "uberdirect:token";

    /// <summary>Erros da Uber que significam "não entregamos nesse endereço" (não é falha do sistema).</summary>
    internal static readonly HashSet<string> CodigosDeForaDaArea = new(StringComparer.OrdinalIgnoreCase)
    {
        "address_undeliverable", "address_undeliverable_limited_couriers", "unknown_location", "pickup_window_too_small"
    };

    private static readonly JsonSerializerOptions OpcoesJson = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly IUberEnvironmentVariables _uber;
    private readonly IEntregaEnvironmentVariables _loja;
    private readonly IWhatsAppEnvironmentVariables _whatsApp;
    private readonly ILogger<UberDirectService> _logger;

    public UberDirectService(IHttpClientFactory httpClientFactory, IMemoryCache cache, IUberEnvironmentVariables uber,
        IEntregaEnvironmentVariables loja, IWhatsAppEnvironmentVariables whatsApp, ILogger<UberDirectService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _uber = uber;
        _loja = loja;
        _whatsApp = whatsApp;
        _logger = logger;
    }

    public ParceiroEntrega Parceiro => ParceiroEntrega.Uber;

    public async Task<CotacaoParceiro?> CotarAsync(EnderecoEntrega destino, CancellationToken cancellationToken = default)
    {
        var corpo = new SolicitacaoDeCotacao
        {
            PickupAddress = EnderecoDaLoja(),
            DropoffAddress = EnderecoDoCliente(destino),
            PickupLatitude = _loja.LOJA_LATITUDE,
            PickupLongitude = _loja.LOJA_LONGITUDE,
            PickupPhoneNumber = TelefoneInternacional(_whatsApp.WHATSAPP_NUMERO)
        };

        var resposta = await EnviarAsync<RespostaDeCotacao>($"customers/{_uber.UBER_CUSTOMER_ID}/delivery_quotes", corpo,
            "calcular a entrega", aceitaForaDaArea: true, cancellationToken);
        if (resposta is null)
            return null;

        return new CotacaoParceiro(Parceiro, resposta.Fee / 100m, Math.Max(1, resposta.Duration));
    }

    public async Task<EntregaSolicitada> SolicitarAsync(Pedido pedido, CancellationToken cancellationToken = default)
    {
        var destino = pedido.EnderecoEntrega
                      ?? throw new DomainException("O pedido não tem endereço de entrega.");

        var corpo = new SolicitacaoDeEntrega
        {
            PickupName = _loja.LOJA_NOME,
            PickupAddress = EnderecoDaLoja(),
            PickupPhoneNumber = TelefoneInternacional(_whatsApp.WHATSAPP_NUMERO),
            PickupLatitude = _loja.LOJA_LATITUDE,
            PickupLongitude = _loja.LOJA_LONGITUDE,
            DropoffName = pedido.NomeCliente,
            DropoffAddress = EnderecoDoCliente(destino),
            DropoffPhoneNumber = TelefoneInternacional(pedido.Telefone),
            DropoffNotes = destino.Complemento,
            ManifestReference = pedido.Codigo,
            ManifestItems = pedido.Itens
                .Select(i => new ItemDoPacote { Name = i.NomeProduto, Quantity = i.Quantidade, Size = "small" })
                .ToList(),
            TestSpecifications = _uber.UBER_MODO_TESTE
                ? new EspecificacaoDeTeste { RoboCourierSpecification = new EntregadorRobo { Mode = "auto" } }
                : null
        };

        var resposta = await EnviarAsync<RespostaDeEntrega>($"customers/{_uber.UBER_CUSTOMER_ID}/deliveries", corpo,
                           "chamar o entregador", aceitaForaDaArea: false, cancellationToken)
                       ?? throw new GatewayException(NomeDoSistema, "A Uber não atende este endereço.");

        if (string.IsNullOrWhiteSpace(resposta.Id))
            throw new GatewayException(NomeDoSistema, "A Uber não devolveu o id da entrega.");

        return new EntregaSolicitada(resposta.Id, resposta.TrackingUrl);
    }

    /// <summary>
    /// POST com o token. Endereço fora da área (quando aceito) vira nulo; o resto que não for sucesso vira
    /// <see cref="GatewayException"/>. Token recusado (401) é descartado para a próxima chamada pedir outro.
    /// </summary>
    private async Task<T?> EnviarAsync<T>(string caminho, object corpo, string acao, bool aceitaForaDaArea,
        CancellationToken cancellationToken) where T : class
    {
        try
        {
            var token = await ObterTokenAsync(cancellationToken);
            var cliente = _httpClientFactory.CreateClient(NomeDoCliente);
            using var mensagem = new HttpRequestMessage(HttpMethod.Post, caminho)
            {
                Content = JsonContent.Create(corpo, options: OpcoesJson)
            };
            mensagem.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var resposta = await cliente.SendAsync(mensagem, cancellationToken);
            if (resposta.IsSuccessStatusCode)
                return await resposta.Content.ReadFromJsonAsync<T>(OpcoesJson, cancellationToken)
                       ?? throw new GatewayException(NomeDoSistema, $"A Uber respondeu vazio ao {acao}.");

            var erro = await LerErroAsync(resposta, cancellationToken);
            if (resposta.StatusCode == HttpStatusCode.Unauthorized)
                _cache.Remove(ChaveDoToken);

            if (aceitaForaDaArea && erro?.Code is { } codigo && CodigosDeForaDaArea.Contains(codigo))
                return null;

            _logger.LogWarning("Uber Direct recusou {Acao}: {Status} {Codigo} {Mensagem}", acao,
                (int)resposta.StatusCode, erro?.Code, erro?.Message);
            throw new GatewayException(NomeDoSistema, $"A Uber não conseguiu {acao} ({(int)resposta.StatusCode}).");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Cancelado sem pedido de quem chamou: foi o timeout do HttpClient.
            _logger.LogWarning("Uber Direct não respondeu a tempo ao {Acao}", acao);
            throw new GatewayException(NomeDoSistema, "A Uber demorou para responder.");
        }
        catch (Exception erro) when (erro is HttpRequestException or JsonException or NotSupportedException)
        {
            _logger.LogWarning(erro, "Falha ao {Acao} na Uber Direct", acao);
            throw new GatewayException(NomeDoSistema, $"Não foi possível {acao} na Uber.", erro);
        }
    }

    private async Task<string> ObterTokenAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(ChaveDoToken, out string? guardado) && !string.IsNullOrEmpty(guardado))
            return guardado;

        var cliente = _httpClientFactory.CreateClient(NomeDoClienteDeAutenticacao);
        using var resposta = await cliente.PostAsync(_uber.UBER_AUTH_URL, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = _uber.UBER_CLIENT_ID,
            ["client_secret"] = _uber.UBER_CLIENT_SECRET,
            ["grant_type"] = "client_credentials",
            ["scope"] = EscopoDeEntregas
        }), cancellationToken);

        if (!resposta.IsSuccessStatusCode)
        {
            _logger.LogWarning("Uber Direct recusou as credenciais: {Status}", (int)resposta.StatusCode);
            throw new GatewayException(NomeDoSistema, "A Uber recusou as credenciais da API. Confira UBER_CLIENT_ID e UBER_CLIENT_SECRET.");
        }

        var token = await resposta.Content.ReadFromJsonAsync<RespostaDeToken>(OpcoesJson, cancellationToken);
        if (string.IsNullOrWhiteSpace(token?.AccessToken))
            throw new GatewayException(NomeDoSistema, "A Uber não devolveu o token da API.");

        // O token vale 30 dias; renova um dia antes para não usar um vencido no meio de um pedido.
        var validade = TimeSpan.FromSeconds(Math.Max(60, token.ExpiresIn - 86_400));
        _cache.Set(ChaveDoToken, token.AccessToken, validade);
        return token.AccessToken;
    }

    private static async Task<ErroDaUber?> LerErroAsync(HttpResponseMessage resposta, CancellationToken cancellationToken)
    {
        try
        {
            return await resposta.Content.ReadFromJsonAsync<ErroDaUber>(OpcoesJson, cancellationToken);
        }
        catch (Exception erro) when (erro is JsonException or NotSupportedException)
        {
            return null;
        }
    }

    private string EnderecoDaLoja() =>
        EnderecoDaUber([_loja.LOJA_LOGRADOURO, _loja.LOJA_BAIRRO], _loja.LOJA_CIDADE, _loja.LOJA_UF, _loja.LOJA_CEP);

    internal static string EnderecoDoCliente(EnderecoEntrega destino) =>
        EnderecoDaUber(
            [$"{destino.Logradouro}, {destino.Numero}", string.Join(" - ", new[] { destino.Complemento, destino.Bairro }.Where(p => !string.IsNullOrWhiteSpace(p)))],
            destino.Cidade, destino.Uf, destino.Cep);

    /// <summary>A Uber recebe o endereço como um JSON dentro de um texto.</summary>
    internal static string EnderecoDaUber(string[] linhas, string cidade, string uf, string cep)
    {
        var digitos = new string((cep ?? string.Empty).Where(char.IsAsciiDigit).ToArray());
        return JsonSerializer.Serialize(new EnderecoUber
        {
            StreetAddress = linhas.Where(l => !string.IsNullOrWhiteSpace(l)).ToArray(),
            City = cidade,
            State = uf,
            ZipCode = digitos.Length == 8 ? $"{digitos[..5]}-{digitos[5..]}" : digitos,
            Country = "BR"
        });
    }

    /// <summary>"21999998888" vira "+5521999998888" (formato E.164 pedido pela Uber).</summary>
    internal static string TelefoneInternacional(string telefone)
    {
        var digitos = new string((telefone ?? string.Empty).Where(char.IsAsciiDigit).ToArray());
        return digitos.StartsWith("55", StringComparison.Ordinal) && digitos.Length > 11
            ? $"+{digitos}"
            : $"+55{digitos}";
    }

    // ---------- Formato da API da Uber ----------

    internal sealed class EnderecoUber
    {
        [JsonPropertyName("street_address")] public string[] StreetAddress { get; set; } = [];
        [JsonPropertyName("city")] public string City { get; set; } = string.Empty;
        [JsonPropertyName("state")] public string State { get; set; } = string.Empty;
        [JsonPropertyName("zip_code")] public string ZipCode { get; set; } = string.Empty;
        [JsonPropertyName("country")] public string Country { get; set; } = string.Empty;
    }

    internal sealed class SolicitacaoDeCotacao
    {
        [JsonPropertyName("pickup_address")] public string PickupAddress { get; set; } = string.Empty;
        [JsonPropertyName("dropoff_address")] public string DropoffAddress { get; set; } = string.Empty;
        [JsonPropertyName("pickup_latitude")] public double PickupLatitude { get; set; }
        [JsonPropertyName("pickup_longitude")] public double PickupLongitude { get; set; }
        [JsonPropertyName("pickup_phone_number")] public string? PickupPhoneNumber { get; set; }
    }

    internal sealed class RespostaDeCotacao
    {
        [JsonPropertyName("id")] public string? Id { get; set; }

        /// <summary>Em centavos.</summary>
        [JsonPropertyName("fee")] public long Fee { get; set; }

        [JsonPropertyName("currency_type")] public string? CurrencyType { get; set; }

        /// <summary>Minutos até a entrega.</summary>
        [JsonPropertyName("duration")] public int Duration { get; set; }
    }

    internal sealed class SolicitacaoDeEntrega
    {
        [JsonPropertyName("pickup_name")] public string PickupName { get; set; } = string.Empty;
        [JsonPropertyName("pickup_address")] public string PickupAddress { get; set; } = string.Empty;
        [JsonPropertyName("pickup_phone_number")] public string PickupPhoneNumber { get; set; } = string.Empty;
        [JsonPropertyName("pickup_latitude")] public double PickupLatitude { get; set; }
        [JsonPropertyName("pickup_longitude")] public double PickupLongitude { get; set; }
        [JsonPropertyName("dropoff_name")] public string DropoffName { get; set; } = string.Empty;
        [JsonPropertyName("dropoff_address")] public string DropoffAddress { get; set; } = string.Empty;
        [JsonPropertyName("dropoff_phone_number")] public string DropoffPhoneNumber { get; set; } = string.Empty;
        [JsonPropertyName("dropoff_notes")] public string? DropoffNotes { get; set; }
        [JsonPropertyName("manifest_reference")] public string? ManifestReference { get; set; }
        [JsonPropertyName("manifest_items")] public List<ItemDoPacote> ManifestItems { get; set; } = new();
        [JsonPropertyName("test_specifications")] public EspecificacaoDeTeste? TestSpecifications { get; set; }
    }

    internal sealed class ItemDoPacote
    {
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("quantity")] public int Quantity { get; set; }
        [JsonPropertyName("size")] public string Size { get; set; } = "small";
    }

    internal sealed class EspecificacaoDeTeste
    {
        [JsonPropertyName("robo_courier_specification")] public EntregadorRobo? RoboCourierSpecification { get; set; }
    }

    internal sealed class EntregadorRobo
    {
        [JsonPropertyName("mode")] public string Mode { get; set; } = "auto";
    }

    internal sealed class RespostaDeEntrega
    {
        [JsonPropertyName("id")] public string? Id { get; set; }
        [JsonPropertyName("tracking_url")] public string? TrackingUrl { get; set; }
        [JsonPropertyName("status")] public string? Status { get; set; }
    }

    internal sealed class RespostaDeToken
    {
        [JsonPropertyName("access_token")] public string? AccessToken { get; set; }
        [JsonPropertyName("expires_in")] public long ExpiresIn { get; set; }
    }

    internal sealed class ErroDaUber
    {
        [JsonPropertyName("code")] public string? Code { get; set; }
        [JsonPropertyName("message")] public string? Message { get; set; }
    }
}
