using EntreIrmas.Domain.Entities.CategoriaAggregate;
using EntreIrmas.Domain.Exceptions;
using EntreIrmas.Domain.Seedwork;

namespace EntreIrmas.Domain.Entities.ProdutoAggregate;

/// <summary>Item do cardápio vendido pela confeitaria.</summary>
public class Produto : Entity, IAggregateRoot
{
    protected Produto()
    {
    }

    public Produto(long categoriaId, string nome, string slug, string descricao, decimal preco, string unidade,
        int quantidadeMinima, string ilustracao, string? urlImagem, bool destaque, string cadastradoPor)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome do produto é obrigatório.");
        if (string.IsNullOrWhiteSpace(slug))
            throw new DomainException("O identificador (slug) do produto é obrigatório.");
        if (preco <= 0)
            throw new DomainException("O preço do produto deve ser maior que zero.");
        if (quantidadeMinima < 1)
            throw new DomainException("A quantidade mínima deve ser de pelo menos 1.");

        CategoriaId = categoriaId;
        Nome = nome.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        Descricao = descricao.Trim();
        Preco = preco;
        Unidade = unidade.Trim();
        QuantidadeMinima = quantidadeMinima;
        Ilustracao = ilustracao.Trim();
        UrlImagem = urlImagem?.Trim();
        Destaque = destaque;
        DefinirCadastro(cadastradoPor);
    }

    public long CategoriaId { get; private set; }
    public Categoria? Categoria { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public decimal Preco { get; private set; }

    /// <summary>Como o produto é vendido: "unidade", "fatia", "kg", "caixa com 4"...</summary>
    public string Unidade { get; private set; } = string.Empty;

    public int QuantidadeMinima { get; private set; } = 1;

    /// <summary>Código da ilustração usada pelo site quando não há foto (brownie, bolo, brigadeiro...).</summary>
    public string Ilustracao { get; private set; } = string.Empty;

    public string? UrlImagem { get; private set; }
    public bool Destaque { get; private set; }
}
