import { computed, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SituacaoLoja } from '@models/loja.model';
import { LojaService } from '@services/loja.service';
import { AvisoLojaComponent } from './aviso-loja.component';

describe('AvisoLojaComponent', () => {
  const situacao = signal<SituacaoLoja>({ aberta: true, mensagem: null });
  const loja = { situacao, aberta: computed(() => situacao().aberta), atualizar: jest.fn() };
  let fixture: ComponentFixture<AvisoLojaComponent>;

  beforeEach(() => {
    situacao.set({ aberta: true, mensagem: null });
    TestBed.configureTestingModule({ providers: [{ provide: LojaService, useValue: loja }] });
    fixture = TestBed.createComponent(AvisoLojaComponent);
    fixture.detectChanges();
  });

  it('consulta a situação ao abrir e quando a aba volta ao foco', () => {
    expect(loja.atualizar).toHaveBeenCalledTimes(1);

    window.dispatchEvent(new Event('focus'));

    expect(loja.atualizar).toHaveBeenCalledTimes(2);
  });

  it('aberta não mostra nada; fechada mostra o aviso com a mensagem e a regra das encomendas', () => {
    const elemento: HTMLElement = fixture.nativeElement;
    expect(elemento.querySelector('.aviso')).toBeNull();

    situacao.set({ aberta: false, mensagem: 'Voltamos segunda às 9h.' });
    fixture.detectChanges();

    const aviso = elemento.querySelector('[role="status"]');
    expect(aviso?.textContent).toContain('Estamos fechados agora.');
    expect(aviso?.textContent).toContain('Voltamos segunda às 9h.');
    expect(aviso?.textContent).toContain('encomendas');
  });
});
