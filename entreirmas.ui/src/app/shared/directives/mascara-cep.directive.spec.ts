import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MascaraCepDirective, formatarCep, somenteDigitos } from './mascara-cep.directive';

@Component({
  imports: [ReactiveFormsModule, MascaraCepDirective],
  template: `<input eirmMascaraCep [formControl]="cep" />`,
})
class HospedeiroComponent {
  readonly cep = new FormControl('', { nonNullable: true });
}

describe('MascaraCepDirective', () => {
  it.each([
    ['', ''],
    ['010', '010'],
    ['01001', '01001'],
    ['010010', '01001-0'],
    ['01001000', '01001-000'],
    ['01001-0009999', '01001-000'],
  ])('formata "%s" como "%s"', (entrada, esperado) => {
    expect(formatarCep(entrada)).toBe(esperado);
  });

  it('extrai só os dígitos, aceitando vazio', () => {
    expect(somenteDigitos('01.001-000')).toBe('01001000');
    expect(somenteDigitos(null)).toBe('');
    expect(somenteDigitos(undefined)).toBe('');
  });

  it('aplica a máscara no campo e no form control', () => {
    const fixture = TestBed.createComponent(HospedeiroComponent);
    fixture.detectChanges();
    const campo: HTMLInputElement = fixture.nativeElement.querySelector('input');

    campo.value = '01001000';
    campo.dispatchEvent(new Event('input'));

    expect(campo.value).toBe('01001-000');
    expect(fixture.componentInstance.cep.value).toBe('01001-000');
  });
});
