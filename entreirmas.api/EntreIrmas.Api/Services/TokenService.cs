using System.Security.Claims;
using System.Text;
using EntreIrmas.Api.EnviromentVariables;
using EntreIrmas.Domain.DTOs.Autenticacao;
using EntreIrmas.Domain.Entities.UsuarioAggregate;
using EntreIrmas.Domain.Services;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace EntreIrmas.Api.Services;

/// <summary>Emite o JWT (HMAC-SHA256) com a mesma chave, emissor e audiência validados pelo JwtBearer.</summary>
public class TokenService : ITokenService
{
    public const string ClaimId = JwtRegisteredClaimNames.Sub;
    public const string ClaimLogin = "login";
    public const string ClaimNome = "nome";

    private readonly IJwtEnvironmentVariables _variaveis;
    private readonly TimeProvider _timeProvider;
    private readonly SigningCredentials _credenciais;

    public TokenService(IJwtEnvironmentVariables variaveis, TimeProvider timeProvider)
    {
        _variaveis = variaveis;
        _timeProvider = timeProvider;
        _credenciais = new SigningCredentials(CriarChave(variaveis.JWT_CHAVE), SecurityAlgorithms.HmacSha256);
    }

    public static SymmetricSecurityKey CriarChave(string chave) => new(Encoding.UTF8.GetBytes(chave));

    public SessaoDto GerarSessao(Usuario usuario)
    {
        var agora = _timeProvider.GetUtcNow().UtcDateTime;
        var expiraEm = agora.AddHours(Math.Max(1, _variaveis.JWT_EXPIRACAO_HORAS));

        var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = _variaveis.JWT_EMISSOR,
            Audience = _variaveis.JWT_AUDIENCIA,
            IssuedAt = agora,
            NotBefore = agora,
            Expires = expiraEm,
            SigningCredentials = _credenciais,
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimId, usuario.Id.ToString()),
                new Claim(ClaimLogin, usuario.Login),
                new Claim(ClaimNome, usuario.Nome)
            ])
        });

        return new SessaoDto { Token = token, ExpiraEm = expiraEm, Login = usuario.Login, Nome = usuario.Nome };
    }
}
