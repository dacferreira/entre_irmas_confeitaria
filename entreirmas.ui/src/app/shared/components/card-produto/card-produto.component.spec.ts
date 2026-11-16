import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { NotificacaoService } from '@services/notificacao.service';
import { SacolaService } from '@services/sacola.service';
import { BRIGADEIRO, criarProduto } from '@testing/dados-de-teste';
import { Subject } from 'rxjs';
import { CardProdutoComponent } from './card-produto.component';

describe('CardProdutoComponent', () => {
  const acao$ = new Subject<void>();
  const notificacao = { sucesso: jest.fn(() => acao$.asObservable()) };
  let fixture: ComponentFixture<CardProdutoComponent>;
  let sacola: SacolaService;

  function renderizar(produto = criarProduto()): HTMLElement {
    fixture = TestBed.createComponent(CardProdutoComponent);
    fixture.componentRef.setInput('produto', produto);
    fixture.detectChanges();
    return fixture.nativeElement;
  }

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideRouter([]), { provide: NotificacaoService, useValue: notificacao }],
    });
    sacola = TestBed.inject(SacolaService);
  });

  it('marca os doces sob encomenda', () => {
    expect(renderizar().querySelector('.card__encomenda')).toBeNull();
    expect(renderizar(criarProduto({ encomenda: true })).querySelector('.card__encomenda')?.textContent).toContain('sob encomenda');
  });

  it('mostra nome, categoria, link e selo de destaque', () => {
    const elemento = renderizar();

    expect(elemento.querySelector('.card__nome a')?.getAttribute('href')).toBe('/produto/brownie-tradicional');
    expect(elemento.textContent).toContain('Brownies & Fudges');
    expect(elemento.querySelector('.card__selo')).not.toBeNull();
    expect(elemento.querySelector('.card__minimo')).toBeNull();
    expect(elemento.querySelector('[data-sabor="brownie"]')).not.toBeNull();
  });

  it('mostra o pedido mínimo e a foto quando houver', () => {
    const elemento = renderizar({ ...BRIGADEIRO, urlImagem: 'https://exemplo.com/brigadeiro.jpg' });

    expect(elemento.querySelector('.card__minimo')?.textContent).toContain('25');
    expect(elemento.querySelector('img')?.getAttribute('src')).toBe('https://exemplo.com/brigadeiro.jpg');
  });

  it('adiciona à sacola, mostra o contador e abre a sacola pela ação da notificação', () => {
    const elemento = renderizar(BRIGADEIRO);

    elemento.querySelector<HTMLButtonElement>('.card__adicionar')!.click();
    fixture.detectChanges();

    expect(sacola.quantidadeDe(BRIGADEIRO.id)).toBe(25);
    expect(elemento.querySelector('.card__contador')?.textContent).toBe('25');
    expect(notificacao.sucesso).toHaveBeenCalledWith('Brigadeiro Gourmet foi para a sacola!', 'Ver sacola');

    acao$.next();
    expect(sacola.aberta()).toBe(true);
  });
});
