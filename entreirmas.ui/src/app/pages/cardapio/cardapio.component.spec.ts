import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { CatalogoService } from '@services/catalogo.service';
import { NotificacaoService } from '@services/notificacao.service';
import { CATEGORIAS, PRODUTOS, criarProduto } from '@testing/dados-de-teste';
import { NEVER, Observable, of, throwError } from 'rxjs';
import { CardapioComponent, normalizarTexto } from './cardapio.component';

describe('CardapioComponent', () => {
  let fixture: ComponentFixture<CardapioComponent>;

  function renderizar(produtos$: Observable<unknown> = of(PRODUTOS), categorias$: Observable<unknown> = of(CATEGORIAS)): HTMLElement {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        { provide: CatalogoService, useValue: { listarProdutos: () => produtos$, listarCategorias: () => categorias$ } },
        { provide: NotificacaoService, useValue: { sucesso: () => of() } },
      ],
    });
    fixture = TestBed.createComponent(CardapioComponent);
    fixture.detectChanges();
    return fixture.nativeElement;
  }

  function buscar(elemento: HTMLElement, termo: string): void {
    const campo = elemento.querySelector<HTMLInputElement>('input[type="search"]')!;
    campo.value = termo;
    campo.dispatchEvent(new Event('input'));
    fixture.detectChanges();
  }

  it('normaliza acentos e caixa', () => {
    expect(normalizarTexto('  Bicho de PÉ ')).toBe('bicho de pe');
  });

  it('agrupa os produtos por categoria com os chips de filtro', () => {
    const elemento = renderizar();

    expect(Array.from(elemento.querySelectorAll('.grupo__titulo')).map((t) => t.textContent)).toEqual([
      'Brownies & Fudges',
      'Docinhos',
    ]);
    expect(elemento.querySelectorAll('.categorias__chip').length).toBe(3);
    expect(elemento.querySelector('.resultado')?.textContent).toContain('3 opções');
  });

  it('sem doces sob encomenda, não mostra os títulos dos blocos', () => {
    const elemento = renderizar();

    expect(elemento.querySelector('.bloco__titulo')).toBeNull();
  });

  it('separa pronta entrega e deixa os doces sob encomenda por último', () => {
    const bolo = criarProduto({ id: 7, nome: 'Bolo de Festa', slug: 'bolo-de-festa', encomenda: true, categoriaSlug: 'docinhos', categoriaNome: 'Docinhos' });
    const elemento = renderizar(of([bolo, ...PRODUTOS]));

    expect(Array.from(elemento.querySelectorAll('.bloco__titulo')).map((t) => t.textContent)).toEqual([
      'Pronta entrega',
      'Sob encomenda',
    ]);
    const ultimoBloco = elemento.querySelectorAll('.bloco')[1];
    expect(ultimoBloco.classList).toContain('bloco--encomenda');
    expect(ultimoBloco.querySelectorAll('eirm-card-produto').length).toBe(1);
    expect(ultimoBloco.textContent).toContain('antecedência');
    expect(elemento.querySelector('.resultado')?.textContent).toContain('4 opções');

    // Só encomenda (filtro de busca): o título do bloco continua, para avisar da antecedência.
    buscar(elemento, 'festa');
    expect(Array.from(elemento.querySelectorAll('.bloco__titulo')).map((t) => t.textContent)).toEqual(['Sob encomenda']);
  });

  it('filtra pela categoria da URL', () => {
    renderizar();
    fixture.componentRef.setInput('categoria', 'docinhos');
    fixture.detectChanges();
    const elemento: HTMLElement = fixture.nativeElement;

    expect(elemento.querySelectorAll('eirm-card-produto').length).toBe(2);
    expect(elemento.querySelector('.categorias__chip--ativo')?.textContent?.trim()).toBe('Docinhos');
  });

  it('busca sem acento e mostra o estado vazio com opção de limpar', () => {
    const elemento = renderizar();
    const navegar = jest.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);

    buscar(elemento, 'pe');
    expect(elemento.querySelectorAll('eirm-card-produto').length).toBe(1);
    expect(elemento.querySelector('.resultado')?.textContent).toContain('1 opção');

    buscar(elemento, 'pudim');
    expect(elemento.querySelector('.vazio__titulo')?.textContent).toContain('Nenhum doce');

    elemento.querySelector<HTMLButtonElement>('.vazio .botao')!.click();
    fixture.detectChanges();
    expect(fixture.componentInstance.busca()).toBe('');
    expect(navegar).toHaveBeenCalledWith([], expect.objectContaining({ queryParams: { categoria: null } }));
  });

  it('troca a categoria pela URL ao clicar no chip', () => {
    const elemento = renderizar();
    const navegar = jest.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);

    elemento.querySelectorAll<HTMLButtonElement>('.categorias__chip')[2].click();

    expect(navegar).toHaveBeenCalledWith([], expect.objectContaining({ queryParams: { categoria: 'docinhos' } }));
  });

  it('mostra esqueletos ao carregar e aviso quando falha', () => {
    expect(renderizar(NEVER).querySelectorAll('.grade__esqueleto').length).toBe(6);

    TestBed.resetTestingModule();
    const comErro = renderizar(throwError(() => new Error('x')), throwError(() => new Error('x')));
    expect(comErro.querySelector('.vazio__titulo')?.textContent).toContain('não carregou');
  });
});
