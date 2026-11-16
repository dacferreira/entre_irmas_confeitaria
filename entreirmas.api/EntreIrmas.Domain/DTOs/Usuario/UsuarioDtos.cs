namespace EntreIrmas.Domain.DTOs.Usuario;

public class UsuarioDto
{
    public long Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime? UltimoAcesso { get; set; }
    public DateTime DataEHoraDeCadastro { get; set; }
}

/// <summary>Criação e edição. Na edição, a senha é opcional: vazia mantém a atual.</summary>
public class SalvarUsuarioDto
{
    public string Login { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string? Senha { get; set; }
}
