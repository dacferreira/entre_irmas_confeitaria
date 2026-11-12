namespace EntreIrmas.Domain.DTOs.Produto;

/// <summary>Filtros opcionais do cardápio.</summary>
public class FiltroProdutoDto
{
    /// <summary>Slug da categoria (ex.: "bolos").</summary>
    public string? Categoria { get; set; }

    /// <summary>Trecho do nome ou da descrição.</summary>
    public string? Busca { get; set; }

    /// <summary>Quando verdadeiro, traz só os destaques.</summary>
    public bool? Destaque { get; set; }
}
