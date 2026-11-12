namespace EntreIrmas.Domain.Seedwork;

/// <summary>Objeto de valor: a igualdade é dada pelos componentes, não por identificador.</summary>
public abstract class ValueObject
{
    protected abstract IEnumerable<object?> ObterComponentesDeIgualdade();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        return ObterComponentesDeIgualdade().SequenceEqual(((ValueObject)obj).ObterComponentesDeIgualdade());
    }

    public override int GetHashCode() =>
        ObterComponentesDeIgualdade().Aggregate(0, (hash, componente) => HashCode.Combine(hash, componente));
}
