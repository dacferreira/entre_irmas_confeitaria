using EntreIrmas.Domain.Entities.ProdutoAggregate;
using EntreIrmas.Domain.Exceptions;
using EntreIrmas.Domain.Seedwork;

namespace EntreIrmas.Domain.Entities.PedidoAggregate;

/// <summary>Encomenda feita pelo site.</summary>
public class Pedido : Entity, IAggregateRoot
{
    private readonly List<ItemPedido> _itens = new();

    protected Pedido()
    {
    }

    public Pedido(string codigo, string nomeCliente, string telefone, TipoEntrega tipoEntrega,
        EnderecoEntrega? enderecoEntrega, DateOnly dataDesejada, string? observacao, string cadastradoPor)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new DomainException("O código do pedido é obrigatório.");
        if (string.IsNullOrWhiteSpace(nomeCliente))
            throw new DomainException("Informe o seu nome.");
        if (string.IsNullOrWhiteSpace(telefone))
            throw new DomainException("Informe um telefone para contato.");
        if (tipoEntrega == TipoEntrega.Entrega && enderecoEntrega is null)
            throw new DomainException("Informe o endereço para a entrega.");

        Codigo = codigo;
        NomeCliente = nomeCliente.Trim();
        Telefone = telefone.Trim();
        TipoEntrega = tipoEntrega;
        // Na retirada, um endereço enviado por engano é descartado.
        EnderecoEntrega = tipoEntrega == TipoEntrega.Entrega ? enderecoEntrega : null;
        Endereco = EnderecoEntrega?.Formatar();
        DataDesejada = dataDesejada;
        Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
        Status = StatusPedido.Recebido;
        DefinirCadastro(cadastradoPor);
    }

    public string Codigo { get; private set; } = string.Empty;
    public string NomeCliente { get; private set; } = string.Empty;
    public string Telefone { get; private set; } = string.Empty;
    public TipoEntrega TipoEntrega { get; private set; }
    /// <summary>Endereço de entrega em partes (nulo na retirada).</summary>
    public EnderecoEntrega? EnderecoEntrega { get; private set; }

    /// <summary>Endereço em uma linha, para a mensagem do WhatsApp e o painel (nulo na retirada).</summary>
    public string? Endereco { get; private set; }
    public DateOnly DataDesejada { get; private set; }
    public string? Observacao { get; private set; }
    public decimal ValorTotal { get; private set; }
    public StatusPedido Status { get; private set; }
    public IReadOnlyCollection<ItemPedido> Itens => _itens;

    /// <summary>Custo dos produtos do pedido, pelo custo copiado em cada item.</summary>
    public decimal CustoTotal => _itens.Sum(i => i.CustoTotal);

    /// <summary>Pedidos que contam como venda: confirmados pela confeitaria e não cancelados.</summary>
    public bool ContaComoVenda => Status is not (StatusPedido.Recebido or StatusPedido.Cancelado);

    public void AlterarStatus(StatusPedido status)
    {
        if (!Enum.IsDefined(status))
            throw new DomainException("Status do pedido inválido.");

        Status = status;
    }

    /// <summary>Adiciona o produto respeitando o pedido mínimo; repetir o produto soma a quantidade.</summary>
    public void AdicionarItem(Produto produto, int quantidade)
    {
        var existente = _itens.FirstOrDefault(i => i.ProdutoId == produto.Id);
        var quantidadeFinal = quantidade + (existente?.Quantidade ?? 0);

        if (quantidadeFinal < produto.QuantidadeMinima)
            throw new DomainException(
                $"O pedido mínimo de {produto.Nome} é de {produto.QuantidadeMinima} ({produto.Unidade}).");

        if (existente is null)
            _itens.Add(new ItemPedido(produto, quantidade, CadastradoPor));
        else
            existente.SomarQuantidade(quantidade);

        ValorTotal = _itens.Sum(i => i.ValorTotal);
    }

    /// <summary>Garante que o pedido tem o necessário para ser gravado.</summary>
    public void ValidarParaEnvio()
    {
        if (_itens.Count == 0)
            throw new DomainException("Adicione pelo menos um doce ao pedido.");
    }
}
