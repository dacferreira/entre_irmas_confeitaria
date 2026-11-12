using System.Security.Cryptography;
using EntreIrmas.Domain.AppServices;
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

    // Sem 0/O e 1/I/L, para o cliente ditar o código sem confusão.
    private const string CaracteresDoCodigo = "23456789ABCDEFGHJKMNPQRSTUVWXYZ";
    private const int TamanhoDoCodigo = 6;
    private const int TentativasDeCodigo = 5;

    private readonly IPedidoRepository _pedidoRepository;
    private readonly IProdutoRepository _produtoRepository;
    private readonly IMensagemPedidoService _mensagemPedidoService;
    private readonly IPedidoEnvironmentVariables _variaveis;
    private readonly IMapper _mapper;
    private readonly TimeProvider _timeProvider;

    public PedidoAppService(IPedidoRepository pedidoRepository, IProdutoRepository produtoRepository,
        IMensagemPedidoService mensagemPedidoService, IPedidoEnvironmentVariables variaveis, IMapper mapper,
        TimeProvider timeProvider)
    {
        _pedidoRepository = pedidoRepository;
        _produtoRepository = produtoRepository;
        _mensagemPedidoService = mensagemPedidoService;
        _variaveis = variaveis;
        _mapper = mapper;
        _timeProvider = timeProvider;
    }

    public async Task<PedidoCriadoDto> CriarAsync(CriarPedidoDto dto, CancellationToken cancellationToken = default)
    {
        ValidarDataDesejada(dto.DataDesejada);

        var idsDosProdutos = dto.Itens.Select(i => i.ProdutoId).Distinct().ToList();
        var produtos = await _produtoRepository.ObterPorIdsAsync(idsDosProdutos, cancellationToken);

        if (produtos.Count != idsDosProdutos.Count)
            throw new DomainException("Algum doce do seu pedido não está mais disponível. Atualize a sacola e tente de novo.");

        var codigo = await GerarCodigoUnicoAsync(cancellationToken);
        var pedido = new Pedido(codigo, dto.NomeCliente, CriarPedidoValidation.SomenteDigitos(dto.Telefone),
            dto.TipoEntrega, dto.Endereco, dto.DataDesejada, dto.Observacao, CadastradoPor);

        // O preço vem sempre do cadastro; o valor enviado pelo navegador não é considerado.
        foreach (var item in dto.Itens)
            pedido.AdicionarItem(produtos.First(p => p.Id == item.ProdutoId), item.Quantidade);

        pedido.ValidarParaEnvio();

        await _pedidoRepository.AdicionarAsync(pedido, cancellationToken);
        await _pedidoRepository.SalvarAlteracoesAsync(cancellationToken);

        var resultado = _mapper.Map<PedidoCriadoDto>(pedido);
        resultado.LinkWhatsApp = _mensagemPedidoService.MontarLinkWhatsApp(pedido);
        return resultado;
    }

    private void ValidarDataDesejada(DateOnly dataDesejada)
    {
        var hoje = DateOnly.FromDateTime(_timeProvider.GetLocalNow().DateTime);
        var antecedencia = Math.Max(0, _variaveis.PEDIDO_ANTECEDENCIA_MINIMA_DIAS);
        var dataMinima = hoje.AddDays(antecedencia);

        if (dataDesejada < dataMinima)
            throw new DomainException(antecedencia == 0
                ? "A data desejada não pode estar no passado."
                : $"Os pedidos precisam de {antecedencia} dia(s) de antecedência. Escolha a partir de {dataMinima:dd/MM/yyyy}.");
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
