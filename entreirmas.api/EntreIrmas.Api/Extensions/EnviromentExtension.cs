using EntreIrmas.Api.EnviromentVariables;
using EntreIrmas.Domain.EnviromentVariables;

namespace EntreIrmas.Api.Extensions;

public static class EnviromentExtension
{
    public const string AmbienteLocal = "Local";
    public const string AmbienteTesting = "Testing";

    /// <summary>Lê as variáveis e registra uma interface por assunto como singleton.</summary>
    public static Variables AddVariables(this WebApplicationBuilder builder)
    {
        // O ASP.NET só carrega user-secrets no ambiente "Development"; o ambiente local daqui é "Local".
        if (builder.Environment.IsEnvironment(AmbienteLocal))
            builder.Configuration.AddUserSecrets<Program>(optional: true);

        var variaveis = builder.Configuration.Get<Variables>() ?? new Variables();

        builder.Services.AddSingleton(variaveis);
        builder.Services.AddSingleton<IMySqlEnvironmentVariables>(variaveis);
        builder.Services.AddSingleton<IWhatsAppEnvironmentVariables>(variaveis);
        builder.Services.AddSingleton<IViaCepEnvironmentVariables>(variaveis);
        builder.Services.AddSingleton<IPedidoEnvironmentVariables>(variaveis);
        builder.Services.AddSingleton<ISwaggerEnvironmentVariables>(variaveis);
        builder.Services.AddSingleton<ICorsEnvironmentVariables>(variaveis);
        builder.Services.AddSingleton<IJwtEnvironmentVariables>(variaveis);
        builder.Services.AddSingleton<IHealthCheckEnvironmentVariables>(variaveis);

        return variaveis;
    }
}
