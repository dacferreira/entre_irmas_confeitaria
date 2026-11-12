import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { SacolaService } from '@services/sacola.service';
import { criarProduto } from '@testing/dados-de-teste';
import { CabecalhoComponent } from './cabecalho.component';

describe('CabecalhoComponent', () => {
  let fixture: ComponentFixture<CabecalhoComponent>;
  let elemento: HTMLElement;
  let sacola: SacolaService;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({ providers: [provideRouter([])] });
    sacola = TestBed.inject(SacolaService);
    fixture = TestBed.createComponent(CabecalhoComponent);
    fixture.detectChanges();
    elemento = fixture.nativeElement;
  });

  it('mostra o contador da sacola só quando há itens e abre a sacola', () => {
    expect(elemento.querySelector('.botao-sacola__contador')).toBeNull();

    sacola.adicionar(criarProduto());
    fixture.detectChanges();
    expect(elemento.querySelector('.botao-sacola__contador')?.textContent).toBe('1');

    elemento.querySelector<HTMLButtonElement>('.botao-sacola')!.click();
    expect(sacola.aberta()).toBe(true);
  });

  it('abre e fecha o menu do celular', () => {
    const botaoMenu = elemento.querySelector<HTMLButtonElement>('.botao-menu')!;

    botaoMenu.click();
    fixture.detectChanges();
    expect(botaoMenu.getAttribute('aria-expanded')).toBe('true');
    expect(elemento.querySelector('.menu--aberto')).not.toBeNull();

    elemento.querySelector<HTMLAnchorElement>('.menu__link')!.click();
    fixture.detectChanges();
    expect(botaoMenu.getAttribute('aria-expanded')).toBe('false');
  });

  it('marca o cabeçalho ao rolar a página', () => {
    Object.defineProperty(window, 'scrollY', { value: 120, configurable: true });
    window.dispatchEvent(new Event('scroll'));
    fixture.detectChanges();

    expect(elemento.querySelector('.cabecalho--rolado')).not.toBeNull();
    Object.defineProperty(window, 'scrollY', { value: 0, configurable: true });
  });
});
