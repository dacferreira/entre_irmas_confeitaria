import { Injectable, inject, signal } from '@angular/core';
import { NovoPedido, PedidoCriado } from '@models/pedido.model';
import { Observable, tap } from 'rxjs';
import { ApiService } from './api.service';

export const CHAVE_ULTIMO_PEDIDO = 'eirm.ultimo-pedido';

@Injectable({ providedIn: 'root' })
export class PedidoService {
  private readonly api = inject(ApiService);
  private readonly _ultimoPedido = signal<PedidoCriado | null>(this.carregar());

  /** Pedido recém-criado, exibido na tela de confirmação (sobrevive a um recarregamento da aba). */
  readonly ultimoPedido = this._ultimoPedido.asReadonly();

  criar(pedido: NovoPedido): Observable<PedidoCriado> {
    return this.api.post<PedidoCriado>('pedidos', pedido).pipe(tap((criado) => this.guardar(criado)));
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
