namespace EntreIrmas.Domain.Entities.PedidoAggregate;

/// <summary>Chamada do entregador no parceiro (nula enquanto não foi chamado).</summary>
public enum SituacaoEntrega
{
    /// <summary>O parceiro aceitou: o entregador está a caminho da loja.</summary>
    Solicitada,

    /// <summary>A chamada não deu certo; dá para tentar de novo pela área administrativa.</summary>
    Falhou
}
