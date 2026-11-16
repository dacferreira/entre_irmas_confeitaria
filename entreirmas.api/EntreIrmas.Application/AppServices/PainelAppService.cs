using EntreIrmas.Domain.AppServices;
using EntreIrmas.Domain.DTOs.Lancamento;
using EntreIrmas.Domain.DTOs.Painel;
using EntreIrmas.Domain.Entities.LancamentoAggregate;
using EntreIrmas.Domain.Entities.PedidoAggregate;
using EntreIrmas.Domain.Exceptions;
using EntreIrmas.Domain.Repositories;

namespace EntreIrmas.Application.AppServices;

/// <summary>
/// Monta o painel financeiro. O volume de uma confeitaria cabe em memória: os pedidos e os lançamentos
/// do período são lidos uma vez e somados aqui, o que deixa as regras fáceis de testar.
/// </summary>
public class PainelAppService : IPainelAppService
{
    internal const int DiasMaximosPorDia = 62;
    internal const int DiasMaximosDoPeriodo = 3 * 366;
    internal const int QuantidadeDeProdutosMaisVendidos = 5;

    private readonly IPedidoRepository _pedidoRepository;
    private readonly ILancamentoRepository _lancamentoRepository;
    private readonly TimeProvider _timeProvider;

    public PainelAppService(IPedidoRepository pedidoRepository, ILancamentoRepository lancamentoRepository,
        TimeProvider timeProvider)
    {
        _pedidoRepository = pedidoRepository;
        _lancamentoRepository = lancamentoRepository;
        _timeProvider = timeProvider;
    }

    public async Task<PainelDto> ObterAsync(DateOnly? inicio, DateOnly? fim, CancellationToken cancellationToken = default)
    {
        var hoje = DateOnly.FromDateTime(_timeProvider.GetLocalNow().DateTime);
        var dataInicial = inicio ?? new DateOnly(hoje.Year, hoje.Month, 1);
        var dataFinal = fim ?? dataInicial.AddMonths(1).AddDays(-1);

        if (dataFinal < dataInicial)
            throw new DomainException("A data final deve ser igual ou posterior à inicial.");
        if (dataFinal.DayNumber - dataInicial.DayNumber > DiasMaximosDoPeriodo)
            throw new DomainException("Escolha um período de até 3 anos.");

        var pedidos = await _pedidoRepository.ListarPorPeriodoAsync(dataInicial, dataFinal, cancellationToken);
        var lancamentos = await _lancamentoRepository.ListarAsync(
            new FiltroLancamentoDto { Inicio = dataInicial, Fim = dataFinal }, cancellationToken);

        var vendas = pedidos.Where(p => p.ContaComoVenda).ToList();
        var receita = vendas.Sum(p => p.ValorTotal);
        var custoProdutos = vendas.Sum(p => p.CustoTotal);
        var despesas = lancamentos.Where(l => l.Tipo == TipoLancamento.Despesa).Sum(l => l.Valor);
        var perdas = lancamentos.Where(l => l.Tipo == TipoLancamento.Perda).Sum(l => l.Valor);
        var lucro = receita - custoProdutos - despesas - perdas;
        var porMes = dataFinal.DayNumber - dataInicial.DayNumber + 1 > DiasMaximosPorDia;

        return new PainelDto
        {
            Inicio = dataInicial,
            Fim = dataFinal,
            Receita = receita,
            CustoProdutos = custoProdutos,
            Despesas = despesas,
            Perdas = perdas,
            Lucro = lucro,
            Margem = receita > 0 ? Math.Round(lucro / receita * 100, 1) : null,
            QuantidadeVendas = vendas.Count,
            TicketMedio = vendas.Count > 0 ? Math.Round(receita / vendas.Count, 2) : 0,
            AguardandoConfirmacao = Resumir(pedidos.Where(p => p.Status == StatusPedido.Recebido)),
            Cancelados = Resumir(pedidos.Where(p => p.Status == StatusPedido.Cancelado)),
            PorStatus = Enum.GetValues<StatusPedido>()
                .Select(status => new PedidosPorStatusDto
                {
                    Status = status,
                    Quantidade = pedidos.Count(p => p.Status == status),
                    Valor = pedidos.Where(p => p.Status == status).Sum(p => p.ValorTotal)
                })
                .ToList(),
            Agrupamento = porMes ? "Mes" : "Dia",
            Serie = MontarSerie(dataInicial, dataFinal, porMes, vendas, lancamentos),
            ProdutosMaisVendidos = vendas
                .SelectMany(p => p.Itens)
                .GroupBy(i => i.ProdutoId)
                .Select(grupo => new ProdutoMaisVendidoDto
                {
                    ProdutoId = grupo.Key,
                    Nome = grupo.Last().NomeProduto,
                    Unidade = grupo.Last().Unidade,
                    Quantidade = grupo.Sum(i => i.Quantidade),
                    Receita = grupo.Sum(i => i.ValorTotal),
                    Lucro = grupo.Sum(i => i.ValorTotal - i.CustoTotal)
                })
                .OrderByDescending(p => p.Receita)
                .ThenBy(p => p.Nome)
                .Take(QuantidadeDeProdutosMaisVendidos)
                .ToList()
        };
    }

    private static ResumoPedidosDto Resumir(IEnumerable<Pedido> pedidos)
    {
        var lista = pedidos.ToList();
        return new ResumoPedidosDto { Quantidade = lista.Count, Valor = lista.Sum(p => p.ValorTotal) };
    }

    // Um ponto por dia (ou por mês), inclusive os sem movimento, para o gráfico não pular datas.
    private static List<PontoDaSerieDto> MontarSerie(DateOnly inicio, DateOnly fim, bool porMes,
        IReadOnlyList<Pedido> vendas, IReadOnlyList<Lancamento> lancamentos)
    {
        DateOnly Chave(DateOnly data) => porMes ? new DateOnly(data.Year, data.Month, 1) : data;

        var receitaPorChave = vendas.GroupBy(p => Chave(p.DataDesejada))
            .ToDictionary(g => g.Key, g => (Receita: g.Sum(p => p.ValorTotal), Custo: g.Sum(p => p.CustoTotal)));
        var lancamentosPorChave = lancamentos.GroupBy(l => Chave(l.Data))
            .ToDictionary(g => g.Key, g => g.Sum(l => l.Valor));

        var serie = new List<PontoDaSerieDto>();
        for (var data = Chave(inicio); data <= fim; data = porMes ? data.AddMonths(1) : data.AddDays(1))
        {
            var (receita, custo) = receitaPorChave.GetValueOrDefault(data);
            var custos = custo + lancamentosPorChave.GetValueOrDefault(data);
            serie.Add(new PontoDaSerieDto { Data = data, Receita = receita, Custos = custos, Lucro = receita - custos });
        }

        return serie;
    }
}
