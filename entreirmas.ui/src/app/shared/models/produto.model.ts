export interface Produto {
  id: number;
  nome: string;
  slug: string;
  descricao: string;
  preco: number;
  unidade: string;
  quantidadeMinima: number;
  ilustracao: string;
  urlImagem: string | null;
  destaque: boolean;
  /** Feito sob encomenda: exige a antecedência mínima no pedido. */
  encomenda: boolean;
  categoriaSlug: string;
  categoriaNome: string;
}

export interface FiltroProduto {
  categoria?: string | null;
  busca?: string | null;
  destaque?: boolean | null;
}
