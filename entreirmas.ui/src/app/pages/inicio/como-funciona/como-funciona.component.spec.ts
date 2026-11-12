import { TestBed } from '@angular/core/testing';
import { ComoFuncionaComponent } from './como-funciona.component';

describe('ComoFuncionaComponent', () => {
  it('lista os três passos com a âncora da seção', () => {
    const fixture = TestBed.createComponent(ComoFuncionaComponent);
    fixture.detectChanges();
    const elemento: HTMLElement = fixture.nativeElement;

    expect(elemento.querySelector('#como-funciona')).not.toBeNull();
    expect(Array.from(elemento.querySelectorAll('.passo__numero')).map((n) => n.textContent)).toEqual(['1', '2', '3']);
    expect(elemento.textContent).toContain('Confirme no WhatsApp');
  });
});
