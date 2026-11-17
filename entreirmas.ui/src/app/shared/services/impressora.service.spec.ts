import { TestBed } from '@angular/core/testing';
import { environment } from '@environments/environment';
import { criarNotificacaoFalsa, criarPedidoDetalhe } from '@testing/dados-admin';
import { CHAVE_IMPRESSAO_AUTOMATICA, ImpressoraService } from './impressora.service';
import { NotificacaoService } from './notificacao.service';

/** Porta serial falsa: guarda o que foi escrito e pode falhar nas próximas escritas. */
class PortaFalsa {
  writable: { getWriter: () => { write: jest.Mock; releaseLock: jest.Mock } } | null = null;
  escritos: Uint8Array[] = [];
  falhasNaEscrita = 0;
  open = jest.fn(async () => {
    this.writable = {
      getWriter: () => ({
        write: jest.fn(async (bytes: Uint8Array) => {
          if (this.falhasNaEscrita > 0) {
            this.falhasNaEscrita--;
            throw new Error('Conexão perdida');
          }
          this.escritos.push(bytes);
        }),
        releaseLock: jest.fn(),
      }),
    };
  });
  close = jest.fn(async () => {
    this.writable = null;
  });
  forget = jest.fn(async () => undefined);
}

describe('ImpressoraService', () => {
  let notificacao: ReturnType<typeof criarNotificacaoFalsa>;
  let serial: { getPorts: jest.Mock; requestPort: jest.Mock };
  let servico: ImpressoraService;

  function criar(comSerial = true, habilitada = true): void {
    environment.impressoraHabilitada = habilitada;
    notificacao = criarNotificacaoFalsa();
    if (comSerial) {
      Object.defineProperty(navigator, 'serial', { value: serial, configurable: true });
    }
    TestBed.configureTestingModule({ providers: [{ provide: NotificacaoService, useValue: notificacao }] });
    servico = TestBed.inject(ImpressoraService);
  }

  beforeEach(() => {
    localStorage.clear();
    serial = { getPorts: jest.fn(async () => []), requestPort: jest.fn() };
  });

  afterEach(() => {
    delete (navigator as { serial?: unknown }).serial;
    environment.impressoraHabilitada = false;
  });

  it('desligada no environment, não reconecta, não abre a lista de portas e não imprime', async () => {
    const porta = new PortaFalsa();
    serial.getPorts.mockResolvedValue([porta]);
    criar(true, false);

    expect(servico.habilitada).toBe(false);
    expect(servico.suportada).toBe(true);
    await servico.reconectar();
    expect(await servico.conectar()).toBe(false);
    await servico.imprimirPedidoNovo(criarPedidoDetalhe());
    await expect(servico.imprimirTeste()).rejects.toThrow('Impressora desligada neste ambiente.');

    expect(servico.conectada()).toBe(false);
    expect(serial.getPorts).not.toHaveBeenCalled();
    expect(serial.requestPort).not.toHaveBeenCalled();
    expect(porta.open).not.toHaveBeenCalled();
    expect(notificacao.erro).not.toHaveBeenCalled();
  });

  it('sem Web Serial (Firefox, Safari, celular) não é suportada e não quebra', async () => {
    criar(false);

    expect(servico.suportada).toBe(false);
    await servico.reconectar();
    expect(await servico.conectar()).toBe(false);
    await expect(servico.imprimirTeste()).rejects.toThrow('Nenhuma impressora conectada.');
  });

  it('reconecta sozinha à porta já autorizada neste navegador', async () => {
    const porta = new PortaFalsa();
    serial.getPorts.mockResolvedValue([porta]);
    criar();

    expect(servico.conectada()).toBe(false);
    await servico.reconectar();

    expect(servico.conectada()).toBe(true);
  });

  it('conecta pela lista do navegador (Bluetooth SPP) e imprime o recibo do pedido', async () => {
    const porta = new PortaFalsa();
    serial.requestPort.mockResolvedValue(porta);
    criar();

    expect(await servico.conectar()).toBe(true);
    expect(serial.requestPort).toHaveBeenCalledWith({
      allowedBluetoothServiceClassIds: ['00001101-0000-1000-8000-00805f9b34fb'],
    });

    await servico.imprimir(criarPedidoDetalhe());
    await servico.imprimirTeste();

    expect(porta.open).toHaveBeenCalledTimes(1);
    expect(porta.open).toHaveBeenCalledWith({ baudRate: 9600 });
    expect(porta.escritos).toHaveLength(2);
    expect(String.fromCharCode(...porta.escritos[0])).toContain('EI-ABC234');
    expect(servico.imprimindo()).toBe(false);
  });

  it('ao trocar de impressora, esquece a porta escolhida por engano e passa a imprimir na nova', async () => {
    const errada = new PortaFalsa();
    const certa = new PortaFalsa();
    serial.getPorts.mockResolvedValue([errada]);
    criar();
    await servico.reconectar();
    await servico.imprimirTeste();
    expect(errada.escritos).toHaveLength(1);

    serial.requestPort.mockResolvedValue(certa);
    serial.getPorts.mockResolvedValue([errada, certa]);
    expect(await servico.conectar()).toBe(true);

    expect(errada.close).toHaveBeenCalled();
    expect(errada.forget).toHaveBeenCalled();
    expect(certa.forget).not.toHaveBeenCalled();
    await servico.imprimirTeste();
    expect(certa.escritos).toHaveLength(1);
    expect(errada.escritos).toHaveLength(1);
  });

  it('escolher de novo a mesma porta não a esquece', async () => {
    const porta = new PortaFalsa();
    serial.getPorts.mockResolvedValue([porta]);
    serial.requestPort.mockResolvedValue(porta);
    criar();
    await servico.reconectar();

    expect(await servico.conectar()).toBe(true);

    expect(porta.forget).not.toHaveBeenCalled();
    expect(servico.conectada()).toBe(true);
  });

  it('se a pessoa fecha a lista sem escolher, continua desconectada', async () => {
    serial.requestPort.mockRejectedValue(new DOMException('Nenhuma porta', 'NotFoundError'));
    criar();

    expect(await servico.conectar()).toBe(false);
    expect(servico.conectada()).toBe(false);
  });

  it('se a conexão caiu, reabre a porta e tenta de novo uma vez', async () => {
    const porta = new PortaFalsa();
    serial.getPorts.mockResolvedValue([porta]);
    criar();
    await servico.reconectar();
    await servico.imprimirTeste();

    porta.falhasNaEscrita = 1;
    await servico.imprimirTeste();
    expect(porta.close).toHaveBeenCalledTimes(1);
    expect(porta.open).toHaveBeenCalledTimes(2);
    expect(porta.escritos).toHaveLength(2);

    porta.falhasNaEscrita = 2;
    await expect(servico.imprimirTeste()).rejects.toThrow('Conexão perdida');
    // A fila segue: o próximo recibo sai normalmente.
    await servico.imprimirTeste();
    expect(porta.escritos).toHaveLength(3);
  });

  it('pedido novo: imprime com a impressão automática ligada e avisa se falhar, sem lançar erro', async () => {
    const porta = new PortaFalsa();
    serial.getPorts.mockResolvedValue([porta]);
    criar();

    await servico.imprimirPedidoNovo(criarPedidoDetalhe()); // ainda sem impressora: nada acontece
    expect(porta.open).not.toHaveBeenCalled();

    await servico.reconectar();
    await servico.imprimirPedidoNovo(criarPedidoDetalhe());
    expect(porta.escritos).toHaveLength(1);

    porta.falhasNaEscrita = 2;
    await servico.imprimirPedidoNovo(criarPedidoDetalhe());
    expect(notificacao.erro).toHaveBeenCalledWith(expect.stringContaining('pedido EI-ABC234'));
  });

  it('desliga a impressão automática e lembra a escolha no aparelho', async () => {
    const porta = new PortaFalsa();
    serial.getPorts.mockResolvedValue([porta]);
    criar();
    await servico.reconectar();

    servico.alternarAutomatica();
    expect(servico.automatica()).toBe(false);
    expect(localStorage.getItem(CHAVE_IMPRESSAO_AUTOMATICA)).toBe('desligada');
    await servico.imprimirPedidoNovo(criarPedidoDetalhe());
    expect(porta.open).not.toHaveBeenCalled();

    TestBed.resetTestingModule();
    criar();
    expect(servico.automatica()).toBe(false);
    servico.alternarAutomatica();
    expect(localStorage.getItem(CHAVE_IMPRESSAO_AUTOMATICA)).toBe('ligada');
  });
});
