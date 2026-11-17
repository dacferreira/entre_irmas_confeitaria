using EntreIrmas.Application.Services;
using EntreIrmas.Domain.Entities.PedidoAggregate;
using EntreIrmas.Domain.EnviromentVariables;
using EntreIrmas.Domain.Services;
using EntreIrmas.UberDirectService;

namespace EntreIrmas.Api.Extensions;

public static class EntregaExtension
{
    /// <summary>
    /// Parceiros de entrega: com ENTREGA_SIMULADOR, Uber e 99 simuladas (ambiente local e testes); senão, os
    /// parceiros reais que tiverem credenciais. Hoje só a Uber Direct tem integração real; a 99 entra quando a
    /// 99Entrega liberar a documentação da API para a conta da confeitaria. Sem nenhum parceiro, o checkout
    /// avisa que a taxa é combinada pelo WhatsApp.
    /// </summary>
    public static IServiceCollection AddEntregaParceiros(this IServiceCollection services,
        IEntregaEnvironmentVariables entrega, IUberEnvironmentVariables uber)
    {
        if (entrega.ENTREGA_SIMULADOR)
        {
            services.AddSingleton<IEntregaParceiroService>(_ => new EntregaSimuladaService(ParceiroEntrega.Uber, entrega));
            services.AddSingleton<IEntregaParceiroService>(_ => new EntregaSimuladaService(ParceiroEntrega.NoventaENove, entrega));
            return services;
        }

        return services.AddUberDirectApiService(uber, entrega);
    }
}
