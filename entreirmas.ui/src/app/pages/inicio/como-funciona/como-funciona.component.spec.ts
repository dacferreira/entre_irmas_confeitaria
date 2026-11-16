import { TestBed } from '@angular/core/testing';
import { environment } from '@environments/environment';
import { ComoFuncionaComponent } from './como-funciona.component';

describe('ComoFuncionaComponent', () => {
  it('explica os dois fluxos (pedido normal e encomenda) com a âncora da seção', () => {
    const fixture = TestBed.createComponent(ComoFuncionaComponent);
    fixture.detectChanges();
    const elemento: HTMLElement = fixture.nativeElement;

    expect(elemento.querySelector('#como-funciona')).not.toBeNull();
    const fluxos = Array.from(elemento.querySelectorAll('.fluxo'));
    expect(fluxos.map((f) => f.querySelector('.fluxo__titulo')?.textContent)).toEqual(['Pedido normal', 'Encomenda']);
    for (const fluxo of fluxos) {
      expect(Array.from(fluxo.querySelectorAll('.passo__titulo')).map((t) => t.textContent)).toContain('Pague');
      expect(fluxo.querySelectorAll('.passo__numero').length).toBe(3);
    }
    expect(fluxos[1].textContent).toContain(`${environment.antecedenciaMinimaDias} dia(s)`);
    expect(elemento.textContent).toContain('aceitamos só encomendas');
  });
});
