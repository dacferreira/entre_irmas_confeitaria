import { Injectable, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ROTAS_ADMIN } from '@constants/admin';
import { PedidoDetalhe, UltimoPedido } from '@models/admin.model';
import { Observable, catchError, exhaustMap, filter, from, map, mergeMap, of, tap, timer } from 'rxjs';
import { ApiService } from './api.service';
import { NotificacaoService } from './notificacao.service';

export const INTERVALO_ALERTA_MS = 20_000;
export const CHAVE_SOM_PEDIDOS = 'eirm.som-pedidos';

/** Notas do aviso (dó, mi, sol agudos), em Hz. */
const NOTAS_DO_AVISO = [1046.5, 1318.5, 1568];

/** Quantas vezes as notas se repetem, para o aviso não passar despercebido. */
const REPETICOES_DO_AVISO = 2;

/** Volume de pico de cada nota (0 a 1). O compressor segura o pico quando as notas se sobrepõem. */
const VOLUME_DO_AVISO = 0.9;

/**
 * Aviso de pedido novo na área administrativa: consulta os pedidos do site de tempos em tempos e, quando
 * chega pedido, toca um som e mostra a notificação. O som é gerado pelo navegador (Web Audio), sem arquivo.
 * Navegadores só liberam o áudio depois de um clique na página: o layout chama prepararAudio() no primeiro clique.
 */
@Injectable({ providedIn: 'root' })
export class AlertaPedidosService {
  private readonly api = inject(ApiService);
  private readonly notificacao = inject(NotificacaoService);
  private readonly router = inject(Router);

  private readonly _somLigado = signal(lerPreferenciaDeSom());
  private readonly _novosPedidos = signal(0);
  private contextoDeAudio: AudioContext | null = null;

  readonly somLigado = this._somLigado.asReadonly();

  /** Quantos pedidos novos chegaram nesta sessão; telas como a de Pedidos recarregam quando muda. */
  readonly novosPedidos = this._novosPedidos.asReadonly();

  /**
   * Observa os pedidos enquanto houver inscrição e emite cada pedido novo do site, com os itens (para o recibo).
   * A primeira consulta só marca o ponto de partida (o que já existia não toca); as seguintes trazem todos os
   * pedidos depois dele, mesmo que vários cheguem juntos. Falhas são ignoradas e a próxima consulta tenta de novo.
   */
  observar(intervaloMs = INTERVALO_ALERTA_MS): Observable<PedidoDetalhe> {
    let ultimoId: number | null = null;

    const marcarPontoDePartida = (): Observable<PedidoDetalhe[]> =>
      this.api.get<UltimoPedido | null>('admin/pedidos/ultimo-do-site', {}, { silencioso: true }).pipe(
        tap((ultimo) => (ultimoId = ultimo?.id ?? 0)),
        map((): PedidoDetalhe[] => []),
      );

    const buscarNovos = (depoisDe: number): Observable<PedidoDetalhe[]> =>
      this.api.get<PedidoDetalhe[]>('admin/pedidos/novos-do-site', { depoisDe }, { silencioso: true }).pipe(
        tap((novos) => (ultimoId = Math.max(depoisDe, ...novos.map((pedido) => pedido.id)))),
      );

    return timer(0, intervaloMs).pipe(
      // exhaustMap: com a rede lenta, não empilha consultas.
      exhaustMap(() =>
        (ultimoId === null ? marcarPontoDePartida() : buscarNovos(ultimoId)).pipe(catchError(() => of([]))),
      ),
      filter((novos) => novos.length > 0),
      tap((novos) => this.avisar(novos)),
      mergeMap((novos) => from(novos)),
    );
  }

  alternarSom(): void {
    const ligado = !this._somLigado();
    this._somLigado.set(ligado);
    try {
      localStorage.setItem(CHAVE_SOM_PEDIDOS, ligado ? 'ligado' : 'desligado');
    } catch {
      // Sem localStorage a preferência vale só nesta aba.
    }
    if (ligado) {
      // O clique no botão libera o áudio; o som serve de teste.
      this.tocar();
    }
  }

  /** Cria (ou retoma) o áudio durante um gesto da pessoa, para o aviso poder tocar depois. */
  prepararAudio(): void {
    const contexto = this.obterContexto();
    if (contexto?.state === 'suspended') {
      void contexto.resume().catch(() => undefined);
    }
  }

  tocar(): void {
    if (!this._somLigado()) {
      return;
    }
    const contexto = this.obterContexto();
    if (!contexto) {
      return;
    }
    if (contexto.state === 'suspended') {
      void contexto.resume().catch(() => undefined);
    }

    // O compressor evita que o som distorça com o volume alto.
    const compressor = contexto.createDynamicsCompressor();
    compressor.threshold.value = -6;
    compressor.ratio.value = 12;
    compressor.connect(contexto.destination);

    const agora = contexto.currentTime;
    for (let repeticao = 0; repeticao < REPETICOES_DO_AVISO; repeticao++) {
      NOTAS_DO_AVISO.forEach((frequencia, i) => {
        const inicio = agora + repeticao * 0.8 + i * 0.16;
        const oscilador = contexto.createOscillator();
        const volume = contexto.createGain();
        // Triangular soa mais forte que a senoidal no mesmo volume, sem ficar estridente.
        oscilador.type = 'triangle';
        oscilador.frequency.value = frequencia;
        volume.gain.setValueAtTime(0.0001, inicio);
        volume.gain.exponentialRampToValueAtTime(VOLUME_DO_AVISO, inicio + 0.02);
        volume.gain.exponentialRampToValueAtTime(0.0001, inicio + 0.6);
        oscilador.connect(volume).connect(compressor);
        oscilador.start(inicio);
        oscilador.stop(inicio + 0.65);
      });
    }
  }

  /** Um som e uma mensagem por consulta, mesmo que tenham chegado vários pedidos. */
  private avisar(pedidos: PedidoDetalhe[]): void {
    this._novosPedidos.update((total) => total + pedidos.length);
    this.tocar();
    const [pedido] = pedidos;
    const mensagem =
      pedidos.length === 1 ? `Pedido novo: ${pedido.nomeCliente} (${pedido.codigo})` : `${pedidos.length} pedidos novos`;
    this.notificacao
      .sucesso(mensagem, 'Ver')
      .subscribe(() => void this.router.navigate(['/', ROTAS_ADMIN.BASE, ROTAS_ADMIN.PEDIDOS]));
  }

  private obterContexto(): AudioContext | null {
    if (this.contextoDeAudio) {
      return this.contextoDeAudio;
    }
    if (typeof AudioContext === 'undefined') {
      return null;
    }
    try {
      this.contextoDeAudio = new AudioContext();
    } catch {
      return null;
    }
    return this.contextoDeAudio;
  }
}

function lerPreferenciaDeSom(): boolean {
  try {
    return localStorage.getItem(CHAVE_SOM_PEDIDOS) !== 'desligado';
  } catch {
    return true;
  }
}
