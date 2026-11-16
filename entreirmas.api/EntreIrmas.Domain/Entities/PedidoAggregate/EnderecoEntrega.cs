using EntreIrmas.Domain.Exceptions;
using EntreIrmas.Domain.Seedwork;

namespace EntreIrmas.Domain.Entities.PedidoAggregate;

/// <summary>Endereço de entrega do pedido, em partes (CEP, logradouro, número...).</summary>
public class EnderecoEntrega : ValueObject
{
    protected EnderecoEntrega()
    {
    }

    public EnderecoEntrega(string cep, string logradouro, string numero, string? complemento, string bairro,
        string cidade, string uf)
    {
        Cep = new string((cep ?? string.Empty).Where(char.IsAsciiDigit).ToArray());
        if (Cep.Length != 8)
            throw new DomainException("Informe um CEP com 8 números.");
        if (string.IsNullOrWhiteSpace(logradouro))
            throw new DomainException("Informe a rua da entrega.");
        if (string.IsNullOrWhiteSpace(numero))
            throw new DomainException("Informe o número da entrega (ou \"s/n\").");
        if (string.IsNullOrWhiteSpace(bairro))
            throw new DomainException("Informe o bairro da entrega.");
        if (string.IsNullOrWhiteSpace(cidade))
            throw new DomainException("Informe a cidade da entrega.");
        if (string.IsNullOrWhiteSpace(uf) || uf.Trim().Length != 2)
            throw new DomainException("Informe a UF da entrega com 2 letras.");

        Logradouro = logradouro.Trim();
        Numero = numero.Trim();
        Complemento = string.IsNullOrWhiteSpace(complemento) ? null : complemento.Trim();
        Bairro = bairro.Trim();
        Cidade = cidade.Trim();
        Uf = uf.Trim().ToUpperInvariant();
    }

    /// <summary>Só os 8 dígitos.</summary>
    public string Cep { get; private set; } = string.Empty;

    public string Logradouro { get; private set; } = string.Empty;
    public string Numero { get; private set; } = string.Empty;
    public string? Complemento { get; private set; }
    public string Bairro { get; private set; } = string.Empty;
    public string Cidade { get; private set; } = string.Empty;
    public string Uf { get; private set; } = string.Empty;

    /// <summary>Uma linha para a mensagem do WhatsApp e o painel: "Rua X, 10 - Apto 2 - Bairro, Cidade/UF - CEP 00000-000".</summary>
    public string Formatar()
    {
        var complemento = Complemento is null ? string.Empty : $" - {Complemento}";
        return $"{Logradouro}, {Numero}{complemento} - {Bairro}, {Cidade}/{Uf} - CEP {Cep[..5]}-{Cep[5..]}";
    }

    protected override IEnumerable<object?> ObterComponentesDeIgualdade()
    {
        yield return Cep;
        yield return Logradouro;
        yield return Numero;
        yield return Complemento;
        yield return Bairro;
        yield return Cidade;
        yield return Uf;
    }
}
