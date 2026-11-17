using EntreIrmas.Domain.EnviromentVariables;
using EntreIrmas.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EntreIrmas.UberDirectService;

public static class UberDirectApiServiceExtension
{
    /// <summary>Com as três credenciais preenchidas, a Uber entra como parceiro de entrega.</summary>
    public static bool UberConfigurada(IUberEnvironmentVariables uber) =>
        !string.IsNullOrWhiteSpace(uber.UBER_CLIENT_ID)
        && !string.IsNullOrWhiteSpace(uber.UBER_CLIENT_SECRET)
        && !string.IsNullOrWhiteSpace(uber.UBER_CUSTOMER_ID);

    /// <summary>
    /// Registra os clientes HTTP do Uber Direct (com timeout sempre definido) e o serviço como parceiro de entrega.
    /// Sem credenciais, não registra nada: a Uber fica de fora das cotações.
    /// </summary>
    public static IServiceCollection AddUberDirectApiService(this IServiceCollection services, IUberEnvironmentVariables uber,
        IEntregaEnvironmentVariables entrega)
    {
        if (!UberConfigurada(uber))
            return services;

        var timeout = TimeSpan.FromSeconds(Math.Max(1, entrega.ENTREGA_TIMEOUT_SEGUNDOS));

        services.AddHttpClient(UberDirectService.NomeDoCliente, cliente =>
            {
                cliente.BaseAddress = new Uri(uber.UBER_API_URL.TrimEnd('/') + "/");
                cliente.Timeout = timeout;
                cliente.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            })
            // Recicla as conexões para acompanhar mudanças de DNS do serviço.
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(5)
            });

        services.AddHttpClient(UberDirectService.NomeDoClienteDeAutenticacao, cliente =>
        {
            cliente.Timeout = timeout;
            cliente.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        });

        services.AddMemoryCache();
        services.AddScoped<IEntregaParceiroService, UberDirectService>();

        return services;
    }
}
