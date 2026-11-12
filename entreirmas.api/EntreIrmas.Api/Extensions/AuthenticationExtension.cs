using System.Text;
using EntreIrmas.Api.EnviromentVariables;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace EntreIrmas.Api.Extensions;

public static class AuthenticationExtension
{
    private const int TamanhoMinimoDaChave = 32;

    /// <summary>
    /// JWT Bearer. A autorização é exigida por padrão (AuthorizeFilter global em AddControllers);
    /// as rotas públicas do site usam [AllowAnonymous].
    /// </summary>
    public static IServiceCollection AddAuthenticationJwt(this IServiceCollection services, IJwtEnvironmentVariables variaveis)
    {
        if (Encoding.UTF8.GetByteCount(variaveis.JWT_CHAVE ?? string.Empty) < TamanhoMinimoDaChave)
            throw new InvalidOperationException(
                $"JWT_CHAVE deve ter pelo menos {TamanhoMinimoDaChave} caracteres. " +
                "No ambiente local, defina com: dotnet user-secrets set JWT_CHAVE \"<chave>\"");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = variaveis.JWT_EMISSOR,
                ValidateAudience = true,
                ValidAudience = variaveis.JWT_AUDIENCIA,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(variaveis.JWT_CHAVE!)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            });

        services.AddAuthorization();
        return services;
    }
}
