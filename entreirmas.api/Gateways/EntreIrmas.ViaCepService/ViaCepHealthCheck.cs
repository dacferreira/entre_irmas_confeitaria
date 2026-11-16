using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EntreIrmas.ViaCepService;

/// <summary>
/// Consulta um CEP conhecido (Praça da Sé). Registrado como "Degraded": o ViaCEP fora do ar não derruba
/// a API, só impede o preenchimento automático (o endereço pode ser digitado à mão).
/// </summary>
public class ViaCepHealthCheck : IHealthCheck
{
    internal const string CepDeReferencia = "01001000";

    private readonly IHttpClientFactory _httpClientFactory;

    public ViaCepHealthCheck(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var resposta = await _httpClientFactory.CreateClient(ViaCepService.NomeDoCliente)
                .GetAsync($"{CepDeReferencia}/json/", cancellationToken);

            return resposta.IsSuccessStatusCode
                ? HealthCheckResult.Healthy()
                : new HealthCheckResult(context.Registration.FailureStatus, $"ViaCEP respondeu {(int)resposta.StatusCode}.");
        }
        catch (Exception erro) when (erro is HttpRequestException or OperationCanceledException)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, "ViaCEP indisponível.", erro);
        }
    }
}
