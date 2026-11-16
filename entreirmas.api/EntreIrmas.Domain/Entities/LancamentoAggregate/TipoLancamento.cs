namespace EntreIrmas.Domain.Entities.LancamentoAggregate;

/// <summary>Tipo do lançamento (gravado por nome na coluna LANC_CD_TIPO).</summary>
public enum TipoLancamento
{
    /// <summary>Gasto da operação: ingredientes, embalagem, gás, entrega...</summary>
    Despesa,

    /// <summary>Prejuízo: doce descartado, encomenda desperdiçada, ingrediente vencido...</summary>
    Perda
}
