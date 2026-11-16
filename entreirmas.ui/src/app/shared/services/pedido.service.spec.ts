import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '@environments/environment';
import { SEM_AVISO_DE_ERRO } from '@interceptors/erro.interceptor';
import { NovoPedido } from '@models/pedido.model';
import { criarAcompanhamento, criarPedido } from '@testing/dados-de-teste';
import {
  CHAVE_PEDIDOS_RECENTES,
  CHAVE_ULTIMO_PEDIDO,
  MAXIMO_PEDIDOS_RECENTES,
  PedidoService,
  normalizarCodigoPedido,
} from './pedido.service';

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

  beforeEach(() => {
    sessionStorage.clear();
    localStorage.clear();
  });

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

  it('consulta o acompanhamento sem o aviso geral de erro e lembra o código', () => {
    const servico = criarServico();
    const recebido = jest.fn();

    servico.acompanhar('EI-ABC234').subscribe(recebido);
    const requisicao = http.expectOne(`${environment.apiUrl}/pedidos/EI-ABC234`);
    expect(requisicao.request.context.get(SEM_AVISO_DE_ERRO)).toBe(true);
    requisicao.flush(criarAcompanhamento());

    expect(recebido).toHaveBeenCalledWith(expect.objectContaining({ status: 'Recebido' }));
    expect(servico.pedidosRecentes()).toEqual(['EI-ABC234']);
  });

  it('lembra os pedidos criados, do mais novo para o mais antigo, sem repetir e até o limite', () => {
    const servico = criarServico();
    const codigos = ['EI-AAAAA2', 'EI-AAAAA3', 'EI-AAAAA4', 'EI-AAAAA5', 'EI-AAAAA6', 'EI-AAAAA7', 'EI-AAAAA3'];

    for (const codigo of codigos) {
      servico.criar(NOVO_PEDIDO).subscribe();
      http.expectOne(`${environment.apiUrl}/pedidos`).flush(criarPedido({ codigo }));
    }

    expect(servico.pedidosRecentes()).toEqual(['EI-AAAAA3', 'EI-AAAAA7', 'EI-AAAAA6', 'EI-AAAAA5', 'EI-AAAAA4']);
    expect(servico.pedidosRecentes().length).toBe(MAXIMO_PEDIDOS_RECENTES);
    expect(JSON.parse(localStorage.getItem(CHAVE_PEDIDOS_RECENTES) ?? '[]')).toEqual(servico.pedidosRecentes());
  });

  it('recupera os pedidos do aparelho e ignora valores inválidos', () => {
    localStorage.setItem(CHAVE_PEDIDOS_RECENTES, JSON.stringify(['EI-ABC234', 42]));
    expect(criarServico().pedidosRecentes()).toEqual(['EI-ABC234']);

    TestBed.resetTestingModule();
    localStorage.setItem(CHAVE_PEDIDOS_RECENTES, '{quebrado');
    expect(criarServico().pedidosRecentes()).toEqual([]);

    TestBed.resetTestingModule();
    localStorage.setItem(CHAVE_PEDIDOS_RECENTES, '"texto"');
    expect(criarServico().pedidosRecentes()).toEqual([]);
  });
});

describe('normalizarCodigoPedido', () => {
  it.each([
    ['EI-ABC234', 'EI-ABC234'],
    ['ei-abc234', 'EI-ABC234'],
    [' EI ABC 234 ', 'EI-ABC234'],
    ['abc234', 'EI-ABC234'],
    ['EIABC234', 'EI-ABC234'],
    ['EI-ABC23', null],
    ['EI-ABC10O', null],
    ['', null],
    [null, null],
  ])('"%s" vira %s', (entrada, esperado) => {
    expect(normalizarCodigoPedido(entrada)).toBe(esperado);
  });
});
