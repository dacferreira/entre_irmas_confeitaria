using Serilog;
using Serilog.Events;

namespace EntreIrmas.Api.Extensions;

public static class SerilogExtension
{
    /// <summary>Configura o Serilog a partir da seção "Serilog" do appsettings.</summary>
    public static WebApplicationBuilder AddSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services));

        return builder;
    }

    /// <summary>Log de requisições com o usuário, sem poluir com as chamadas de /health.</summary>
    public static IApplicationBuilder UseSerilogRequestLog(this IApplicationBuilder app) =>
        app.UseSerilogRequestLogging(options =>
        {
            options.GetLevel = (httpContext, _, excecao) =>
                excecao is not null || httpContext.Response.StatusCode >= 500
                    ? LogEventLevel.Error
                    : httpContext.Request.Path.StartsWithSegments("/health")
                        ? LogEventLevel.Verbose
                        : LogEventLevel.Information;

            options.EnrichDiagnosticContext = (diagnostico, httpContext) =>
                diagnostico.Set("Usuario", httpContext.User.Identity?.IsAuthenticated == true
                    ? httpContext.User.Identity.Name ?? "autenticado"
                    : "anonimo");
        });
}
