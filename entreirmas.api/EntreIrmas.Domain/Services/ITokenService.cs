using EntreIrmas.Domain.DTOs.Autenticacao;
using EntreIrmas.Domain.Entities.UsuarioAggregate;

namespace EntreIrmas.Domain.Services;

/// <summary>Emite o token de acesso à área administrativa.</summary>
public interface ITokenService
{
    SessaoDto GerarSessao(Usuario usuario);
}
