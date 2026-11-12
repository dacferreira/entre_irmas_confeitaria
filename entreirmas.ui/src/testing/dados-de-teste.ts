import { Categoria } from '@models/categoria.model';
import { PedidoCriado } from '@models/pedido.model';
import { Produto } from '@models/produto.model';

/** Dados usados nos specs. */
export function criarProduto(parcial: Partial<Produto> = {}): Produto {
  return {
    id: 1,
    nome: 'Brownie Tradicional',
    slug: 'brownie-tradicional',
    descricao: 'Casquinha crocante e molhadinho por dentro.',
    preco: 9,
    unidade: 'unidade',
    quantidadeMinima: 1,
    ilustracao: 'brownie',
    urlImagem: null,
    destaque: true,
    categoriaSlug: 'brownies',
    categoriaNome: 'Brownies & Fudges',
    ...parcial,
  };
}

export const BRIGADEIRO = criarProduto({
  id: 2,
  nome: 'Brigadeiro Gourmet',
  slug: 'brigadeiro-gourmet',
  descricao: 'Chocolate nobre.',
  preco: 2.5,
  quantidadeMinima: 25,
  ilustracao: 'brigadeiro',
  categoriaSlug: 'docinhos',
  categoriaNome: 'Docinhos',
});

export const BICHO_DE_PE = criarProduto({
  id: 3,
  nome: 'Bicho de Pé',
  slug: 'bicho-de-pe',
  descricao: 'Docinho de morango.',
  preco: 2.5,
  quantidadeMinima: 25,
  ilustracao: 'brigadeiro',
  destaque: false,
  categoriaSlug: 'docinhos',
  categoriaNome: 'Docinhos',
});

export const PRODUTOS: Produto[] = [criarProduto(), BRIGADEIRO, BICHO_DE_PE];

export const CATEGORIAS: Categoria[] = [
  { id: 1, nome: 'Brownies & Fudges', slug: 'brownies', descricao: null, quantidadeProdutos: 1 },
  { id: 2, nome: 'Docinhos', slug: 'docinhos', descricao: null, quantidadeProdutos: 2 },
];

export function criarPedido(parcial: Partial<PedidoCriado> = {}): PedidoCriado {
  return {
    codigo: 'EI-ABC234',
    nomeCliente: 'Maria',
    tipoEntrega: 'Retirada',
    dataDesejada: '2026-11-20',
    valorTotal: 18,
    status: 'Recebido',
    itens: [
      { produtoId: 1, nomeProduto: 'Brownie Tradicional', unidade: 'unidade', quantidade: 2, precoUnitario: 9, valorTotal: 18 },
    ],
    linkWhatsApp: 'https://wa.me/5521990225050?text=ola',
    ...parcial,
  };
}
