import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed, discardPeriodicTasks, fakeAsync, tick } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { environment } from '@environments/environment';
import { SEM_AVISO_DE_ERRO } from '@interceptors/erro.interceptor';
import { PedidoDetalhe } from '@models/admin.model';
import { criarPedidoDetalhe } from '@testing/dados-admin';
import { Subscription, of } from 'rxjs';
import { AlertaPedidosService, CHAVE_SOM_PEDIDOS, INTERVALO_ALERTA_MS } from './alerta-pedidos.service';
import { NotificacaoService } from './notificacao.service';

const URL_ULTIMO = `${environment.apiUrl}/admin/pedidos/ultimo-do-site`;
const URL_NOVOS = `${environment.apiUrl}/admin/pedidos/novos-do-site`;

/** AudioContext falso: o jsdom não tem áudio. */
class AudioFalso {
  static criados = 0;
  state: AudioContextState = 'suspended';
  currentTime = 0;
  destination = {};
  osciladores = 0;
  static ultimo: AudioFalso | null = null;
  constructor() {
    AudioFalso.criados++;
    AudioFalso.ultimo = this;
  }
  resume = jest.fn(() => {
    this.state = 'running';
    return Promise.resolve();
  });
  createOscillator = jest.fn(() => {
    this.osciladores++;
    return { type: '', frequency: { value: 0 }, connect: (no: unknown) => no, start: jest.fn(), stop: jest.fn() };
  });
  createGain = jest.fn(() => ({
    gain: { setValueAtTime: jest.fn(), exponentialRampToValueAtTime: jest.fn() },
    connect: (no: unknown) => no,
  }));
  createDynamicsCompressor = jest.fn(() => ({
    threshold: { value: 0 },
    ratio: { value: 0 },
    connect: (no: unknown) => no,
  }));
}

