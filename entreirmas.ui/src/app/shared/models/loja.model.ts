/** Situação da loja no site. Fechada, só aceita encomendas. */
export interface SituacaoLoja {
  aberta: boolean;
  /** Aviso exibido com a loja fechada (ex.: "Voltamos segunda às 9h"). */
  mensagem: string | null;
}

export interface LojaAdmin extends SituacaoLoja {
  alteradoPor: string;
  dataDaAlteracao: string;
}

export interface AlterarLoja {
  aberta: boolean;
  mensagem: string | null;
}

export const TAMANHO_MAXIMO_MENSAGEM_LOJA = 200;
