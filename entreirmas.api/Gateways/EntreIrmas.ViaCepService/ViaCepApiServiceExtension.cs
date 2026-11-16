using EntreIrmas.Domain.EnviromentVariables;
using EntreIrmas.Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EntreIrmas.ViaCepService;

public static class ViaCepApiServiceExtension
{
    public const string NomeDoHealthCheck = "viacep";

    /// <summary>
    /// Registra o cliente HTTP nomeado do ViaCEP (com timeout sempre definido), o serviço e o health check.
    /// </summary>
    public static IServiceCollection AddViaCepApiService(this IServiceCollection services, IViaCepEnvironmentVariables variaveis)
    {
        var timeout = TimeSpan.FromSeconds(Math.Max(1, variaveis.VIACEP_TIMEOUT_SEGUNDOS));

        services.AddHttpClient(ViaCepService.NomeDoCliente, cliente =>
            {
                cliente.BaseAddress = new Uri(variaveis.VIACEP_URL.TrimEnd('/') + "/");
                cliente.Timeout = timeout;
                cliente.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            })
            // Recicla as conexões para acompanhar mudanças de DNS do serviço.
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(5)
            });

        services.AddMemoryCache();
        services.AddScoped<IViaCepService, ViaCepService>();

        services.AddHealthChecks()
            .AddCheck<ViaCepHealthCheck>(NomeDoHealthCheck, failureStatus: HealthStatus.Degraded, timeout: timeout);

        return services;
    }
}
