using System.Text.RegularExpressions;
using EntreIrmas.Domain.Exceptions;
using EntreIrmas.Domain.Seedwork;

namespace EntreIrmas.Domain.Entities.UsuarioAggregate;

/// <summary>Pessoa com acesso à área administrativa. Guarda só o hash da senha.</summary>
public partial class Usuario : Entity, IAggregateRoot
{
    protected Usuario()
    {
    }

    public Usuario(string login, string nome, string hashSenha, string cadastradoPor)
    {
        Login = NormalizarLogin(login);
        if (!FormatoDoLogin().IsMatch(Login))
            throw new DomainException("O login deve ter de 3 a 30 letras minúsculas, números, ponto, hífen ou sublinhado.");

        AlterarNome(nome);
        DefinirHashSenha(hashSenha);
        DefinirCadastro(cadastradoPor);
    }

    public string Login { get; private set; } = string.Empty;
    public string Nome { get; private set; } = string.Empty;
    public string HashSenha { get; private set; } = string.Empty;
    public DateTime? UltimoAcesso { get; private set; }

    public static string NormalizarLogin(string? login) => (login ?? string.Empty).Trim().ToLowerInvariant();

    public void AlterarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome do usuário é obrigatório.");

        Nome = nome.Trim();
    }

    public void DefinirHashSenha(string hashSenha)
    {
        if (string.IsNullOrWhiteSpace(hashSenha))
            throw new DomainException("A senha do usuário é obrigatória.");

        HashSenha = hashSenha;
    }

    public void RegistrarAcesso(DateTime dataEHora) => UltimoAcesso = dataEHora;

    /// <summary>Exclusão lógica: o acesso deixa de valer e o login fica livre para outro cadastro.</summary>
    public void Excluir()
    {
        Arquivar();
        Login = ValorUnicoAposExclusao(Login, TamanhoMaximoDoLogin);
    }

    public const int TamanhoMaximoDoLogin = 30;

    [GeneratedRegex("^[a-z0-9._-]{3,30}$")]
    private static partial Regex FormatoDoLogin();
}
