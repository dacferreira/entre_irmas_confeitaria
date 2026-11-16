using System.Security.Cryptography;
using EntreIrmas.Domain.Services;

namespace EntreIrmas.Application.Services;

/// <summary>
/// PBKDF2-SHA256 (nativo do .NET, sem biblioteca externa). Formato gravado: PBKDF2$iteracoes$sal$hash,
/// com sal e hash em Base64. As iterações ficam no próprio hash, então dá para aumentá-las sem invalidar
/// as senhas já gravadas.
/// </summary>
public class SenhaService : ISenhaService
{
    /// <summary>Recomendação da OWASP (2023) para PBKDF2-HMAC-SHA256.</summary>
    public const int IteracoesPadrao = 600_000;

    private const string Prefixo = "PBKDF2";
    private const int TamanhoDoSal = 16;
    private const int TamanhoDoHash = 32;
    private static readonly HashAlgorithmName Algoritmo = HashAlgorithmName.SHA256;

    private readonly int _iteracoes;

    public SenhaService() : this(IteracoesPadrao)
    {
    }

    /// <summary>Permite menos iterações nos testes, para a suíte não ficar lenta.</summary>
    internal SenhaService(int iteracoes)
    {
        _iteracoes = iteracoes;
    }

    public string GerarHash(string senha)
    {
        if (string.IsNullOrEmpty(senha))
            throw new ArgumentException("A senha é obrigatória.", nameof(senha));

        var sal = RandomNumberGenerator.GetBytes(TamanhoDoSal);
        var hash = Rfc2898DeriveBytes.Pbkdf2(senha, sal, _iteracoes, Algoritmo, TamanhoDoHash);
        return $"{Prefixo}${_iteracoes}${Convert.ToBase64String(sal)}${Convert.ToBase64String(hash)}";
    }

    public bool Conferir(string senha, string hash)
    {
        if (string.IsNullOrEmpty(senha) || string.IsNullOrWhiteSpace(hash))
            return false;

        var partes = hash.Split('$');
        if (partes.Length != 4 || partes[0] != Prefixo || !int.TryParse(partes[1], out var iteracoes) || iteracoes < 1)
            return false;

        try
        {
            var sal = Convert.FromBase64String(partes[2]);
            var esperado = Convert.FromBase64String(partes[3]);
            var calculado = Rfc2898DeriveBytes.Pbkdf2(senha, sal, iteracoes, Algoritmo, esperado.Length);
            return CryptographicOperations.FixedTimeEquals(calculado, esperado);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
