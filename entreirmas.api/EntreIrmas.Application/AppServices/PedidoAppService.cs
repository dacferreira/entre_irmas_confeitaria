using System.Security.Cryptography;
using EntreIrmas.Domain.AppServices;
using EntreIrmas.Domain.DTOs.Comum;
using EntreIrmas.Domain.DTOs.Pedido;
using EntreIrmas.Domain.Entities.PedidoAggregate;
using EntreIrmas.Domain.EnviromentVariables;
using EntreIrmas.Domain.Exceptions;
using EntreIrmas.Domain.Repositories;
using EntreIrmas.Domain.Services;
using EntreIrmas.Domain.Validations;
using MapsterMapper;

namespace EntreIrmas.Application.AppServices;

public class PedidoAppService : IPedidoAppService
{
    internal const string CadastradoPor = "SITE";
    internal const string PrefixoCodigo = "EI-";
    internal const int MaximoDeNovosDoSite = 20;
    internal const string MensagemCodigoInvalido = "Código inválido. Ele tem o formato EI-XXXXXX.";
    internal const string MensagemNaoEncontrado = "Pedido não encontrado. Confira o código.";
    internal const string MensagemSemEntregadorParaChamar =
        "Este pedido não tem entregador para chamar: só entregas com Uber ou 99 ainda não chamadas.";
    internal const string MensagemLojaFechada =
        "A loja está fechada agora e só aceita encomendas. Inclua um doce sob encomenda ou volte quando abrirmos.";

    // Sem 0/O e 1/I/L, para o cliente ditar o código sem confusão.
    private const string CaracteresDoCodigo = "23456789ABCDEFGHJKMNPQRSTUVWXYZ";
    private const int TamanhoDoCodigo = 6;
    private const int TentativasDeCodigo = 5;

    private readonly IPedidoRepository _pedidoRepository;
    private readonly IProdutoRepository _produtoRepository;
    private readonly ILojaRepository _lojaRepository;
    private readonly IEntregaService _entregaService;
    private readonly IMensagemPedidoService _mensagemPedidoService;
    private readonly IPedidoEnvironmentVariables _variaveis;
    private readonly IAuthentication _authentication;
    private readonly IMapper _mapper;
    private readonly TimeProvider _timeProvider;

    public PedidoAppService(IPedidoRepository pedidoRepository, IProdutoRepository produtoRepository,
        ILojaRepository lojaRepository, IEntregaService entregaService,
        IMensagemPedidoService mensagemPedidoService, IPedidoEnvironmentVariables variaveis,
        IAuthentication authentication, IMapper mapper, TimeProvider timeProvider)
    {
        _pedidoRepository = pedidoRepository;
        _produtoRepository = produtoRepository;
        _lojaRepository = lojaRepository;
        _entregaService = entregaService;
        _mensagemPedidoService = mensagemPedidoService;
        _variaveis = variaveis;
        _authentication = authentication;
        _mapper = mapper;
        _timeProvider = timeProvider;
    }

    public async Task<PedidoCriadoDto> CriarAsync(CriarPedidoDto dto, CancellationToken cancellationToken = default)
    {
        var pedido = await MontarEGravarAsync(dto, CadastradoPor, null, regrasDoSite: true, cancellationToken);

        var resultado = _mapper.Map<PedidoCriadoDto>(pedido);
        resultado.LinkWhatsApp = _mensagemPedidoService.MontarLinkWhatsApp(pedido);
        return resultado;
    }

    public async Task<AcompanhamentoPedidoDto> AcompanharAsync(string codigo,
        CancellationToken cancellationToken = default)
    {
        var pedido = await _pedidoRepository.ObterPorCodigoAsync(NormalizarCodigo(codigo), cancellationToken)
            ?? throw new ObjectNotFoundException(MensagemNaoEncontrado);

        return _mapper.Map<AcompanhamentoPedidoDto>(pedido);
    }

    public async Task<PaginaDto<PedidoResumoDto>> ListarAsync(FiltroPedidoDto filtro,
        CancellationToken cancellationToken = default)
    {
        if (filtro.Inicio > filtro.Fim)
            throw new DomainException("A data final deve ser igual ou posterior à inicial.");

        var (pedidos, total) = await _pedidoRepository.ListarAsync(filtro, cancellationToken);
        return new PaginaDto<PedidoResumoDto>
        {
            Itens = _mapper.Map<List<PedidoResumoDto>>(pedidos),
            Total = total,
            Pagina = Math.Max(1, filtro.Pagina),
            TamanhoPagina = filtro.TamanhoPagina
        };
    }

