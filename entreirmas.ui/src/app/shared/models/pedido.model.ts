import { EnderecoPedido } from './endereco.model';

export type TipoEntrega = 'Retirada' | 'Entrega';

export type StatusPedido = 'Recebido' | 'Confirmado' | 'EmPreparo' | 'Pronto' | 'Entregue' | 'Cancelado';

/** Empresa que faz a entrega. */
export type ParceiroEntrega = 'Uber' | 'NoventaENove';

/** Nome para mostrar: "Uber" ou "99". */
export function nomeDoParceiro(parceiro: ParceiroEntrega | null | undefined): string {
  return parceiro === 'NoventaENove' ? '99' : parceiro === 'Uber' ? 'Uber' : '';
}

/** "Calcular entrega" do checkout: a cotação mais barata ou o aviso de taxa combinada. */
export interface CotacaoEntrega {
  disponivel: boolean;
  parceiro: ParceiroEntrega | null;
  valor: number | null;
  /** Minutos até a entrega, depois que o entregador é chamado. */
  prazoMinutos: number | null;
  mensagem: string | null;
}

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
  /** Taxa de entrega cobrada (nula na retirada e quando é combinada pelo WhatsApp). */
  taxaEntrega: number | null;
  parceiroEntrega: ParceiroEntrega | null;
  /** Itens + taxa de entrega. */
  totalAPagar: number;
  /** Link para acompanhar o entregador, depois que ele é chamado. */
  rastreioEntrega: string | null;
  itens: ItemPedido[];
}

export interface PedidoCriado {
  codigo: string;
  nomeCliente: string;
  tipoEntrega: TipoEntrega;
  dataDesejada: string;
  valorTotal: number;
  /** Taxa de entrega cobrada (nula na retirada e quando é combinada pelo WhatsApp). */
  taxaEntrega: number | null;
  parceiroEntrega: ParceiroEntrega | null;
  /** Itens + taxa de entrega. */
  totalAPagar: number;
  status: string;
  itens: ItemPedido[];
  linkWhatsApp: string;
}
