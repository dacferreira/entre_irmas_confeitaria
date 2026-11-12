namespace EntreIrmas.Domain.Exceptions;

/// <summary>Registro não encontrado (HTTP 404).</summary>
public class ObjectNotFoundException : Exception
{
    public ObjectNotFoundException(string mensagem) : base(mensagem)
    {
    }
}
