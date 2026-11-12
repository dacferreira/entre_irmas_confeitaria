import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { SacolaService } from '@services/sacola.service';
import { BRIGADEIRO, criarProduto } from '@testing/dados-de-teste';
import { SacolaComponent } from './sacola.component';

@Component({ template: '' })
class TelaVaziaComponent {}

describe('SacolaComponent', () => {
  let fixture: ComponentFixture<SacolaComponent>;
  let elemento: HTMLElement;
  let sacola: SacolaService;
  let router: Router;

  beforeEach(async () => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideRouter([{ path: '**', component: TelaVaziaComponent }])],
    });
    sacola = TestBed.inject(SacolaService);
    router = TestBed.inject(Router);
    await router.navigateByUrl('/cardapio');
    fixture = TestBed.createComponent(SacolaComponent);
    fixture.detectChanges();
    elemento = fixture.nativeElement;
  });

  it('não mostra painel nem barra com a sacola vazia e fechada', () => {
    expect(elemento.querySelector('.painel')).toBeNull();
    expect(elemento.querySelector('.barra')).toBeNull();
  });

  it('mostra o estado vazio ao abrir sem itens e trava a rolagem da página', () => {
    sacola.abrir();
    fixture.detectChanges();

    expect(elemento.querySelector('.vazia')).not.toBeNull();
    expect(document.body.classList.contains('sem-rolagem')).toBe(true);

    elemento.querySelector<HTMLButtonElement>('.painel__fechar')!.click();
    fixture.detectChanges();
    expect(document.body.classList.contains('sem-rolagem')).toBe(false);
  });

  it('mostra a barra flutuante com total e a esconde no checkout', async () => {
    sacola.adicionar(criarProduto(), 2);
    fixture.detectChanges();
    expect(elemento.querySelector('.barra__contador')?.textContent).toBe('1');

    await router.navigateByUrl('/finalizar-pedido');
    fixture.detectChanges();
    expect(elemento.querySelector('.barra')).toBeNull();
  });

  it('lista os itens, altera quantidade, remove e fecha com Esc', () => {
    sacola.adicionar(criarProduto(), 2);
    sacola.adicionar(BRIGADEIRO);
    sacola.abrir();
    fixture.detectChanges();

    expect(elemento.querySelectorAll('.item').length).toBe(2);
    expect(elemento.querySelector('.painel__resumo')?.textContent).toContain('2 itens');

    elemento.querySelectorAll<HTMLButtonElement>('.item eirm-seletor-quantidade button')[1].click();
    expect(sacola.quantidadeDe(1)).toBe(3);

    elemento.querySelectorAll<HTMLButtonElement>('.item__remover')[1].click();
    fixture.detectChanges();
    expect(elemento.querySelector('.painel__resumo')?.textContent).toContain('1 item');

    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }));
    expect(sacola.aberta()).toBe(false);
  });

  it('fecha e navega para o checkout ao finalizar', () => {
    const navegar = jest.spyOn(router, 'navigate').mockResolvedValue(true);
    sacola.adicionar(criarProduto());
    sacola.abrir();
    fixture.detectChanges();

    elemento.querySelector<HTMLButtonElement>('.painel__rodape .botao')!.click();

    expect(sacola.aberta()).toBe(false);
    expect(navegar).toHaveBeenCalledWith(['/', 'finalizar-pedido']);
  });
});
