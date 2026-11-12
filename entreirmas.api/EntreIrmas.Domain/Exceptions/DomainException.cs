namespace EntreIrmas.Domain.Exceptions;

/// <summary>Violação de regra de negócio (HTTP 400).</summary>
public class DomainException : Exception
{
    public DomainException(string mensagem) : base(mensagem)
    {
    }
}
