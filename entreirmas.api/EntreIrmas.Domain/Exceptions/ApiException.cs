namespace EntreIrmas.Domain.Exceptions;

/// <summary>Erro com status HTTP definido por quem lança.</summary>
public class ApiException : Exception
{
    public int StatusCode { get; }

    public ApiException(int statusCode, string mensagem) : base(mensagem)
    {
        StatusCode = statusCode;
    }
}