describe('AlertaPedidosService', () => {
  const notificacao = { sucesso: jest.fn(() => of(undefined)) };
  let servico: AlertaPedidosService;
  let http: HttpTestingController;
  let inscricao: Subscription | undefined;
  let navegar: jest.SpyInstance;

  function criar(): void {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: NotificacaoService, useValue: notificacao },
      ],
    });
    servico = TestBed.inject(AlertaPedidosService);
    http = TestBed.inject(HttpTestingController);
    navegar = jest.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
  }

  /** Consulta dos pedidos depois do id informado. */
  function novos(depoisDe: number) {
    return http.expectOne((requisicao) => requisicao.url === URL_NOVOS && requisicao.params.get('depoisDe') === String(depoisDe));
  }

  beforeEach(() => {
    localStorage.clear();
    AudioFalso.criados = 0;
    (globalThis as { AudioContext?: unknown }).AudioContext = AudioFalso;
  });

  afterEach(() => {
    inscricao?.unsubscribe();
    delete (globalThis as { AudioContext?: unknown }).AudioContext;
  });

  it('a primeira consulta só marca o ponto de partida; depois traz os pedidos novos, avisa, toca e emite cada um', fakeAsync(() => {
    criar();
    const tocar = jest.spyOn(servico, 'tocar');
    const emitidos: PedidoDetalhe[] = [];
    inscricao = servico.observar().subscribe((pedido) => emitidos.push(pedido));

    tick(0);
    const primeira = http.expectOne(URL_ULTIMO);
    expect(primeira.request.context.get(SEM_AVISO_DE_ERRO)).toBe(true);
    primeira.flush({ id: 10, codigo: 'EI-AAAAA2', nomeCliente: 'Ana' });
    expect(notificacao.sucesso).not.toHaveBeenCalled();

    tick(INTERVALO_ALERTA_MS);
    const segunda = novos(10);
    expect(segunda.request.context.get(SEM_AVISO_DE_ERRO)).toBe(true);
    segunda.flush([]);
    expect(servico.novosPedidos()).toBe(0);

    tick(INTERVALO_ALERTA_MS);
    novos(10).flush([criarPedidoDetalhe({ id: 11, codigo: 'EI-BBBBB3', nomeCliente: 'Bia' })]);

    expect(servico.novosPedidos()).toBe(1);
    expect(tocar).toHaveBeenCalledTimes(1);
    expect(emitidos.map((pedido) => pedido.codigo)).toEqual(['EI-BBBBB3']);
    expect(notificacao.sucesso).toHaveBeenCalledWith('Pedido novo: Bia (EI-BBBBB3)', 'Ver');
    expect(navegar).toHaveBeenCalledWith(['/', 'admin', 'pedidos']);

    discardPeriodicTasks();
  }));

  it('vários pedidos juntos: um som e uma mensagem, e emite todos em ordem', fakeAsync(() => {
    criar();
    const tocar = jest.spyOn(servico, 'tocar');
    const emitidos: PedidoDetalhe[] = [];
    inscricao = servico.observar().subscribe((pedido) => emitidos.push(pedido));

    tick(0);
    http.expectOne(URL_ULTIMO).flush({ id: 3, codigo: 'EI-AAAAA2', nomeCliente: 'Ana' });
    tick(INTERVALO_ALERTA_MS);
    novos(3).flush([criarPedidoDetalhe({ id: 4, codigo: 'EI-CCCCC4' }), criarPedidoDetalhe({ id: 6, codigo: 'EI-CCCCC6' })]);

    expect(emitidos.map((pedido) => pedido.id)).toEqual([4, 6]);
    expect(servico.novosPedidos()).toBe(2);
    expect(tocar).toHaveBeenCalledTimes(1);
    expect(notificacao.sucesso).toHaveBeenCalledWith('2 pedidos novos', 'Ver');

    discardPeriodicTasks();
  }));

  it('sem pedidos ainda (204), parte do zero; falhas são ignoradas e a próxima consulta tenta de novo', fakeAsync(() => {
    criar();
    inscricao = servico.observar().subscribe();

    tick(0);
    http.expectOne(URL_ULTIMO).flush(null, { status: 204, statusText: 'No Content' });

    tick(INTERVALO_ALERTA_MS);
    novos(0).flush(null, { status: 503, statusText: 'Indisponível' });
    expect(servico.novosPedidos()).toBe(0);

    tick(INTERVALO_ALERTA_MS);
    novos(0).flush([criarPedidoDetalhe({ id: 1 })]);
    expect(servico.novosPedidos()).toBe(1);
    discardPeriodicTasks();
  }));

  it('a primeira consulta que falha não vira ponto de partida', fakeAsync(() => {
    criar();
    inscricao = servico.observar().subscribe();

    tick(0);
    http.expectOne(URL_ULTIMO).flush(null, { status: 0, statusText: 'Sem rede' });
    tick(INTERVALO_ALERTA_MS);
    http.expectOne(URL_ULTIMO).flush({ id: 7, codigo: 'EI-AAAAA2', nomeCliente: 'Ana' });

    expect(servico.novosPedidos()).toBe(0);
    discardPeriodicTasks();
  }));

  it('toca as três notas duas vezes, alto e com compressor, libera o áudio e respeita o som desligado (lembrado no aparelho)', () => {
    criar();

    servico.prepararAudio();
    servico.tocar();
    expect(AudioFalso.criados).toBe(1);
    const audio = AudioFalso.ultimo!;
    expect(audio.osciladores).toBe(6);
    expect(audio.createDynamicsCompressor).toHaveBeenCalledTimes(1);
    const volume = audio.createGain.mock.results[0].value as {
      gain: { exponentialRampToValueAtTime: jest.Mock };
    };
    expect(volume.gain.exponentialRampToValueAtTime).toHaveBeenCalledWith(0.9, expect.any(Number));

    servico.alternarSom();
    expect(servico.somLigado()).toBe(false);
    expect(localStorage.getItem(CHAVE_SOM_PEDIDOS)).toBe('desligado');

    TestBed.resetTestingModule();
    criar();
    expect(servico.somLigado()).toBe(false);
    const tocar = jest.spyOn(servico, 'tocar');
    servico.alternarSom();
    expect(servico.somLigado()).toBe(true);
    expect(tocar).toHaveBeenCalled();
  });

  it('sem suporte a áudio não quebra', () => {
    delete (globalThis as { AudioContext?: unknown }).AudioContext;
    criar();

    expect(() => {
      servico.prepararAudio();
      servico.tocar();
    }).not.toThrow();
  });
});
