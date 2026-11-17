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
    expect(elemento.querySelector('.cartao__retirada')).toBeNull();
  });

  it('na retirada mostra onde retirar, com link para o mapa', () => {
    const elemento = renderizar(criarPedido({ tipoEntrega: 'Retirada' }));
    const retirada = elemento.querySelector('.cartao__retirada');

    expect(retirada?.textContent).toContain('Rua A, 41 - Paciência, Rio de Janeiro - RJ, 23580-250');
    expect(retirada?.querySelector('a')?.getAttribute('href')).toContain('google.com/maps/search');
  });

  it('na entrega mostra a taxa com o parceiro e o total a pagar', () => {
    const comTaxa = renderizar(
      criarPedido({ tipoEntrega: 'Entrega', taxaEntrega: 10.21, parceiroEntrega: 'Uber', totalAPagar: 28.21 }),
    );

    expect(comTaxa.querySelector('.cartao__entrega')?.textContent).toContain('Entrega (Uber)');
    expect(comTaxa.querySelector('.cartao__entrega')?.textContent).toContain('10.21');
    expect(comTaxa.querySelector('.cartao__total')?.textContent).toContain('28.21');
  });

  it('entrega sem taxa calculada avisa que é combinada pelo WhatsApp', () => {
    const elemento = renderizar(criarPedido({ tipoEntrega: 'Entrega' }));

    expect(elemento.querySelector('.cartao__entrega')?.textContent).toContain('combinada pelo WhatsApp');
  });

  it('orienta quando não há pedido recente', () => {
    const elemento = renderizar(null);

    expect(elemento.querySelector('h1')?.textContent).toContain('Nenhum pedido');
    expect(elemento.querySelector('.sem-pedido a')?.getAttribute('href')).toBe('/cardapio');
  });
});
