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
  categoriaSlug: string;
  categoriaNome: string;
}

export interface FiltroProduto {
  categoria?: string | null;
  busca?: string | null;
  destaque?: boolean | null;
}
