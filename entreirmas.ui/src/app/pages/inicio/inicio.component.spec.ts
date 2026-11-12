import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { CatalogoService } from '@services/catalogo.service';
import { NotificacaoService } from '@services/notificacao.service';
import { CATEGORIAS, PRODUTOS } from '@testing/dados-de-teste';
import { NEVER, Observable, of, throwError } from 'rxjs';
import { InicioComponent } from './inicio.component';

describe('InicioComponent', () => {
  function renderizar(produtos$: Observable<unknown>, categorias$: Observable<unknown> = of(CATEGORIAS)): HTMLElement {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        { provide: CatalogoService, useValue: { listarProdutos: () => produtos$, listarCategorias: () => categorias$ } },
        { provide: NotificacaoService, useValue: { sucesso: () => of() } },
      ],
    });
    const fixture = TestBed.createComponent(InicioComponent);
    fixture.detectChanges();
    return fixture.nativeElement;
  }

  it('mostra destaques, vitrine de categorias com ilustração e a faixa com nomes', () => {
    const elemento = renderizar(of(PRODUTOS));

    expect(elemento.querySelectorAll('eirm-card-produto').length).toBe(2);
    const vitrine = Array.from(elemento.querySelectorAll('.vitrine__card'));
    expect(vitrine.map((v) => v.getAttribute('data-sabor'))).toEqual(['brownie', 'brigadeiro']);
    expect(vitrine[1].getAttribute('href')).toBe('/cardapio?categoria=docinhos');
    expect(vitrine[1].textContent).toContain('2 opções');
    expect(elemento.querySelectorAll('.faixa__item').length).toBe(PRODUTOS.length * 2);
  });

  it('mostra esqueletos enquanto carrega e a faixa padrão', () => {
    const elemento = renderizar(NEVER, of([]));

    expect(elemento.querySelectorAll('.grade__esqueleto').length).toBe(3);
    expect(elemento.querySelector('.vitrine')).toBeNull();
    expect(elemento.querySelector('.faixa__item')?.textContent).toBe('Bolos recheados');
  });

  it('oferece o WhatsApp quando o cardápio falha', () => {
    const elemento = renderizar(throwError(() => new Error('fora do ar')), throwError(() => new Error('x')));

    expect(elemento.querySelector('.aviso .botao--whatsapp')).not.toBeNull();
  });
});
