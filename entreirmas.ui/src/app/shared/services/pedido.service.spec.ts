import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '@environments/environment';
import { NovoPedido } from '@models/pedido.model';
import { criarPedido } from '@testing/dados-de-teste';
import { CHAVE_ULTIMO_PEDIDO, PedidoService } from './pedido.service';

const NOVO_PEDIDO: NovoPedido = {
  nomeCliente: 'Maria',
  telefone: '21999998888',
  tipoEntrega: 'Retirada',
  endereco: null,
  dataDesejada: '2026-11-20',
  observacao: null,
  itens: [{ produtoId: 1, quantidade: 2 }],
};

describe('PedidoService', () => {
  let http: HttpTestingController;

  function criarServico(): PedidoService {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    http = TestBed.inject(HttpTestingController);
    return TestBed.inject(PedidoService);
  }

  beforeEach(() => sessionStorage.clear());

  it('envia o pedido e guarda o resultado como último pedido', () => {
    const servico = criarServico();
    expect(servico.ultimoPedido()).toBeNull();

    servico.criar(NOVO_PEDIDO).subscribe();
    const requisicao = http.expectOne(`${environment.apiUrl}/pedidos`);
    expect(requisicao.request.body).toEqual(NOVO_PEDIDO);
    requisicao.flush(criarPedido());

    expect(servico.ultimoPedido()?.codigo).toBe('EI-ABC234');
    expect(sessionStorage.getItem(CHAVE_ULTIMO_PEDIDO)).toContain('EI-ABC234');
  });

  it('recupera o último pedido do sessionStorage e ignora valores inválidos', () => {
    sessionStorage.setItem(CHAVE_ULTIMO_PEDIDO, JSON.stringify(criarPedido({ codigo: 'EI-ZZZ999' })));
    expect(criarServico().ultimoPedido()?.codigo).toBe('EI-ZZZ999');

    TestBed.resetTestingModule();
    sessionStorage.setItem(CHAVE_ULTIMO_PEDIDO, '{quebrado');
    expect(criarServico().ultimoPedido()).toBeNull();
  });

  it('mantém o pedido em memória quando o sessionStorage falha', () => {
    const servico = criarServico();
    jest.spyOn(Storage.prototype, 'setItem').mockImplementation(() => {
      throw new Error('indisponível');
    });

    servico.criar(NOVO_PEDIDO).subscribe();
    http.expectOne(`${environment.apiUrl}/pedidos`).flush(criarPedido());

    expect(servico.ultimoPedido()?.codigo).toBe('EI-ABC234');
  });
});
