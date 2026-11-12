using EntreIrmas.Api.EnviromentVariables;

namespace EntreIrmas.Api.Extensions;

public static class CorsExtension
{
    public const string CorsPolicy = "EntreIrmasCors";

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, ICorsEnvironmentVariables variaveis)
    {
        var origens = variaveis.CORS_ORIGENS
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        services.AddCors(options => options.AddPolicy(CorsPolicy, policy =>
        {
            if (origens.Length > 0)
                policy.WithOrigins(origens).AllowAnyHeader().AllowAnyMethod();
        }));

        return services;
    }
}
