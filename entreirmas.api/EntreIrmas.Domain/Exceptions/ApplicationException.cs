namespace EntreIrmas.Domain.Exceptions;

/// <summary>Falha de caso de uso na camada de aplicação (HTTP 500).</summary>
public class ApplicationException : Exception
{
    public ApplicationException(string mensagem, Exception? excecaoInterna = null) : base(mensagem, excecaoInterna)
    {
    }
}
