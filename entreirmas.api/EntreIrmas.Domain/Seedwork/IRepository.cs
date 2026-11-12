namespace EntreIrmas.Domain.Seedwork;

/// <summary>Contrato base dos repositórios de raízes de agregado.</summary>
public interface IRepository<T> where T : Entity, IAggregateRoot
{
    Task<T?> ObterPorIdAsync(long id, CancellationToken cancellationToken = default);
    Task AdicionarAsync(T entidade, CancellationToken cancellationToken = default);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
