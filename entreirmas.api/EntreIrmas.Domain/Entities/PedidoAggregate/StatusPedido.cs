namespace EntreIrmas.Domain.Entities.PedidoAggregate;

/// <summary>Etapas do pedido (gravadas por nome na coluna PEDI_CD_STATUS).</summary>
public enum StatusPedido
{
    Recebido,
    Confirmado,
    EmPreparo,
    Pronto,
    Entregue,
    Cancelado
}
