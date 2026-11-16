namespace EntreIrmas.Domain.DTOs.Categoria;

/// <summary>Categoria na área administrativa (inclui as inativas).</summary>
public class CategoriaAdminDto
{
    public long Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int Ordem { get; set; }
    public bool Ativo { get; set; }
    public int QuantidadeProdutos { get; set; }
}

/// <summary>Criação e edição. O slug é gerado a partir do nome.</summary>
public class SalvarCategoriaDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int Ordem { get; set; }
}
