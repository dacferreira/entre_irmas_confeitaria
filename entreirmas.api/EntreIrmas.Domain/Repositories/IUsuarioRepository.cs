using EntreIrmas.Domain.Entities.UsuarioAggregate;
using EntreIrmas.Domain.Seedwork;

namespace EntreIrmas.Domain.Repositories;

public interface IUsuarioRepository : IRepository<Usuario>
{
    /// <summary>Só usuários ativos: os inativos não entram.</summary>
    Task<Usuario?> ObterPorLoginAsync(string login, CancellationToken cancellationToken = default);

    /// <summary>Ativos e inativos (sem os excluídos), em ordem de nome.</summary>
    Task<IReadOnlyList<Usuario>> ListarAdminAsync(CancellationToken cancellationToken = default);

    Task<Usuario?> ObterAdminPorIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>Considera também os registros inativos e excluídos (o índice único vale para todos).</summary>
    Task<bool> ExisteLoginAsync(string login, long? ignorarId, CancellationToken cancellationToken = default);

    Task<int> ContarAtivosAsync(CancellationToken cancellationToken = default);
}