    public async Task<PedidoDetalheDto> ObterAsync(long id, CancellationToken cancellationToken = default) =>
        _mapper.Map<PedidoDetalheDto>(await ObterPedidoAsync(id, cancellationToken));

    public async Task<UltimoPedidoDto?> ObterUltimoDoSiteAsync(CancellationToken cancellationToken = default)
    {
        var pedido = await _pedidoRepository.ObterUltimoAsync(CadastradoPor, cancellationToken);
        return pedido is null ? null : _mapper.Map<UltimoPedidoDto>(pedido);
    }

    public async Task<IReadOnlyList<PedidoDetalheDto>> ListarNovosDoSiteAsync(long depoisDe,
        CancellationToken cancellationToken = default) =>
        _mapper.Map<List<PedidoDetalheDto>>(
            await _pedidoRepository.ListarDepoisDeAsync(CadastradoPor, depoisDe, MaximoDeNovosDoSite, cancellationToken));

    public async Task<PedidoDetalheDto> CriarManualAsync(CriarPedidoDto dto, CancellationToken cancellationToken = default)
    {
        // Sem as regras do site (antecedência, loja fechada): o pedido pode ter sido combinado antes ou já entregue.
        var pedido = await MontarEGravarAsync(dto, _authentication.Login, StatusPedido.Confirmado, regrasDoSite: false,
            cancellationToken);
        return _mapper.Map<PedidoDetalheDto>(pedido);
    }

    public async Task<PedidoDetalheDto> AlterarStatusAsync(long id, StatusPedido status,
        CancellationToken cancellationToken = default)
    {
        var pedido = await ObterPedidoAsync(id, cancellationToken);

        pedido.AlterarStatus(status);

        // Pronto para entrega com parceiro: o entregador já sai para buscar. Falha do parceiro não impede a
        // mudança de etapa; o motivo fica no pedido e dá para chamar de novo.
        if (status == StatusPedido.Pronto)
            await _entregaService.ChamarEntregadorAsync(pedido, cancellationToken);

        await _pedidoRepository.SalvarAlteracoesAsync(cancellationToken);
        return _mapper.Map<PedidoDetalheDto>(pedido);
    }

    public async Task<PedidoDetalheDto> ChamarEntregadorAsync(long id, CancellationToken cancellationToken = default)
    {
        var pedido = await ObterPedidoAsync(id, cancellationToken);

        if (!pedido.PodeChamarEntregador || pedido.Status is StatusPedido.Cancelado or StatusPedido.Entregue)
            throw new DomainException(MensagemSemEntregadorParaChamar);

        await _entregaService.ChamarEntregadorAsync(pedido, cancellationToken);
        await _pedidoRepository.SalvarAlteracoesAsync(cancellationToken);
        return _mapper.Map<PedidoDetalheDto>(pedido);
    }

    public async Task ExcluirAsync(long id, CancellationToken cancellationToken = default)
    {
        var pedido = await ObterPedidoAsync(id, cancellationToken);

        pedido.Arquivar();
        await _pedidoRepository.SalvarAlteracoesAsync(cancellationToken);
    }

    private async Task<Pedido> ObterPedidoAsync(long id, CancellationToken cancellationToken) =>
        await _pedidoRepository.ObterPorIdAsync(id, cancellationToken)
        ?? throw new ObjectNotFoundException("Pedido não encontrado.");

