using EntreIrmas.Domain.Entities.ProdutoAggregate;
using EntreIrmas.Domain.Exceptions;
using EntreIrmas.Domain.Seedwork;

namespace EntreIrmas.Domain.Entities.CategoriaAggregate;

/// <summary>Agrupamento do cardápio (bolos, docinhos, salgados...).</summary>
public class Categoria : Entity, IAggregateRoot
{
    private readonly List<Produto> _produtos = new();

    protected Categoria()
    {
    }

    public Categoria(string nome, string slug, string? descricao, int ordem, string cadastradoPor)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome da categoria é obrigatório.");
        if (string.IsNullOrWhiteSpace(slug))
            throw new DomainException("O identificador (slug) da categoria é obrigatório.");

        Nome = nome.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        Descricao = descricao?.Trim();
        Ordem = ordem;
        DefinirCadastro(cadastradoPor);
    }

    public string Nome { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Descricao { get; private set; }
    public int Ordem { get; private set; }
    public IReadOnlyCollection<Produto> Produtos => _produtos;
}
