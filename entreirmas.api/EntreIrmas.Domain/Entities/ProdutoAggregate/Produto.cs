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

    public Produto(long categoriaId, string nome, string slug, string descricao, decimal preco, decimal custo,
        string unidade, int quantidadeMinima, string ilustracao, bool destaque, bool encomenda, string cadastradoPor)
    {
        Atualizar(categoriaId, nome, slug, descricao, preco, custo, unidade, quantidadeMinima, ilustracao, destaque,
            encomenda);
        DefinirCadastro(cadastradoPor);
    }

    public long CategoriaId { get; private set; }
    public Categoria? Categoria { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public decimal Preco { get; private set; }

    /// <summary>Custo de produção por unidade de venda (ingredientes e embalagem), usado no cálculo do lucro.</summary>
    public decimal Custo { get; private set; }

    /// <summary>Como o produto é vendido: "unidade", "fatia", "kg", "caixa com 4"...</summary>
    public string Unidade { get; private set; } = string.Empty;

    public int QuantidadeMinima { get; private set; } = 1;

    /// <summary>Código da ilustração usada pelo site quando não há foto (brownie, bolo, brigadeiro...).</summary>
    public string Ilustracao { get; private set; } = string.Empty;

    /// <summary>
    /// Caminho da foto na API, relativo à base da API (ex.: produtos/10/imagem?v=638...). A versão muda a cada
    /// envio, para o navegador não mostrar a foto antiga do cache. Nulo: o site usa a ilustração.
    /// </summary>
    public string? UrlImagem { get; private set; }

    public bool Destaque { get; private set; }

    /// <summary>
    /// Feito sob encomenda: aparece por último no cardápio e faz o pedido exigir a antecedência mínima.
    /// Sem encomenda (pronta entrega), o pedido pode ser para o mesmo dia.
    /// </summary>
    public bool Encomenda { get; private set; }

    public void Atualizar(long categoriaId, string nome, string slug, string descricao, decimal preco, decimal custo,
        string unidade, int quantidadeMinima, string ilustracao, bool destaque, bool encomenda)
    {
        if (categoriaId <= 0)
            throw new DomainException("Escolha a categoria do produto.");
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome do produto é obrigatório.");
        if (string.IsNullOrWhiteSpace(slug))
            throw new DomainException("O identificador (slug) do produto é obrigatório.");
        if (preco <= 0)
            throw new DomainException("O preço do produto deve ser maior que zero.");
        if (custo < 0)
            throw new DomainException("O custo do produto não pode ser negativo.");
        if (quantidadeMinima < 1)
            throw new DomainException("A quantidade mínima deve ser de pelo menos 1.");

        CategoriaId = categoriaId;
        Nome = nome.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        Descricao = descricao.Trim();
        Preco = preco;
        Custo = custo;
        Unidade = unidade.Trim();
        QuantidadeMinima = quantidadeMinima;
        Ilustracao = ilustracao.Trim();
        Destaque = destaque;
        Encomenda = encomenda;
    }

    /// <summary>Aponta para a foto enviada (versão = momento do envio) ou, com null, volta para a ilustração.</summary>
    public void DefinirImagem(long? versao)
    {
        UrlImagem = versao.HasValue ? $"produtos/{Id}/imagem?v={versao.Value}" : null;
    }

    /// <summary>Exclusão lógica: some do site e da área administrativa; os pedidos antigos continuam.</summary>
    public void Excluir()
    {
        Arquivar();
        Slug = ValorUnicoAposExclusao(Slug, TamanhoMaximoDoSlug);
    }

    public const int TamanhoMaximoDoSlug = 120;
}
