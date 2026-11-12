import { TestBed } from '@angular/core/testing';
import { ILUSTRACOES } from '@constants/ilustracoes';
import { IlustracaoComponent } from './ilustracao.component';

describe('IlustracaoComponent', () => {
  function renderizar(tipo: string | null, rotulo = ''): SVGElement {
    const fixture = TestBed.createComponent(IlustracaoComponent);
    fixture.componentRef.setInput('tipo', tipo);
    fixture.componentRef.setInput('rotulo', rotulo);
    fixture.detectChanges();
    return fixture.nativeElement.querySelector('svg');
  }

  it.each(ILUSTRACOES)('desenha a ilustração "%s"', (tipo) => {
    const svg = renderizar(tipo);

    expect(svg.getAttribute('data-tipo')).toBe(tipo);
    expect(svg.querySelectorAll('path, circle, rect, ellipse').length).toBeGreaterThan(3);
  });

  it('usa o cupcake para tipos desconhecidos', () => {
    expect(renderizar('pudim').getAttribute('data-tipo')).toBe('cupcake');
    expect(renderizar(null).getAttribute('data-tipo')).toBe('cupcake');
  });

  it('é decorativa sem rótulo e acessível com rótulo', () => {
    expect(renderizar('bolo').getAttribute('aria-hidden')).toBe('true');

    const comRotulo = renderizar('bolo', 'Bolo de morango');
    expect(comRotulo.getAttribute('role')).toBe('img');
    expect(comRotulo.getAttribute('aria-label')).toBe('Bolo de morango');
  });
});
