import { Directive, ElementRef, HostListener, inject } from '@angular/core';
import { NgControl } from '@angular/forms';

/** Formata o CEP enquanto a pessoa digita: 01001-000. */
@Directive({ selector: 'input[eirmMascaraCep]' })
export class MascaraCepDirective {
  private readonly elemento = inject<ElementRef<HTMLInputElement>>(ElementRef);
  private readonly controle = inject(NgControl, { optional: true, self: true });

  @HostListener('input')
  aoDigitar(): void {
    const formatado = formatarCep(this.elemento.nativeElement.value);
    this.elemento.nativeElement.value = formatado;
    this.controle?.control?.setValue(formatado);
  }
}

export function formatarCep(valor: string): string {
  const digitos = somenteDigitos(valor).slice(0, 8);
  return digitos.length > 5 ? `${digitos.slice(0, 5)}-${digitos.slice(5)}` : digitos;
}

export function somenteDigitos(valor: string | null | undefined): string {
  return (valor ?? '').replace(/\D/g, '');
}
