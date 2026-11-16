/** Endereço encontrado pelo CEP (resposta de GET /api/enderecos/cep/{cep}). */
export interface EnderecoCep {
  cep: string;
  logradouro: string;
  complemento: string;
  bairro: string;
  cidade: string;
  uf: string;
}

/** Endereço de entrega enviado no pedido. */
export interface EnderecoPedido {
  /** Só os 8 dígitos. */
  cep: string;
  logradouro: string;
  numero: string;
  complemento: string | null;
  bairro: string;
  cidade: string;
  uf: string;
}
