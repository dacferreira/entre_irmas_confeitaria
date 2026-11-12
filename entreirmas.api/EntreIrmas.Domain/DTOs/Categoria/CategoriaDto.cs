namespace EntreIrmas.Domain.DTOs.Categoria;

public class CategoriaDto
{
    public long Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int QuantidadeProdutos { get; set; }
}
