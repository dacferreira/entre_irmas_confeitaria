namespace EntreIrmas.Domain.Services;

/// <summary>Hash e conferência de senhas. A senha em si nunca é gravada.</summary>
public interface ISenhaService
{
    string GerarHash(string senha);
    bool Conferir(string senha, string hash);
}
