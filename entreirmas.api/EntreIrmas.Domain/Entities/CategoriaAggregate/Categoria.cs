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
        Atualizar(nome, slug, descricao, ordem);
        DefinirCadastro(cadastradoPor);
    }

    public string Nome { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Descricao { get; private set; }
    public int Ordem { get; private set; }
    public IReadOnlyCollection<Produto> Produtos => _produtos;

    public void Atualizar(string nome, string slug, string? descricao, int ordem)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome da categoria é obrigatório.");
        if (string.IsNullOrWhiteSpace(slug))
            throw new DomainException("O identificador (slug) da categoria é obrigatório.");

        Nome = nome.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        Descricao = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim();
        Ordem = ordem;
    }

    /// <summary>Exclusão lógica. Só é permitida sem produtos (ativos ou inativos) na categoria.</summary>
    public void Excluir()
    {
        if (_produtos.Count > 0)
            throw new DomainException(
                $"Esta categoria tem {_produtos.Count} produto(s). Mova ou exclua os produtos antes de excluí-la.");

        Arquivar();
        Slug = ValorUnicoAposExclusao(Slug, TamanhoMaximoDoSlug);
    }

    public const int TamanhoMaximoDoSlug = 100;
}
