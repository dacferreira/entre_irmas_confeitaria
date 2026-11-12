import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MascaraTelefoneDirective, formatarTelefone } from './mascara-telefone.directive';

@Component({
  imports: [ReactiveFormsModule, MascaraTelefoneDirective],
  template: `<input eirmMascaraTelefone [formControl]="telefone" />`,
})
class HospedeiroComponent {
  readonly telefone = new FormControl('', { nonNullable: true });
}

describe('MascaraTelefoneDirective', () => {
  it.each([
    ['', ''],
    ['2', '(2'],
    ['2199', '(21) 99'],
    ['2133334444', '(21) 3333-4444'],
    ['21999998888', '(21) 99999-8888'],
    ['(21) 99999-88889999', '(21) 99999-8888'],
  ])('formata "%s" como "%s"', (entrada, esperado) => {
    expect(formatarTelefone(entrada)).toBe(esperado);
  });

  it('aplica a máscara no campo e no form control', () => {
    const fixture = TestBed.createComponent(HospedeiroComponent);
    fixture.detectChanges();
    const campo: HTMLInputElement = fixture.nativeElement.querySelector('input');

    campo.value = '21999998888';
    campo.dispatchEvent(new Event('input'));

    expect(campo.value).toBe('(21) 99999-8888');
    expect(fixture.componentInstance.telefone.value).toBe('(21) 99999-8888');
  });
});
