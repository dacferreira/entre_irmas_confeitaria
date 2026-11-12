export type TipoEntrega = 'Retirada' | 'Entrega';

export interface NovoPedido {
  nomeCliente: string;
  telefone: string;
  tipoEntrega: TipoEntrega;
  endereco: string | null;
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
