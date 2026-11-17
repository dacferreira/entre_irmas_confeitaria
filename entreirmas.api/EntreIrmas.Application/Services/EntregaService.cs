using EntreIrmas.Domain.DTOs.Entrega;
using EntreIrmas.Domain.Entities.PedidoAggregate;
using EntreIrmas.Domain.Exceptions;
using EntreIrmas.Domain.Services;
using Microsoft.Extensions.Logging;

namespace EntreIrmas.Application.Services;

public class EntregaService : IEntregaService
{
    private readonly IReadOnlyList<IEntregaParceiroService> _parceiros;
    private readonly ILogger<EntregaService> _logger;

    public EntregaService(IEnumerable<IEntregaParceiroService> parceiros, ILogger<EntregaService> logger)
    {
        _parceiros = parceiros.ToList();
        _logger = logger;
    }

    public bool Disponivel => _parceiros.Count > 0;

    public async Task<CotacaoParceiro?> CotarMaisBarataAsync(EnderecoEntrega destino,
        CancellationToken cancellationToken = default)
    {
        // Os parceiros respondem ao mesmo tempo; um fora do ar não impede a cotação do outro.
        var cotacoes = await Task.WhenAll(_parceiros.Select(parceiro => CotarAsync(parceiro, destino, cancellationToken)));

        return cotacoes
            .OfType<CotacaoParceiro>()
            .OrderBy(c => c.Valor)
            .ThenBy(c => c.PrazoMinutos)
            .FirstOrDefault();
    }

    public async Task ChamarEntregadorAsync(Pedido pedido, CancellationToken cancellationToken = default)
    {
        if (!pedido.PodeChamarEntregador)
            return;

        var parceiro = _parceiros.FirstOrDefault(p => p.Parceiro == pedido.ParceiroEntrega);
        if (parceiro is null)
        {
            pedido.RegistrarFalhaAoChamarEntregador(
                $"A {pedido.ParceiroEntrega!.Value.Nome()} não está configurada na API. Chame o entregador pelo aplicativo.");
            return;
        }

        try
        {
            var entrega = await parceiro.SolicitarAsync(pedido, cancellationToken);
            pedido.RegistrarEntregadorChamado(entrega.IdEntrega, entrega.Rastreio);
            _logger.LogInformation("Entregador da {Parceiro} chamado para o pedido {Codigo} (entrega {Entrega})",
                parceiro.Parceiro, pedido.Codigo, entrega.IdEntrega);
        }
        catch (GatewayException erro)
        {
            // A falha já foi registrada no gateway; o pedido guarda o motivo para a confeitaria tentar de novo.
            pedido.RegistrarFalhaAoChamarEntregador(erro.Message);
        }
    }

    private async Task<CotacaoParceiro?> CotarAsync(IEntregaParceiroService parceiro, EnderecoEntrega destino,
        CancellationToken cancellationToken)
    {
        try
        {
            return await parceiro.CotarAsync(destino, cancellationToken);
        }
        catch (GatewayException erro)
        {
            _logger.LogWarning("Cotação da {Parceiro} indisponível: {Motivo}", parceiro.Parceiro, erro.Message);
            return null;
        }
    }
}
