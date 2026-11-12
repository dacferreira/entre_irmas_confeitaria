import { DataBrPipe } from './data-br.pipe';

describe('DataBrPipe', () => {
  const pipe = new DataBrPipe();

  it('formata yyyy-MM-dd sem mudar o dia', () => {
    expect(pipe.transform('2026-11-20')).toBe('20/11/2026');
    expect(pipe.transform('2026-01-01T00:00:00')).toBe('01/01/2026');
  });

  it('retorna vazio para valores inválidos', () => {
    expect(pipe.transform(null)).toBe('');
    expect(pipe.transform('20/11/2026')).toBe('');
  });
});
