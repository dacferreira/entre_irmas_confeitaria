/** Corpo das respostas 400 da API. */
export interface ValidationResultApi {
  mensagem: string;
  erros: { campo: string; mensagem: string }[];
}

/** Corpo das demais respostas de erro da API. */
export interface ErroApi {
  error: string;
}
