namespace EntreIrmas.Domain.Services;

/// <summary>Usuário logado na requisição atual (lido do token).</summary>
public interface IAuthentication
{
    long Id { get; }
    string Login { get; }
}
