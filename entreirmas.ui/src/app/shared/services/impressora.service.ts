import { Injectable, computed, inject, signal } from '@angular/core';
import { PedidoDetalhe } from '@models/admin.model';
import { NotificacaoService } from './notificacao.service';
import { montarRecibo, montarReciboDeTeste } from './recibo-termico';

export const CHAVE_IMPRESSAO_AUTOMATICA = 'eirm.impressao-automatica';

/** Serviço "porta serial" do Bluetooth clássico (SPP), usado pelas impressoras térmicas. */
const SERVICO_SERIAL_BLUETOOTH = '00001101-0000-1000-8000-00805f9b34fb';

// O TypeScript ainda não traz os tipos da Web Serial; só o que é usado aqui.
interface PortaSerial {
  readonly writable: WritableStream<Uint8Array> | null;
  open(opcoes: { baudRate: number }): Promise<void>;
  close(): Promise<void>;
  /** Tira a permissão do site para a porta (Chrome/Edge 103+). */
  forget?(): Promise<void>;
}

interface WebSerial {
  getPorts(): Promise<PortaSerial[]>;
  requestPort(opcoes?: { allowedBluetoothServiceClassIds?: string[] }): Promise<PortaSerial>;
}

/**
 * Impressora térmica de recibos (58mm) pela Web Serial do Chrome e do Edge no computador: a impressora
 * bluetooth pareada no Windows vira uma porta COM. A pessoa escolhe a porta uma vez (conectar());
 * depois o navegador lembra e a área administrativa reconecta sozinha (reconectar()).
 */
@Injectable({ providedIn: 'root' })
export class ImpressoraService {
  private readonly notificacao = inject(NotificacaoService);
  private readonly serial = (navigator as Navigator & { serial?: WebSerial }).serial ?? null;

  private readonly _porta = signal<PortaSerial | null>(null);
  private readonly _automatica = signal(lerPreferenciaDeImpressao());
  private readonly _imprimindo = signal(false);
  /** Um recibo por vez: a porta não aceita dois escritores ao mesmo tempo. */
  private fila: Promise<void> = Promise.resolve();

  /** Chrome e Edge no computador; Firefox, Safari e celulares não têm a Web Serial. */
  readonly suportada = this.serial !== null;
  readonly conectada = computed(() => this._porta() !== null);
  readonly automatica = this._automatica.asReadonly();
  readonly imprimindo = this._imprimindo.asReadonly();

  /** Volta a usar a impressora já escolhida antes neste navegador (sem pedir nada à pessoa). */
  async reconectar(): Promise<void> {
    if (!this.serial || this._porta()) {
      return;
    }
    try {
      const [porta] = await this.serial.getPorts();
      this._porta.set(porta ?? null);
    } catch {
      // Sem permissão ou sem portas: fica desconectada.
    }
  }

  /**
   * Abre a lista de portas do navegador. Precisa de um clique da pessoa. Devolve se alguma foi escolhida.
   * As outras portas autorizadas antes (ex.: uma escolhida por engano) são esquecidas, para o reconectar()
   * não voltar para elas.
   */
  async conectar(): Promise<boolean> {
    if (!this.serial) {
      return false;
    }
    let porta: PortaSerial;
    try {
      porta = await this.serial.requestPort({ allowedBluetoothServiceClassIds: [SERVICO_SERIAL_BLUETOOTH] });
    } catch {
      // A pessoa fechou a lista sem escolher.
      return false;
    }
    if (porta !== this._porta()) {
      await this.fecharPorta();
    }
    this._porta.set(porta);
    await this.esquecerOutrasPortas(porta);
    return true;
  }

  alternarAutomatica(): void {
    const ligada = !this._automatica();
    this._automatica.set(ligada);
    try {
      localStorage.setItem(CHAVE_IMPRESSAO_AUTOMATICA, ligada ? 'ligada' : 'desligada');
    } catch {
      // Sem localStorage a preferência vale só nesta aba.
    }
  }

  imprimir(pedido: PedidoDetalhe): Promise<void> {
    return this.enviar(montarRecibo(pedido));
  }

  imprimirTeste(): Promise<void> {
    return this.enviar(montarReciboDeTeste());
  }

  /** Pedido novo do site: imprime se a impressão automática estiver ligada e houver impressora. Não lança erro. */
  async imprimirPedidoNovo(pedido: PedidoDetalhe): Promise<void> {
    if (!this._automatica() || !this.conectada()) {
      return;
    }
    try {
      await this.imprimir(pedido);
    } catch {
      this.notificacao.erro(
        `Não foi possível imprimir o recibo do pedido ${pedido.codigo}. Confira se a impressora está ligada e use "Imprimir recibo" no pedido.`,
      );
    }
  }

  private enviar(bytes: Uint8Array): Promise<void> {
    const envio = this.fila.then(() => this.escrever(bytes));
    // A fila segue mesmo se este envio falhar.
    this.fila = envio.catch(() => undefined);
    return envio;
  }

  private async escrever(bytes: Uint8Array): Promise<void> {
    const porta = this._porta();
    if (!porta) {
      throw new Error('Nenhuma impressora conectada.');
    }
    this._imprimindo.set(true);
    try {
      try {
        await this.escreverNaPorta(porta, bytes);
      } catch {
        // A conexão bluetooth cai quando a impressora desliga ou dorme: reabre e tenta mais uma vez.
        await this.fecharPorta();
        await this.escreverNaPorta(porta, bytes);
      }
    } catch (erro) {
      await this.fecharPorta();
      throw erro;
    } finally {
      this._imprimindo.set(false);
    }
  }

  private async escreverNaPorta(porta: PortaSerial, bytes: Uint8Array): Promise<void> {
    if (!porta.writable) {
      // Na porta bluetooth a velocidade não importa; 9600 é o padrão dessas impressoras no cabo.
      await porta.open({ baudRate: 9600 });
    }
    if (!porta.writable) {
      throw new Error('A impressora não abriu a conexão.');
    }
    const escritor = porta.writable.getWriter();
    try {
      await escritor.write(bytes);
    } finally {
      escritor.releaseLock();
    }
  }

  private async esquecerOutrasPortas(escolhida: PortaSerial): Promise<void> {
    try {
      const autorizadas = await this.serial?.getPorts();
      for (const porta of autorizadas ?? []) {
        if (porta !== escolhida) {
          await porta.close().catch(() => undefined);
          await porta.forget?.();
        }
      }
    } catch {
      // Sem como esquecer: a escolha atual vale nesta aba; o pior caso é reconectar na outra porta depois.
    }
  }

  private async fecharPorta(): Promise<void> {
    const porta = this._porta();
    if (porta?.writable) {
      await porta.close().catch(() => undefined);
    }
  }
}

function lerPreferenciaDeImpressao(): boolean {
  try {
    return localStorage.getItem(CHAVE_IMPRESSAO_AUTOMATICA) !== 'desligada';
  } catch {
    return true;
  }
}