    private async Task<Pedido> MontarEGravarAsync(CriarPedidoDto dto, string cadastradoPor, StatusPedido? status,
        bool regrasDoSite, CancellationToken cancellationToken)
    {
        var idsDosProdutos = dto.Itens.Select(i => i.ProdutoId).Distinct().ToList();
        var produtos = await _produtoRepository.ObterPorIdsAsync(idsDosProdutos, cancellationToken);

        if (produtos.Count != idsDosProdutos.Count)
            throw new DomainException("Algum doce do seu pedido não está mais disponível. Atualize a sacola e tente de novo.");

        // Encomenda ou pedido normal: decidido pelo cadastro dos produtos, não pelo navegador.
        var dataDesejada = dto.DataDesejada;
        if (regrasDoSite)
        {
            if (produtos.Any(p => p.Encomenda))
                ValidarDataDesejada(dataDesejada);
            else
                dataDesejada = await ValidarPedidoNormalAsync(cancellationToken);
        }

        var codigo = await GerarCodigoUnicoAsync(cancellationToken);
        var pedido = new Pedido(codigo, dto.NomeCliente, CriarPedidoValidation.SomenteDigitos(dto.Telefone),
            dto.TipoEntrega, MontarEndereco(dto), dataDesejada, dto.Observacao, cadastradoPor);

        // O preço vem sempre do cadastro; o valor enviado pelo navegador não é considerado.
        foreach (var item in dto.Itens)
            pedido.AdicionarItem(produtos.First(p => p.Id == item.ProdutoId), item.Quantidade);

        pedido.ValidarParaEnvio();

        // Taxa de entrega cotada de novo aqui (a do navegador não vale): a mais barata entre os parceiros.
        // Sem cotação, o pedido segue com a taxa combinada pelo WhatsApp.
        if (regrasDoSite && pedido.EnderecoEntrega is { } destino && _entregaService.Disponivel
            && await _entregaService.CotarMaisBarataAsync(destino, cancellationToken) is { } cotacao)
            pedido.DefinirTaxaEntrega(cotacao.Parceiro, cotacao.Valor);

        if (status.HasValue)
            pedido.AlterarStatus(status.Value);

        await _pedidoRepository.AdicionarAsync(pedido, cancellationToken);
        await _pedidoRepository.SalvarAlteracoesAsync(cancellationToken);
        return pedido;
    }

    private static EnderecoEntrega? MontarEndereco(CriarPedidoDto dto) =>
        dto.TipoEntrega == TipoEntrega.Entrega && dto.Endereco is { } e
            ? new EnderecoEntrega(e.Cep, e.Logradouro, e.Numero, e.Complemento, e.Bairro, e.Cidade, e.Uf)
            : null;

    /// <summary>
    /// Pedido normal (só pronta entrega): é para hoje e só com a loja aberta. A data enviada pelo navegador
    /// não vale. Sem o registro da loja no banco, ela é considerada aberta.
    /// </summary>
    private async Task<DateOnly> ValidarPedidoNormalAsync(CancellationToken cancellationToken)
    {
        var loja = await _lojaRepository.ObterAsync(cancellationToken);
        if (loja is { Aberta: false })
            throw new DomainException(MensagemLojaFechada);

        return Hoje();
    }

    /// <summary>Encomenda: exige a antecedência mínima configurada (aceita com a loja aberta ou fechada).</summary>
    private void ValidarDataDesejada(DateOnly dataDesejada)
    {
        var antecedencia = Math.Max(0, _variaveis.PEDIDO_ANTECEDENCIA_MINIMA_DIAS);
        var dataMinima = Hoje().AddDays(antecedencia);

        if (dataDesejada < dataMinima)
            throw new DomainException(antecedencia == 0
                ? "A data desejada não pode estar no passado."
                : $"Os doces sob encomenda precisam de {antecedencia} dia(s) de antecedência. Escolha a partir de {dataMinima:dd/MM/yyyy}.");
    }

    private DateOnly Hoje() => DateOnly.FromDateTime(_timeProvider.GetLocalNow().DateTime);

    /// <summary>"ei-abc 234", "EIABC234" e "abc234" viram "EI-ABC234"; fora do formato, DomainException.</summary>
    internal static string NormalizarCodigo(string? codigo)
    {
        var letras = new string((codigo ?? string.Empty).Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        if (letras.Length == TamanhoDoCodigo + 2 && letras.StartsWith("EI", StringComparison.Ordinal))
            letras = letras[2..];

        if (letras.Length != TamanhoDoCodigo || letras.Any(c => !CaracteresDoCodigo.Contains(c)))
            throw new DomainException(MensagemCodigoInvalido);

        return PrefixoCodigo + letras;
    }

    private async Task<string> GerarCodigoUnicoAsync(CancellationToken cancellationToken)
    {
        for (var tentativa = 0; tentativa < TentativasDeCodigo; tentativa++)
        {
            var codigo = PrefixoCodigo + RandomNumberGenerator.GetString(CaracteresDoCodigo, TamanhoDoCodigo);
            if (!await _pedidoRepository.ExisteCodigoAsync(codigo, cancellationToken))
                return codigo;
        }

        throw new Domain.Exceptions.ApplicationException("Não foi possível gerar o código do pedido. Tente novamente.");
    }
}
