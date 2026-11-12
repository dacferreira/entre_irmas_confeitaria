import { Directive, ElementRef, HostListener, inject } from '@angular/core';
import { NgControl } from '@angular/forms';

/** Formata o telefone enquanto a pessoa digita: (21) 99999-8888. */
@Directive({ selector: 'input[eirmMascaraTelefone]' })
export class MascaraTelefoneDirective {
  private readonly elemento = inject<ElementRef<HTMLInputElement>>(ElementRef);
  private readonly controle = inject(NgControl, { optional: true, self: true });

  @HostListener('input')
  aoDigitar(): void {
    const formatado = formatarTelefone(this.elemento.nativeElement.value);
    this.elemento.nativeElement.value = formatado;
    this.controle?.control?.setValue(formatado, { emitEvent: false });
  }
}

export function formatarTelefone(valor: string): string {
  const digitos = valor.replace(/\D/g, '').slice(0, 11);

  if (digitos.length === 0) {
    return '';
  }
  if (digitos.length <= 2) {
    return `(${digitos}`;
  }
  if (digitos.length <= 6) {
    return `(${digitos.slice(0, 2)}) ${digitos.slice(2)}`;
  }
  if (digitos.length <= 10) {
    return `(${digitos.slice(0, 2)}) ${digitos.slice(2, 6)}-${digitos.slice(6)}`;
  }
  return `(${digitos.slice(0, 2)}) ${digitos.slice(2, 7)}-${digitos.slice(7)}`;
}
