using EntreIrmas.Domain.DTOs.Entrega;
using EntreIrmas.Domain.Entities.PedidoAggregate;
using EntreIrmas.Domain.EnviromentVariables;
using EntreIrmas.Domain.Services;

namespace EntreIrmas.Application.Services;

/// <summary>
/// Simulador de Uber e 99 para o ambiente local e os testes (ENTREGA_SIMULADOR = true): não chama ninguém.
/// A "distância" sai da diferença entre o CEP da loja e o do cliente, então o mesmo endereço tem sempre o mesmo
/// valor; a Uber sai mais barata perto da loja e a 99 mais longe, para dar para ver as duas no checkout.
/// Fora da UF da loja, o simulador responde que não atende.
/// </summary>
public class EntregaSimuladaService : IEntregaParceiroService
{
    internal const string PrefixoDoId = "simulada-";

    private readonly IEntregaEnvironmentVariables _variaveis;

    public EntregaSimuladaService(ParceiroEntrega parceiro, IEntregaEnvironmentVariables variaveis)
    {
        Parceiro = parceiro;
        _variaveis = variaveis;
    }

    public ParceiroEntrega Parceiro { get; }

    public Task<CotacaoParceiro?> CotarAsync(EnderecoEntrega destino, CancellationToken cancellationToken = default)
    {
        if (!string.Equals(destino.Uf, _variaveis.LOJA_UF, StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<CotacaoParceiro?>(null);

        var km = DistanciaSimuladaKm(_variaveis.LOJA_CEP, destino.Cep);
        // As retas se cruzam em 5,6 km: antes a Uber é mais barata, depois a 99.
        var valor = Parceiro == ParceiroEntrega.Uber ? 6.50m + 1.65m * km : 7.90m + 1.40m * km;
        var prazo = 15 + (int)Math.Round(km * 2.5m);

        return Task.FromResult<CotacaoParceiro?>(new CotacaoParceiro(Parceiro, decimal.Round(valor, 2), prazo));
    }

    public Task<EntregaSolicitada> SolicitarAsync(Pedido pedido, CancellationToken cancellationToken = default) =>
        Task.FromResult(new EntregaSolicitada($"{PrefixoDoId}{Parceiro.ToString().ToLowerInvariant()}-{pedido.Codigo}", null));

    /// <summary>De 1,5 km (mesmo trecho de CEP) a 35 km, pelos 5 primeiros dígitos dos CEPs.</summary>
    internal static decimal DistanciaSimuladaKm(string cepDaLoja, string cepDoCliente)
    {
        var loja = int.TryParse(cepDaLoja.AsSpan(0, Math.Min(5, cepDaLoja.Length)), out var l) ? l : 0;
        var cliente = int.TryParse(cepDoCliente.AsSpan(0, Math.Min(5, cepDoCliente.Length)), out var c) ? c : 0;
        return Math.Clamp(1.5m + Math.Abs(loja - cliente) * 0.15m, 1.5m, 35m);
    }
}
