import { Component, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SeletorQuantidadeComponent } from './seletor-quantidade.component';

@Component({
  imports: [SeletorQuantidadeComponent],
  template: `<eirm-seletor-quantidade [(valor)]="quantidade" [minimo]="minimo" [maximo]="30" rotulo="Brigadeiros" />`,
})
class HospedeiroComponent {
  readonly quantidade = signal(25);
  minimo = 25;
}

describe('SeletorQuantidadeComponent', () => {
  let fixture: ComponentFixture<HospedeiroComponent>;
  let botoes: HTMLButtonElement[];
  let campo: HTMLInputElement;

  beforeEach(() => {
    fixture = TestBed.createComponent(HospedeiroComponent);
    fixture.detectChanges();
    botoes = Array.from(fixture.nativeElement.querySelectorAll('button'));
    campo = fixture.nativeElement.querySelector('input');
  });

  it('desabilita o "menos" no mínimo e aumenta com o "mais"', () => {
    expect(botoes[0].disabled).toBe(true);
    expect(botoes[0].getAttribute('aria-label')).toBe('Diminuir brigadeiros');

    botoes[1].click();
    fixture.detectChanges();

    expect(fixture.componentInstance.quantidade()).toBe(26);
    expect(botoes[0].disabled).toBe(false);

    botoes[0].click();
    expect(fixture.componentInstance.quantidade()).toBe(25);
  });

  it('limita o valor digitado entre o mínimo e o máximo', () => {
    campo.value = '3';
    campo.dispatchEvent(new Event('change'));
    expect(fixture.componentInstance.quantidade()).toBe(25);
    expect(campo.value).toBe('25');

    campo.value = '999';
    campo.dispatchEvent(new Event('change'));
    expect(fixture.componentInstance.quantidade()).toBe(30);

    campo.value = 'abc';
    campo.dispatchEvent(new Event('change'));
    expect(fixture.componentInstance.quantidade()).toBe(25);
  });

  it('desabilita o "mais" no máximo', () => {
    fixture.componentInstance.quantidade.set(30);
    fixture.detectChanges();

    expect(botoes[1].disabled).toBe(true);
  });
});
