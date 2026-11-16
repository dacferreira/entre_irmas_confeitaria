namespace EntreIrmas.Domain.DTOs.Produto;

public class ProdutoDto
{
    public long Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public string Unidade { get; set; } = string.Empty;
    public int QuantidadeMinima { get; set; }
    public string Ilustracao { get; set; } = string.Empty;
    public string? UrlImagem { get; set; }
    public bool Destaque { get; set; }

    /// <summary>Feito sob encomenda (exige antecedência no pedido).</summary>
    public bool Encomenda { get; set; }
    public string CategoriaSlug { get; set; } = string.Empty;
    public string CategoriaNome { get; set; } = string.Empty;
}
