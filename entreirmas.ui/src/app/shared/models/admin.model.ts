import { StatusPedido, TipoEntrega } from './pedido.model';

/** Resposta do login (guardada no localStorage). */
export interface Sessao {
  token: string;
  /** Data e hora em UTC (ISO 8601). */
  expiraEm: string;
  login: string;
  nome: string;
}

export interface Pagina<T> {
  itens: T[];
  total: number;
  pagina: number;
  tamanhoPagina: number;
}

// ---------- Catálogo ----------
export interface CategoriaAdmin {
  id: number;
  nome: string;
  slug: string;
  descricao: string | null;
  ordem: number;
  ativo: boolean;
  quantidadeProdutos: number;
}

export interface SalvarCategoria {
  nome: string;
  descricao: string | null;
  ordem: number;
}

export interface ProdutoAdmin {
  id: number;
  categoriaId: number;
  categoriaNome: string;
  nome: string;
  slug: string;
  descricao: string;
  preco: number;
  custo: number;
  unidade: string;
  quantidadeMinima: number;
  ilustracao: string;
  urlImagem: string | null;
  destaque: boolean;
  encomenda: boolean;
  ativo: boolean;
}

/** A foto tem envio próprio (ProdutoAdminService.enviarImagem). */
export type SalvarProduto = Omit<ProdutoAdmin, 'id' | 'categoriaNome' | 'slug' | 'ativo' | 'urlImagem'>;

export interface FiltroProdutoAdmin {
  categoriaId?: number | null;
  busca?: string | null;
  ativo?: boolean | null;
}

// ---------- Pedidos ----------
/** Último pedido feito pelo site (aviso de pedido novo). */
export interface UltimoPedido {
  id: number;
  codigo: string;
  nomeCliente: string;
}

// O status mora no modelo do pedido (o cliente também acompanha as etapas).
export type { StatusPedido };

export interface FiltroPedido {
  inicio?: string | null;
  fim?: string | null;
  status?: StatusPedido | null;
  busca?: string | null;
  pagina?: number;
  tamanhoPagina?: number;
}

export interface PedidoResumo {
  id: number;
  codigo: string;
  nomeCliente: string;
  telefone: string;
  tipoEntrega: TipoEntrega;
  /** yyyy-MM-dd */
  dataDesejada: string;
  valorTotal: number;
  status: StatusPedido;
  quantidadeItens: number;
  cadastradoPor: string;
  dataEHoraDeCadastro: string;
}

export interface ItemPedidoAdmin {
  produtoId: number;
  nomeProduto: string;
  unidade: string;
  quantidade: number;
  precoUnitario: number;
  custoUnitario: number;
  valorTotal: number;
}

export interface PedidoDetalhe extends PedidoResumo {
  endereco: string | null;
  observacao: string | null;
  custoTotal: number;
  itens: ItemPedidoAdmin[];
}

// ---------- Despesas e perdas ----------
export type TipoLancamento = 'Despesa' | 'Perda';

export interface Lancamento {
  id: number;
  tipo: TipoLancamento;
  descricao: string;
  valor: number;
  /** yyyy-MM-dd */
  data: string;
  cadastradoPor: string;
}

export type SalvarLancamento = Omit<Lancamento, 'id' | 'cadastradoPor'>;

export interface FiltroLancamento {
  inicio?: string | null;
  fim?: string | null;
  tipo?: TipoLancamento | null;
}

// ---------- Usuários ----------
export interface Usuario {
  id: number;
  login: string;
  nome: string;
  ativo: boolean;
  ultimoAcesso: string | null;
  dataEHoraDeCadastro: string;
}

export interface SalvarUsuario {
  login: string;
  nome: string;
  /** Na edição, vazio mantém a senha atual. */
  senha: string | null;
}

// ---------- Painel ----------
export interface ResumoPedidos {
  quantidade: number;
  valor: number;
}

export interface PontoDaSerie {
  /** yyyy-MM-dd (primeiro dia do dia ou do mês) */
  data: string;
  receita: number;
  custos: number;
  lucro: number;
}

export interface Painel {
  inicio: string;
  fim: string;
  receita: number;
  custoProdutos: number;
  despesas: number;
  perdas: number;
  lucro: number;
  margem: number | null;
  quantidadeVendas: number;
  ticketMedio: number;
  aguardandoConfirmacao: ResumoPedidos;
  cancelados: ResumoPedidos;
  porStatus: { status: StatusPedido; quantidade: number; valor: number }[];
  agrupamento: 'Dia' | 'Mes';
  serie: PontoDaSerie[];
  produtosMaisVendidos: {
    produtoId: number;
    nome: string;
    unidade: string;
    quantidade: number;
    receita: number;
    lucro: number;
  }[];
}
