import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { PedidoCriado } from '@models/pedido.model';
import { PedidoService } from '@services/pedido.service';
import { criarPedido } from '@testing/dados-de-teste';
import { PedidoConfirmadoComponent } from './pedido-confirmado.component';

describe('PedidoConfirmadoComponent', () => {
  function renderizar(pedido: PedidoCriado | null): HTMLElement {
    TestBed.configureTestingModule({
      providers: [provideRouter([]), { provide: PedidoService, useValue: { ultimoPedido: signal(pedido) } }],
    });
    const fixture = TestBed.createComponent(PedidoConfirmadoComponent);
    fixture.detectChanges();
    return fixture.nativeElement;
  }

  it('mostra código, link do WhatsApp, data sem fuso e itens', () => {
    const elemento = renderizar(criarPedido({ tipoEntrega: 'Entrega' }));

    expect(elemento.querySelector('h1')?.textContent).toContain('Obrigada, Maria!');
    expect(elemento.querySelector('.cartao__codigo')?.textContent).toBe('EI-ABC234');
    expect(elemento.querySelector('.cartao__whats')?.getAttribute('href')).toBe('https://wa.me/5521990225050?text=ola');
    expect(elemento.querySelector('.cartao__detalhes')?.textContent).toContain('20/11/2026');
    expect(elemento.querySelector('.cartao__detalhes')?.textContent).toContain('Entrega');
    expect(elemento.querySelectorAll('.cartao__itens li').length).toBe(1);
    expect(elemento.querySelectorAll('.confete').length).toBe(24);
    expect(elemento.querySelector('.acompanhar')?.getAttribute('href')).toBe('/acompanhar-pedido/EI-ABC234');
  });

  it('orienta quando não há pedido recente', () => {
    const elemento = renderizar(null);

    expect(elemento.querySelector('h1')?.textContent).toContain('Nenhum pedido');
    expect(elemento.querySelector('.sem-pedido a')?.getAttribute('href')).toBe('/cardapio');
  });
});
