import { Injectable, computed, inject, signal } from '@angular/core';
import { AlterarLoja, LojaAdmin, SituacaoLoja } from '@models/loja.model';
import { Observable, tap } from 'rxjs';
import { ApiService } from './api.service';

/**
 * Loja aberta ou fechada. O site consulta ao abrir e quando a aba volta ao foco; até a primeira resposta
 * a loja é considerada aberta (a API confere de novo no envio do pedido).
 */
@Injectable({ providedIn: 'root' })
export class LojaService {
  private readonly api = inject(ApiService);
  private readonly _situacao = signal<SituacaoLoja>({ aberta: true, mensagem: null });

  readonly situacao = this._situacao.asReadonly();
  readonly aberta = computed(() => this._situacao().aberta);

  /** Silencioso: se falhar, fica a última situação conhecida. */
  atualizar(): void {
    this.api.get<SituacaoLoja>('loja', {}, { silencioso: true }).subscribe({
      next: (situacao) => this._situacao.set(situacao),
      error: () => undefined,
    });
  }

  // Área administrativa
  obterAdmin(): Observable<LojaAdmin> {
    return this.api.get<LojaAdmin>('admin/loja');
  }

  alterar(dados: AlterarLoja): Observable<LojaAdmin> {
    return this.api
      .put<LojaAdmin>('admin/loja', dados)
      .pipe(tap((loja) => this._situacao.set({ aberta: loja.aberta, mensagem: loja.mensagem })));
  }
}
