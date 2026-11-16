namespace EntreIrmas.Domain.DTOs.Comum;

/// <summary>Página de uma listagem.</summary>
public class PaginaDto<T>
{
    public IReadOnlyList<T> Itens { get; set; } = Array.Empty<T>();
    public int Total { get; set; }
    public int Pagina { get; set; }
    public int TamanhoPagina { get; set; }
}

/// <summary>Liga ou desliga um registro (ex.: tirar um produto do cardápio sem excluir).</summary>
public class AlterarSituacaoDto
{
    public bool Ativo { get; set; }
}
