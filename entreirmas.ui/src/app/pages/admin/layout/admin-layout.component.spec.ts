import { Component, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { AlertaPedidosService } from '@services/alerta-pedidos.service';
import { AuthService } from '@services/auth.service';
import { ImpressoraService } from '@services/impressora.service';
import { EMPTY, of } from 'rxjs';
import { AdminLayoutComponent } from './admin-layout.component';

@Component({ template: '' })
class TelaVaziaComponent {}

describe('AdminLayoutComponent', () => {
  const auth = { nome: signal('Suellen Alves'), login: signal('suellen'), sair: jest.fn() };
  const somLigado = signal(true);
  const alerta = {
    somLigado,
    observar: jest.fn(() => EMPTY),
    alternarSom: jest.fn(() => somLigado.update((ligado) => !ligado)),
    prepararAudio: jest.fn(),
  };
  const impressora = { reconectar: jest.fn(() => Promise.resolve()), imprimirPedidoNovo: jest.fn(() => Promise.resolve()) };
  let fixture: ComponentFixture<AdminLayoutComponent>;
  let elemento: HTMLElement;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([{ path: '**', component: TelaVaziaComponent }]),
        { provide: AuthService, useValue: auth },
        { provide: AlertaPedidosService, useValue: alerta },
        { provide: ImpressoraService, useValue: impressora },
      ],
    });
    fixture = TestBed.createComponent(AdminLayoutComponent);
    fixture.detectChanges();
    elemento = fixture.nativeElement;
  });

  it('mostra o menu dos cadastros e quem está logado', () => {
    const links = Array.from(elemento.querySelectorAll('.lateral__menu a')).map((a) => a.textContent?.trim());

    expect(links).toEqual(['Painel', 'Pedidos', 'Produtos', 'Categorias', 'Despesas e perdas', 'Loja', 'Usuários']);
    expect(elemento.querySelector('.usuario__nome')?.textContent).toContain('Suellen Alves');
    expect(fixture.componentInstance.iniciais()).toBe('SA');
  });

  it('abre a gaveta no celular e fecha com Esc e ao navegar', async () => {
    const botao = elemento.querySelector<HTMLButtonElement>('.barra-movel button')!;

    botao.click();
    fixture.detectChanges();
    expect(botao.getAttribute('aria-expanded')).toBe('true');
    expect(elemento.querySelector('.fundo')).not.toBeNull();

    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }));
    fixture.detectChanges();
    expect(fixture.componentInstance.menuAberto()).toBe(false);

    fixture.componentInstance.alternarMenu();
    await TestBed.inject(Router).navigateByUrl('/admin/pedidos');
    expect(fixture.componentInstance.menuAberto()).toBe(false);
  });

  it('sai pelo botão do rodapé do menu', () => {
    elemento.querySelector<HTMLButtonElement>('.usuario button')!.click();

    expect(auth.sair).toHaveBeenCalled();
  });

  it('observa os pedidos novos, libera o áudio no primeiro clique e liga/desliga o som', () => {
    expect(alerta.observar).toHaveBeenCalled();

    document.dispatchEvent(new Event('pointerdown'));
    expect(alerta.prepararAudio).toHaveBeenCalled();

    const botao = elemento.querySelector<HTMLButtonElement>('.lateral__som')!;
    expect(botao.textContent).toContain('ligado');
    expect(botao.getAttribute('aria-pressed')).toBe('true');

    botao.click();
    fixture.detectChanges();

    expect(alerta.alternarSom).toHaveBeenCalled();
    expect(botao.textContent).toContain('desligado');
    expect(botao.getAttribute('aria-pressed')).toBe('false');
  });

  it('reconecta a impressora e manda cada pedido novo para o recibo', () => {
    expect(impressora.reconectar).toHaveBeenCalled();
    const pedido = { id: 5, codigo: 'EI-AAAAA2' };
    alerta.observar.mockReturnValueOnce(of(pedido) as never);

    TestBed.createComponent(AdminLayoutComponent);

    expect(impressora.imprimirPedidoNovo).toHaveBeenCalledWith(pedido);
  });
});
