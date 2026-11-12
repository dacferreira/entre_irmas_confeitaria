import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { PageComponent } from './page.component';

@Component({
  imports: [PageComponent],
  template: `
    <eirm-page titulo="Cardápio" sobretitulo="Sob encomenda" subtitulo="Escolha" largura="estreita" alinhamento="centro">
      <button acoes type="button">Ação</button>
      <p class="conteudo">Conteúdo projetado</p>
    </eirm-page>
  `,
})
class HospedeiroComponent {}

@Component({
  imports: [PageComponent],
  template: `<eirm-page titulo="Só o título" />`,
})
class SoTituloComponent {}

describe('PageComponent', () => {
  it('mostra títulos, ações e conteúdo projetado com as variações de layout', () => {
    const fixture = TestBed.createComponent(HospedeiroComponent);
    fixture.detectChanges();
    const elemento: HTMLElement = fixture.nativeElement;

    expect(elemento.querySelector('h1')?.textContent).toContain('Cardápio');
    expect(elemento.querySelector('.sobretitulo')?.textContent).toContain('Sob encomenda');
    expect(elemento.querySelector('.pagina__subtitulo')?.textContent).toContain('Escolha');
    expect(elemento.querySelector('.pagina__cabecalho button')?.textContent).toBe('Ação');
    expect(elemento.querySelector('.pagina__conteudo .conteudo')).not.toBeNull();
    expect(elemento.querySelector('.pagina--estreita.pagina--centro')).not.toBeNull();
  });

  it('omite sobretítulo e subtítulo quando não informados', () => {
    const fixture = TestBed.createComponent(SoTituloComponent);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('.sobretitulo')).toBeNull();
    expect(fixture.nativeElement.querySelector('.pagina__subtitulo')).toBeNull();
  });
});
