using System.Text.Json;
using EntreIrmas.Api.EnviromentVariables;
using EntreIrmas.Domain.EnviromentVariables;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EntreIrmas.Api.Extensions;

public static class HealthCheckExtension
{
    private const int LimiteDeMemoriaEmMb = 512;

    public static IServiceCollection AddHealthCheck(this IServiceCollection services, Variables variaveis)
    {
        var timeout = TimeSpan.FromSeconds(((IHealthCheckEnvironmentVariables)variaveis).HEALTHCHECK_TIMEOUT_SEGUNDOS);

        services.AddHealthChecks()
            .AddMySql(
                connectionString: MySqlExtension.MontarConnectionString((IMySqlEnvironmentVariables)variaveis),
                healthQuery: "SELECT 1;",
                name: "mysql",
                failureStatus: HealthStatus.Unhealthy,
                timeout: timeout)
            .AddProcessAllocatedMemoryHealthCheck(LimiteDeMemoriaEmMb, name: "memoria", timeout: timeout);

        return services;
    }

    /// <summary>Publica /health (respondido em /api/health pelo path base).</summary>
    public static IApplicationBuilder ConfiguraHealthCheck(this IApplicationBuilder app) =>
        app.UseHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = async (context, relatorio) =>
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    status = relatorio.Status.ToString(),
                    duracao = relatorio.TotalDuration.TotalMilliseconds,
                    verificacoes = relatorio.Entries.Select(e => new
                    {
                        nome = e.Key,
                        status = e.Value.Status.ToString(),
                        duracao = e.Value.Duration.TotalMilliseconds
                    })
                }));
            }
        });
}
