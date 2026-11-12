import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/** Telefone com DDD: 10 ou 11 dígitos, com ou sem máscara. */
export const telefoneValidator: ValidatorFn = (controle: AbstractControl<string>): ValidationErrors | null => {
  const digitos = (controle.value ?? '').replace(/\D/g, '');
  return digitos.length === 0 || digitos.length === 10 || digitos.length === 11 ? null : { telefone: true };
};

/** Data (yyyy-MM-dd) igual ou posterior à mínima. */
export function dataMinimaValidator(dataMinima: string): ValidatorFn {
  return (controle: AbstractControl<string>): ValidationErrors | null =>
    controle.value && controle.value < dataMinima ? { dataMinima: { minima: dataMinima } } : null;
}

/** Data local de hoje + dias, no formato yyyy-MM-dd (sem conversão para UTC). */
export function dataDaquiA(dias: number, hoje: Date = new Date()): string {
  const data = new Date(hoje.getFullYear(), hoje.getMonth(), hoje.getDate() + Math.max(0, dias));
  const mes = String(data.getMonth() + 1).padStart(2, '0');
  const dia = String(data.getDate()).padStart(2, '0');
  return `${data.getFullYear()}-${mes}-${dia}`;
}
