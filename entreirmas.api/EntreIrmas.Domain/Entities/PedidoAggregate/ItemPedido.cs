using EntreIrmas.Domain.Entities.ProdutoAggregate;
using EntreIrmas.Domain.Exceptions;
using EntreIrmas.Domain.Seedwork;

namespace EntreIrmas.Domain.Entities.PedidoAggregate;

/// <summary>Produto pedido. Nome e preço são copiados para o histórico não mudar com o cardápio.</summary>
public class ItemPedido : Entity
{
    protected ItemPedido()
    {
    }

    internal ItemPedido(Produto produto, int quantidade, string cadastradoPor)
    {
        if (quantidade < 1)
            throw new DomainException("A quantidade deve ser de pelo menos 1.");

        ProdutoId = produto.Id;
        NomeProduto = produto.Nome;
        Unidade = produto.Unidade;
        PrecoUnitario = produto.Preco;
        Quantidade = quantidade;
        RecalcularTotal();
        DefinirCadastro(cadastradoPor);
    }

    public long PedidoId { get; private set; }
    public long ProdutoId { get; private set; }
    public string NomeProduto { get; private set; } = string.Empty;
    public string Unidade { get; private set; } = string.Empty;
    public int Quantidade { get; private set; }
    public decimal PrecoUnitario { get; private set; }
    public decimal ValorTotal { get; private set; }

    internal void SomarQuantidade(int quantidade)
    {
        if (quantidade < 1)
            throw new DomainException("A quantidade deve ser de pelo menos 1.");

        Quantidade += quantidade;
        RecalcularTotal();
    }

    private void RecalcularTotal() => ValorTotal = PrecoUnitario * Quantidade;
}
