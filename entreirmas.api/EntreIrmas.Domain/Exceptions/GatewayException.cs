namespace EntreIrmas.Domain.Exceptions;

/// <summary>Falha ao falar com um sistema externo (fora do ar, lento ou com resposta inesperada).</summary>
public class GatewayException : Exception
{
    public string Sistema { get; }

    public GatewayException(string sistema, string mensagem, Exception? causa = null) : base(mensagem, causa)
    {
        Sistema = sistema;
    }
}
