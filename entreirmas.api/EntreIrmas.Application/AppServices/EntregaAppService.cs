using EntreIrmas.Domain.AppServices;
using EntreIrmas.Domain.DTOs.Endereco;
using EntreIrmas.Domain.DTOs.Entrega;
using EntreIrmas.Domain.Entities.PedidoAggregate;
using EntreIrmas.Domain.Services;

namespace EntreIrmas.Application.AppServices;

public class EntregaAppService : IEntregaAppService
{
    internal const string MensagemTaxaCombinada = "A taxa de entrega é combinada pelo WhatsApp depois do pedido.";
    internal const string MensagemSemCotacao =
        "Não conseguimos calcular a entrega para este endereço agora. A taxa será combinada pelo WhatsApp.";

    private readonly IEntregaService _entregaService;

    public EntregaAppService(IEntregaService entregaService)
    {
        _entregaService = entregaService;
    }

    public async Task<CotacaoEntregaDto> CotarAsync(EnderecoDto endereco, CancellationToken cancellationToken = default)
    {
        if (!_entregaService.Disponivel)
            return new CotacaoEntregaDto { Disponivel = false, Mensagem = MensagemTaxaCombinada };

        var destino = new EnderecoEntrega(endereco.Cep, endereco.Logradouro, endereco.Numero, endereco.Complemento,
            endereco.Bairro, endereco.Cidade, endereco.Uf);

        var cotacao = await _entregaService.CotarMaisBarataAsync(destino, cancellationToken);
        if (cotacao is null)
            return new CotacaoEntregaDto { Disponivel = false, Mensagem = MensagemSemCotacao };

        return new CotacaoEntregaDto
        {
            Disponivel = true,
            Parceiro = cotacao.Parceiro,
            Valor = cotacao.Valor,
            PrazoMinutos = cotacao.PrazoMinutos
        };
    }
}
