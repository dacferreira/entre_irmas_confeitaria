import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Title } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { Produto } from '@models/produto.model';
import { CatalogoService } from '@services/catalogo.service';
import { NotificacaoService } from '@services/notificacao.service';
import { SacolaService } from '@services/sacola.service';
import { BICHO_DE_PE, BRIGADEIRO, criarProduto } from '@testing/dados-de-teste';
import { NEVER, Observable, Subject, of, throwError } from 'rxjs';
import { ProdutoComponent } from './produto.component';

describe('ProdutoComponent', () => {
  const acao$ = new Subject<void>();
  const notificacao = { sucesso: jest.fn(() => acao$.asObservable()) };
  let fixture: ComponentFixture<ProdutoComponent>;
  let sacola: SacolaService;

  function renderizar(produto$: Observable<Produto>, relacionados$: Observable<Produto[]> = of([BRIGADEIRO, BICHO_DE_PE])): HTMLElement {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        { provide: NotificacaoService, useValue: notificacao },
        { provide: CatalogoService, useValue: { obterProduto: () => produto$, listarProdutos: () => relacionados$ } },
      ],
    });
    sacola = TestBed.inject(SacolaService);
    fixture = TestBed.createComponent(ProdutoComponent);
    fixture.componentRef.setInput('slug', 'brigadeiro-gourmet');
    fixture.detectChanges();
    return fixture.nativeElement;
  }

  it('mostra o produto com mínimo, trilha, relacionados sem o próprio e título da aba', () => {
    const elemento = renderizar(of(BRIGADEIRO));

    expect(elemento.querySelector('h1')?.textContent).toContain('Brigadeiro Gourmet');
    expect(elemento.querySelector('.produto__minimo')?.textContent).toContain('25');
    expect(elemento.querySelector('.trilha a:last-child')?.getAttribute('href')).toBe('/cardapio?categoria=docinhos');
    expect(elemento.querySelectorAll('.relacionados eirm-card-produto').length).toBe(1);
    expect(TestBed.inject(Title).getTitle()).toBe('Brigadeiro Gourmet · Entre Irmãs Confeitaria');
    expect(fixture.componentInstance.quantidade()).toBe(25);
  });

  it('adiciona a quantidade escolhida e avisa quanto já tem na sacola', () => {
    const elemento = renderizar(of(BRIGADEIRO), of([]));

    fixture.componentInstance.quantidade.set(30);
    fixture.detectChanges();
    expect(elemento.querySelector('.compra__botao')?.textContent).toContain('75');

    elemento.querySelector<HTMLButtonElement>('.compra__botao')!.click();
    fixture.detectChanges();

    expect(sacola.quantidadeDe(BRIGADEIRO.id)).toBe(30);
    expect(elemento.querySelector('.produto__na-sacola')?.textContent).toContain('30');
    acao$.next();
    expect(sacola.aberta()).toBe(true);
  });

  it('usa a foto quando existir', () => {
    const elemento = renderizar(of(criarProduto({ urlImagem: 'https://exemplo.com/b.jpg' })), of([]));

    expect(elemento.querySelector('.produto__visual img')?.getAttribute('src')).toBe('https://exemplo.com/b.jpg');
  });

  it('mostra o carregamento e o aviso de produto não encontrado', () => {
    expect(renderizar(NEVER).querySelector('[aria-busy="true"]')).not.toBeNull();
    fixture.componentInstance.adicionar();
    expect(sacola.vazia()).toBe(true);

    TestBed.resetTestingModule();
    const elemento = renderizar(throwError(() => new Error('404')));
    expect(elemento.querySelector('.nao-achou')).not.toBeNull();
  });
});
