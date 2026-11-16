import { StatusPedido, TipoLancamento } from '@models/admin.model';

/** Caminhos da área administrativa (sem barra inicial). Não há link para ela no site: acesso pela URL. */
export const ROTAS_ADMIN = {
  BASE: 'admin',
  LOGIN: 'entrar',
  PAINEL: 'painel',
  PEDIDOS: 'pedidos',
  PRODUTOS: 'produtos',
  CATEGORIAS: 'categorias',
  LANCAMENTOS: 'despesas-e-perdas',
  USUARIOS: 'usuarios',
  LOJA: 'loja',
} as const;

export interface OpcaoStatus {
  valor: StatusPedido;
  rotulo: string;
  /** Classe de cor do selo (.selo--<tom>). */
  tom: 'alerta' | 'info' | 'preparo' | 'pronto' | 'sucesso' | 'neutro';
}

/** Na ordem do fluxo do pedido. */
export const STATUS_PEDIDO: readonly OpcaoStatus[] = [
  { valor: 'Recebido', rotulo: 'Aguardando confirmação', tom: 'alerta' },
  { valor: 'Confirmado', rotulo: 'Confirmado', tom: 'info' },
  { valor: 'EmPreparo', rotulo: 'Em preparo', tom: 'preparo' },
  { valor: 'Pronto', rotulo: 'Pronto', tom: 'pronto' },
  { valor: 'Entregue', rotulo: 'Entregue', tom: 'sucesso' },
  { valor: 'Cancelado', rotulo: 'Cancelado', tom: 'neutro' },
];

export function opcaoStatus(status: StatusPedido): OpcaoStatus {
  return STATUS_PEDIDO.find((s) => s.valor === status) ?? STATUS_PEDIDO[0];
}

export const TIPOS_LANCAMENTO: readonly { valor: TipoLancamento; rotulo: string; dica: string }[] = [
  { valor: 'Despesa', rotulo: 'Despesa', dica: 'Ingredientes, embalagem, gás, entrega...' },
  { valor: 'Perda', rotulo: 'Perda', dica: 'Doce descartado, encomenda desperdiçada...' },
];

/** Data local no formato yyyy-MM-dd (o que a API e o <input type="date"> usam). */
export function dataIso(data: Date): string {
  const mes = String(data.getMonth() + 1).padStart(2, '0');
  const dia = String(data.getDate()).padStart(2, '0');
  return `${data.getFullYear()}-${mes}-${dia}`;
}

/** Período do mês de uma data: [primeiro dia, último dia]. */
export function periodoDoMes(referencia: Date): { inicio: string; fim: string } {
  const inicio = new Date(referencia.getFullYear(), referencia.getMonth(), 1);
  const fim = new Date(referencia.getFullYear(), referencia.getMonth() + 1, 0);
  return { inicio: dataIso(inicio), fim: dataIso(fim) };
}
