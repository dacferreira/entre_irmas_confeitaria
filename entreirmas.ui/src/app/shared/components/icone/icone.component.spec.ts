import { TestBed } from '@angular/core/testing';
import { IconeComponent } from './icone.component';

describe('IconeComponent', () => {
  it('desenha os traços do ícone pedido, escondido dos leitores de tela', () => {
    const fixture = TestBed.createComponent(IconeComponent);
    fixture.componentRef.setInput('nome', 'categorias');
    fixture.detectChanges();
    const svg: SVGElement = fixture.nativeElement.querySelector('svg');

    expect(svg.getAttribute('aria-hidden')).toBe('true');
    expect(svg.querySelectorAll('path').length).toBe(4);

    fixture.componentRef.setInput('nome', 'mais');
    fixture.detectChanges();
    expect(svg.querySelectorAll('path').length).toBe(2);
  });
});
