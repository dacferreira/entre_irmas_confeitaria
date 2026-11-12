import { FormControl } from '@angular/forms';
import { dataDaquiA, dataMinimaValidator, telefoneValidator } from './pedido.validators';

describe('pedido.validators', () => {
  it('telefoneValidator aceita 10 ou 11 dígitos (ou vazio, que fica para o required)', () => {
    expect(telefoneValidator(new FormControl('(21) 99999-8888'))).toBeNull();
    expect(telefoneValidator(new FormControl('2133334444'))).toBeNull();
    expect(telefoneValidator(new FormControl(''))).toBeNull();
    expect(telefoneValidator(new FormControl('9999-8888'))).toEqual({ telefone: true });
  });

  it('dataMinimaValidator recusa datas anteriores à mínima', () => {
    const validador = dataMinimaValidator('2026-11-12');

    expect(validador(new FormControl('2026-11-12'))).toBeNull();
    expect(validador(new FormControl(''))).toBeNull();
    expect(validador(new FormControl('2026-11-11'))).toEqual({ dataMinima: { minima: '2026-11-12' } });
  });

  it('dataDaquiA soma dias na data local, virando mês e ano', () => {
    const hoje = new Date(2026, 11, 31, 23, 30);

    expect(dataDaquiA(0, hoje)).toBe('2026-12-31');
    expect(dataDaquiA(1, hoje)).toBe('2027-01-01');
    expect(dataDaquiA(-5, hoje)).toBe('2026-12-31');
  });
});
