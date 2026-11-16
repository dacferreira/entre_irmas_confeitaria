import { dataIso, opcaoStatus, periodoDoMes } from './admin';

describe('constantes da área administrativa', () => {
  it('opcaoStatus traduz o status e cai no primeiro para valor desconhecido', () => {
    expect(opcaoStatus('EmPreparo').rotulo).toBe('Em preparo');
    expect(opcaoStatus('x' as never).valor).toBe('Recebido');
  });

  it('dataIso usa a data local com zeros à esquerda', () => {
    expect(dataIso(new Date(2026, 0, 5))).toBe('2026-01-05');
  });

  it('periodoDoMes vai do primeiro ao último dia, inclusive em fevereiro', () => {
    expect(periodoDoMes(new Date(2026, 10, 13))).toEqual({ inicio: '2026-11-01', fim: '2026-11-30' });
    expect(periodoDoMes(new Date(2028, 1, 10))).toEqual({ inicio: '2028-02-01', fim: '2028-02-29' });
  });
});
