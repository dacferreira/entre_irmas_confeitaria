/** Ilustrações disponíveis (coluna PROD_CD_ILUSTRACAO). */
export const ILUSTRACOES = ['bolo', 'bolo-pote', 'brownie', 'fudge', 'brigadeiro', 'cupcake', 'torta', 'salgado'] as const;

export type Ilustracao = (typeof ILUSTRACOES)[number];

export const ILUSTRACAO_PADRAO: Ilustracao = 'cupcake';

export function normalizarIlustracao(valor: string | null | undefined): Ilustracao {
  return (ILUSTRACOES as readonly string[]).includes(valor ?? '') ? (valor as Ilustracao) : ILUSTRACAO_PADRAO;
}
