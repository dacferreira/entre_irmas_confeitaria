import { EnderecoPedido } from './endereco.model';

export type TipoEntrega = 'Retirada' | 'Entrega';

export type StatusPedido = 'Recebido' | 'Confirmado' | 'EmPreparo' | 'Pronto' | 'Entregue' | 'Cancelado';

export interface NovoPedido {
  nomeCliente: string;
  telefone: string;
  tipoEntrega: TipoEntrega;
  /** Só na entrega. */
  endereco: EnderecoPedido | null;
  /** Data no formato yyyy-MM-dd. */
  dataDesejada: string;
  observacao: string | null;
  itens: { produtoId: number; quantidade: number }[];
}

export interface ItemPedido {
  produtoId: number;
  nomeProduto: string;
  unidade: string;
  quantidade: number;
  precoUnitario: number;
  valorTotal: number;
}

/** Etapa do pedido para o cliente (sem telefone, endereço nem observação). */
export interface AcompanhamentoPedido {
  codigo: string;
  primeiroNome: string;
  tipoEntrega: TipoEntrega;
  /** Data no formato yyyy-MM-dd. */
  dataDesejada: string;
  dataDoPedido: string;
  status: StatusPedido;
  valorTotal: number;
  itens: ItemPedido[];
}

export interface PedidoCriado {
  codigo: string;
  nomeCliente: string;
  tipoEntrega: TipoEntrega;
  dataDesejada: string;
  valorTotal: number;
  status: string;
  itens: ItemPedido[];
  linkWhatsApp: string;
}
