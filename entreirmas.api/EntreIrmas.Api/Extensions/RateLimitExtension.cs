using System.Threading.RateLimiting;
using EntreIrmas.Api.Filters;
using EntreIrmas.Domain.EnviromentVariables;
using Microsoft.AspNetCore.RateLimiting;

namespace EntreIrmas.Api.Extensions;

public static class RateLimitExtension
{
    public const string PoliticaPedidos = "pedidos";

    /// <summary>Limita a criação de pedidos por IP, para conter abuso do formulário público.</summary>
    public static IServiceCollection AddRateLimit(this IServiceCollection services, IPedidoEnvironmentVariables variaveis)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, cancellationToken) =>
                await context.HttpContext.Response.WriteAsJsonAsync(
                    new ErroResponse("Muitos pedidos em pouco tempo. Aguarde um minuto e tente de novo."),
                    cancellationToken);

            options.AddPolicy(PoliticaPedidos, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = Math.Max(1, variaveis.PEDIDO_LIMITE_POR_MINUTO),
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));
        });

        return services;
    }
}
