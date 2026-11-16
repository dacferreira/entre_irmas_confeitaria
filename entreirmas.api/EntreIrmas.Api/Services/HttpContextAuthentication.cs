using EntreIrmas.Domain.Services;

namespace EntreIrmas.Api.Services;

/// <summary>
/// Lê o usuário logado das claims do token. Nas rotas públicas não há usuário: Id fica 0 e o Login,
/// "SISTEMA" (as rotas públicas gravam com o próprio identificador, ex.: "SITE").
/// </summary>
public class HttpContextAuthentication : IAuthentication
{
    public const string SemUsuario = "SISTEMA";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextAuthentication(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public long Id =>
        long.TryParse(_httpContextAccessor.HttpContext?.User.FindFirst(TokenService.ClaimId)?.Value, out var id) ? id : 0;

    public string Login => _httpContextAccessor.HttpContext?.User.FindFirst(TokenService.ClaimLogin)?.Value ?? SemUsuario;
}
