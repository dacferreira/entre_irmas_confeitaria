import { Injectable, inject, signal } from '@angular/core';
import { AcompanhamentoPedido, NovoPedido, PedidoCriado } from '@models/pedido.model';
import { Observable, tap } from 'rxjs';
import { ApiService } from './api.service';

export const CHAVE_ULTIMO_PEDIDO = 'eirm.ultimo-pedido';
export const CHAVE_PEDIDOS_RECENTES = 'eirm.pedidos-recentes';
export const MAXIMO_PEDIDOS_RECENTES = 5;

// Os mesmos caracteres do código gerado pela API (sem 0/O e 1/I/L).
const CARACTERES_DO_CODIGO = /^[2-9A-HJKMNP-Z]{6}$/;

/** "ei-abc 234", "EIABC234" e "abc234" viram "EI-ABC234"; fora do formato, null. */
export function normalizarCodigoPedido(valor: string | null | undefined): string | null {
  let letras = (valor ?? '').replace(/[^0-9a-z]/gi, '').toUpperCase();
  if (letras.length === 8 && letras.startsWith('EI')) {
    letras = letras.slice(2);
  }
  return CARACTERES_DO_CODIGO.test(letras) ? `EI-${letras}` : null;
}

@Injectable({ providedIn: 'root' })
export class PedidoService {
  private readonly api = inject(ApiService);
  private readonly _ultimoPedido = signal<PedidoCriado | null>(this.carregar());

  /** Pedido recém-criado, exibido na tela de confirmação (sobrevive a um recarregamento da aba). */
  readonly ultimoPedido = this._ultimoPedido.asReadonly();

  private readonly _pedidosRecentes = signal<string[]>(this.carregarRecentes());

  /** Códigos dos pedidos feitos ou consultados neste aparelho, do mais recente para o mais antigo. */
  readonly pedidosRecentes = this._pedidosRecentes.asReadonly();

  criar(pedido: NovoPedido): Observable<PedidoCriado> {
    return this.api.post<PedidoCriado>('pedidos', pedido).pipe(
      tap((criado) => {
        this.guardar(criado);
        this.lembrar(criado.codigo);
      }),
    );
  }

  /** Silencioso: a tela mostra o erro no lugar do resultado, sem o aviso geral do rodapé. */
  acompanhar(codigo: string): Observable<AcompanhamentoPedido> {
    return this.api
      .get<AcompanhamentoPedido>(`pedidos/${encodeURIComponent(codigo)}`, {}, { silencioso: true })
      .pipe(tap((pedido) => this.lembrar(pedido.codigo)));
  }

  private lembrar(codigo: string): void {
    const recentes = [codigo, ...this._pedidosRecentes().filter((c) => c !== codigo)].slice(0, MAXIMO_PEDIDOS_RECENTES);
    this._pedidosRecentes.set(recentes);
    try {
      localStorage.setItem(CHAVE_PEDIDOS_RECENTES, JSON.stringify(recentes));
    } catch {
      // Sem localStorage a lista vale só até fechar a aba.
    }
  }

  private carregarRecentes(): string[] {
    try {
      const salvos: unknown = JSON.parse(localStorage.getItem(CHAVE_PEDIDOS_RECENTES) ?? '[]');
      return Array.isArray(salvos)
        ? salvos.filter((c): c is string => typeof c === 'string').slice(0, MAXIMO_PEDIDOS_RECENTES)
        : [];
    } catch {
      return [];
    }
  }

  private guardar(pedido: PedidoCriado): void {
    this._ultimoPedido.set(pedido);
    try {
      sessionStorage.setItem(CHAVE_ULTIMO_PEDIDO, JSON.stringify(pedido));
    } catch {
      // Sem sessionStorage o pedido fica só em memória.
    }
  }

  private carregar(): PedidoCriado | null {
    try {
      const pedido = JSON.parse(sessionStorage.getItem(CHAVE_ULTIMO_PEDIDO) ?? 'null') as PedidoCriado | null;
      return typeof pedido?.codigo === 'string' ? pedido : null;
    } catch {
      return null;
    }
  }
}
