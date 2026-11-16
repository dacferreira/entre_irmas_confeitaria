import {
  CategoriaAdmin,
  Lancamento,
  Painel,
  PedidoDetalhe,
  PedidoResumo,
  ProdutoAdmin,
  Sessao,
  Usuario,
} from '@models/admin.model';
import { Observable, of } from 'rxjs';

/** Dados e dublês usados nos specs da área administrativa. */
export function criarSessao(parcial: Partial<Sessao> = {}): Sessao {
  return {
    token: 'token-de-teste',
    expiraEm: new Date(Date.now() + 3_600_000).toISOString(),
    login: 'suellen',
    nome: 'Suellen Alves',
    ...parcial,
  };
}

export function criarCategoriaAdmin(parcial: Partial<CategoriaAdmin> = {}): CategoriaAdmin {
  return {
    id: 1,
    nome: 'Bolos',
    slug: 'bolos',
    descricao: 'Bolos caseiros',
    ordem: 1,
    ativo: true,
    quantidadeProdutos: 2,
    ...parcial,
  };
}

export function criarProdutoAdmin(parcial: Partial<ProdutoAdmin> = {}): ProdutoAdmin {
  return {
    id: 10,
    categoriaId: 1,
    categoriaNome: 'Bolos',
    nome: 'Bolo de Cenoura',
    slug: 'bolo-de-cenoura',
    descricao: 'Com cobertura de chocolate',
    preco: 45,
    custo: 18,
    unidade: 'unidade',
    quantidadeMinima: 1,
    ilustracao: 'bolo',
    urlImagem: null,
    destaque: false,
    encomenda: false,
    ativo: true,
    ...parcial,
  };
}

export function criarPedidoResumo(parcial: Partial<PedidoResumo> = {}): PedidoResumo {
  return {
    id: 5,
    codigo: 'EI-ABC234',
    nomeCliente: 'Maria Souza',
    telefone: '21999998888',
    tipoEntrega: 'Retirada',
    dataDesejada: '2026-11-20',
    valorTotal: 90,
    status: 'Recebido',
    quantidadeItens: 1,
    cadastradoPor: 'SITE',
    dataEHoraDeCadastro: '2026-11-13T10:00:00',
    ...parcial,
  };
}

export function criarPedidoDetalhe(parcial: Partial<PedidoDetalhe> = {}): PedidoDetalhe {
  return {
    ...criarPedidoResumo(),
    endereco: null,
    observacao: 'Sem lactose',
    custoTotal: 36,
    itens: [
      {
        produtoId: 10,
        nomeProduto: 'Bolo de Cenoura',
        unidade: 'unidade',
        quantidade: 2,
        precoUnitario: 45,
        custoUnitario: 18,
        valorTotal: 90,
      },
    ],
    ...parcial,
  };
}

export function criarLancamento(parcial: Partial<Lancamento> = {}): Lancamento {
  return {
    id: 3,
    tipo: 'Despesa',
    descricao: 'Chocolate',
    valor: 80,
    data: '2026-11-05',
    cadastradoPor: 'tatiane',
    ...parcial,
  };
}

export function criarUsuario(parcial: Partial<Usuario> = {}): Usuario {
  return {
    id: 1,
    login: 'suellen',
    nome: 'Suellen Alves',
    ativo: true,
    ultimoAcesso: '2026-11-13T09:00:00',
    dataEHoraDeCadastro: '2026-11-01T09:00:00',
    ...parcial,
  };
}

export function criarPainel(parcial: Partial<Painel> = {}): Painel {
  return {
    inicio: '2026-11-01',
    fim: '2026-11-03',
    receita: 300,
    custoProdutos: 105,
    despesas: 30,
    perdas: 15,
    lucro: 150,
    margem: 50,
    quantidadeVendas: 2,
    ticketMedio: 150,
    aguardandoConfirmacao: { quantidade: 1, valor: 200 },
    cancelados: { quantidade: 1, valor: 60 },
    porStatus: [
      { status: 'Recebido', quantidade: 1, valor: 200 },
      { status: 'Entregue', quantidade: 2, valor: 300 },
      { status: 'Pronto', quantidade: 0, valor: 0 },
    ],
    agrupamento: 'Dia',
    serie: [
      { data: '2026-11-01', receita: 0, custos: 0, lucro: 0 },
      { data: '2026-11-02', receita: 300, custos: 135, lucro: 165 },
      { data: '2026-11-03', receita: 0, custos: 15, lucro: -15 },
    ],
    produtosMaisVendidos: [
      { produtoId: 1, nome: 'Bolo', unidade: 'unidade', quantidade: 2, receita: 200, lucro: 120 },
      { produtoId: 2, nome: 'Brigadeiro', unidade: 'unidade', quantidade: 50, receita: 100, lucro: 75 },
    ],
    ...parcial,
  };
}

/** Dublê do MatDialog: open() devolve uma referência cujo afterClosed emite o resultado informado. */
export function criarDialogoFalso(resultado: unknown = true): {
  open: jest.Mock;
  resultado: (valor: unknown) => void;
} {
  let proximo = resultado;
  return {
    open: jest.fn(() => ({ afterClosed: (): Observable<unknown> => of(proximo) })),
    resultado: (valor: unknown) => (proximo = valor),
  };
}

export function criarNotificacaoFalsa(): { sucesso: jest.Mock; erro: jest.Mock } {
  return { sucesso: jest.fn(() => of(undefined)), erro: jest.fn() };
}
