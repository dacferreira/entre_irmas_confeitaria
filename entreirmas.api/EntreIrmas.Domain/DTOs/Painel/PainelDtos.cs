using EntreIrmas.Domain.Entities.PedidoAggregate;

namespace EntreIrmas.Domain.DTOs.Painel;

/// <summary>
/// Resumo financeiro do período. Vendas são os pedidos confirmados e não cancelados, pela data desejada;
/// lucro = vendas - custo dos produtos vendidos - despesas - perdas.
/// </summary>
public class PainelDto
{
    public DateOnly Inicio { get; set; }
    public DateOnly Fim { get; set; }

    public decimal Receita { get; set; }
    public decimal CustoProdutos { get; set; }
    public decimal Despesas { get; set; }
    public decimal Perdas { get; set; }
    public decimal Lucro { get; set; }

    /// <summary>Lucro sobre a receita, em %. Vazio quando não houve venda.</summary>
    public decimal? Margem { get; set; }

    public int QuantidadeVendas { get; set; }
    public decimal TicketMedio { get; set; }

    /// <summary>Pedidos ainda não confirmados (status Recebido): não entram na receita.</summary>
    public ResumoPedidosDto AguardandoConfirmacao { get; set; } = new();

    public ResumoPedidosDto Cancelados { get; set; } = new();

    public List<PedidosPorStatusDto> PorStatus { get; set; } = new();

    /// <summary>"Dia" para períodos de até 62 dias; "Mes" para os maiores.</summary>
    public string Agrupamento { get; set; } = "Dia";

    public List<PontoDaSerieDto> Serie { get; set; } = new();
    public List<ProdutoMaisVendidoDto> ProdutosMaisVendidos { get; set; } = new();
}

public class ResumoPedidosDto
{
    public int Quantidade { get; set; }
    public decimal Valor { get; set; }
}

public class PedidosPorStatusDto
{
    public StatusPedido Status { get; set; }
    public int Quantidade { get; set; }
    public decimal Valor { get; set; }
}

public class PontoDaSerieDto
{
    /// <summary>Primeiro dia do dia ou do mês representado.</summary>
    public DateOnly Data { get; set; }

    public decimal Receita { get; set; }

    /// <summary>Custo dos produtos + despesas + perdas.</summary>
    public decimal Custos { get; set; }

    public decimal Lucro { get; set; }
}

public class ProdutoMaisVendidoDto
{
    public long ProdutoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Unidade { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal Receita { get; set; }
    public decimal Lucro { get; set; }
}
