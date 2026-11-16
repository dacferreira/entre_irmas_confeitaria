using EntreIrmas.Domain.Entities.LancamentoAggregate;

namespace EntreIrmas.Domain.DTOs.Lancamento;

public class LancamentoDto
{
    public long Id { get; set; }
    public TipoLancamento Tipo { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateOnly Data { get; set; }
    public string CadastradoPor { get; set; } = string.Empty;
}

public class SalvarLancamentoDto
{
    public TipoLancamento Tipo { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateOnly Data { get; set; }
}

public class FiltroLancamentoDto
{
    public DateOnly? Inicio { get; set; }
    public DateOnly? Fim { get; set; }
    public TipoLancamento? Tipo { get; set; }
}
