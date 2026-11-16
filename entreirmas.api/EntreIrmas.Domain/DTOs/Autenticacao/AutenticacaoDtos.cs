namespace EntreIrmas.Domain.DTOs.Autenticacao;

public class LoginDto
{
    public string Login { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

/// <summary>Resposta do login: token JWT e dados para o cabeçalho da área administrativa.</summary>
public class SessaoDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEm { get; set; }
    public string Login { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
}
