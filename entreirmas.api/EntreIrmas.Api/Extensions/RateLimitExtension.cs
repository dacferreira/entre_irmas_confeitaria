using System.Threading.RateLimiting;
using EntreIrmas.Api.EnviromentVariables;
using EntreIrmas.Api.Filters;
using EntreIrmas.Domain.EnviromentVariables;
using Microsoft.AspNetCore.RateLimiting;

namespace EntreIrmas.Api.Extensions;

public static class RateLimitExtension
{
    public const string PoliticaPedidos = "pedidos";
    public const string PoliticaLogin = "login";
    public const string PoliticaCep = "cep";
    public const string PoliticaAcompanhamento = "acompanhamento";

    /// <summary>
    /// Limita por IP a criação de pedidos (abuso do formulário público), as tentativas de login
    /// (adivinhação de senha), as consultas de CEP e o acompanhamento de pedidos (tentativa de códigos).
    /// </summary>
    public static IServiceCollection AddRateLimit(this IServiceCollection services,
        IPedidoEnvironmentVariables variaveisPedido, IJwtEnvironmentVariables variaveisLogin,
        IViaCepEnvironmentVariables variaveisCep)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, cancellationToken) =>
                await context.HttpContext.Response.WriteAsJsonAsync(
                    new ErroResponse("Muitas tentativas em pouco tempo. Aguarde um minuto e tente de novo."),
                    cancellationToken);

            options.AddPolicy(PoliticaPedidos, httpContext =>
                PorIp(httpContext, variaveisPedido.PEDIDO_LIMITE_POR_MINUTO));

            options.AddPolicy(PoliticaLogin, httpContext =>
                PorIp(httpContext, variaveisLogin.LOGIN_LIMITE_POR_MINUTO));

            options.AddPolicy(PoliticaAcompanhamento, httpContext =>
                PorIp(httpContext, variaveisPedido.ACOMPANHAMENTO_LIMITE_POR_MINUTO));

            // A consulta de CEP é pública: o limite evita que a API vire um repasse gratuito para o ViaCEP.
            options.AddPolicy(PoliticaCep, httpContext =>
                PorIp(httpContext, variaveisCep.CEP_LIMITE_POR_MINUTO));
        });

        return services;
    }

    private static RateLimitPartition<string> PorIp(HttpContext httpContext, int limitePorMinuto) =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = Math.Max(1, limitePorMinuto),
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
}
