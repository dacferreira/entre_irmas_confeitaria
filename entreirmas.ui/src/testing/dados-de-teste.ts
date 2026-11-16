import { Categoria } from '@models/categoria.model';
import { AcompanhamentoPedido, PedidoCriado } from '@models/pedido.model';
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
    encomenda: false,
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

/** Endereço como digitado no formulário (com máscara e espaços). */
export const ENDERECO_DIGITADO = {
  cep: '20000-000',
  logradouro: ' Rua das Flores ',
  numero: '10',
  complemento: ' ',
  bairro: 'Centro',
  cidade: 'Rio de Janeiro',
  uf: 'RJ',
};

/** O mesmo endereço como vai para a API. */
export const ENDERECO_ENVIADO = {
  cep: '20000000',
  logradouro: 'Rua das Flores',
  numero: '10',
  complemento: null,
  bairro: 'Centro',
  cidade: 'Rio de Janeiro',
  uf: 'RJ',
};

export const CATEGORIAS: Categoria[] = [
  { id: 1, nome: 'Brownies & Fudges', slug: 'brownies', descricao: null, quantidadeProdutos: 1 },
  { id: 2, nome: 'Docinhos', slug: 'docinhos', descricao: null, quantidadeProdutos: 2 },
];

export function criarAcompanhamento(parcial: Partial<AcompanhamentoPedido> = {}): AcompanhamentoPedido {
  return {
    codigo: 'EI-ABC234',
    primeiroNome: 'Maria',
    tipoEntrega: 'Retirada',
    dataDesejada: '2026-11-20',
    dataDoPedido: '2026-11-14T15:30:00',
    status: 'Recebido',
    valorTotal: 18,
    itens: [
      { produtoId: 1, nomeProduto: 'Brownie Tradicional', unidade: 'unidade', quantidade: 2, precoUnitario: 9, valorTotal: 18 },
    ],
    ...parcial,
  };
}

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
