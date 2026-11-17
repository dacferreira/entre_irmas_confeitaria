export const environment = {
  production: false,
  apiUrl: 'http://localhost:5083/api',
  timeoutMs: 20000,
  whatsappNumero: '5521990225050',
  instagramUrl: 'https://www.instagram.com/entre.irmas.confeitaria_/',
  instagramUsuario: '@entre.irmas.confeitaria_',
  enderecoLoja: 'Rua A, 41 - Paciência, Rio de Janeiro - RJ, 23580-250',
  /** Latitude,longitude da loja: o pino do mapa ("Rua A" sozinho o Google acha no lugar errado). */
  coordenadasLoja: '-22.9161966,-43.6447006',
  antecedenciaMinimaDias: 1,
  /** Libera a impressora de recibos no admin. Desligada fora da produção, para os testes não imprimirem. */
  impressoraHabilitada: false,
};
