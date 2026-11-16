namespace EntreIrmas.Domain.DTOs.Produto;

/// <summary>Produto na área administrativa (inclui custo e os inativos).</summary>
public class ProdutoAdminDto
{
    public long Id { get; set; }
    public long CategoriaId { get; set; }
    public string CategoriaNome { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public decimal Custo { get; set; }
    public string Unidade { get; set; } = string.Empty;
    public int QuantidadeMinima { get; set; }
    public string Ilustracao { get; set; } = string.Empty;
    public string? UrlImagem { get; set; }
    public bool Destaque { get; set; }
    public bool Encomenda { get; set; }
    public bool Ativo { get; set; }
}

/// <summary>Criação e edição. O slug é gerado a partir do nome; a foto tem envio próprio.</summary>
public class SalvarProdutoDto
{
    public long CategoriaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public decimal Custo { get; set; }
    public string Unidade { get; set; } = string.Empty;
    public int QuantidadeMinima { get; set; } = 1;
    public string Ilustracao { get; set; } = string.Empty;
    public bool Destaque { get; set; }

    /// <summary>Feito sob encomenda: exige a antecedência mínima no pedido.</summary>
    public bool Encomenda { get; set; }
}

public class FiltroProdutoAdminDto
{
    public long? CategoriaId { get; set; }

    /// <summary>Trecho do nome.</summary>
    public string? Busca { get; set; }

    /// <summary>true: só ativos; false: só inativos; vazio: todos.</summary>
    public bool? Ativo { get; set; }
}
