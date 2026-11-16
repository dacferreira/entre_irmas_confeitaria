import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, effect, inject, input, signal, untracked } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { IconeComponent } from '@components/icone/icone.component';
import { STATUS_PEDIDO, opcaoStatus } from '@constants/admin';
import { FiltroPedido, Pagina, PedidoResumo, StatusPedido } from '@models/admin.model';
import { DataBrPipe } from '@pipes/data-br.pipe';
import { PedidoAdminService } from '@services/admin.service';
import { AlertaPedidosService } from '@services/alerta-pedidos.service';
import { catchError, debounceTime, of, skip, switchMap, tap } from 'rxjs';
import { PedidoDetalheComponent } from './pedido-detalhe/pedido-detalhe.component';
import { NovoPedidoComponent } from './novo-pedido/novo-pedido.component';

const TAMANHO_DA_PAGINA = 20;

@Component({
  selector: 'eirm-pedidos',
  imports: [CurrencyPipe, FormsModule, MatFormFieldModule, MatInputModule, MatSelectModule, IconeComponent, DataBrPipe],
  templateUrl: './pedidos.component.html',
  styleUrl: './pedidos.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PedidosComponent {
  private readonly pedidoService = inject(PedidoAdminService);
  private readonly dialogo = inject(MatDialog);
  private readonly alerta = inject(AlertaPedidosService);

  /** Filtro de status vindo do painel (?status=Recebido). */
  readonly status = input<StatusPedido | ''>('');

  readonly opcoesStatus = STATUS_PEDIDO;
  readonly opcaoStatus = opcaoStatus;

  readonly filtro = signal<FiltroPedido>({ status: null, busca: '', inicio: '', fim: '', pagina: 1 });
  readonly pagina = signal<Pagina<PedidoResumo> | null>(null);
  readonly carregando = signal(true);

  readonly totalDePaginas = computed(() => {
    const pagina = this.pagina();
    return pagina ? Math.max(1, Math.ceil(pagina.total / TAMANHO_DA_PAGINA)) : 1;
  });

  constructor() {
    effect(() => {
      const status = this.status();
      untracked(() => this.alterarFiltro({ status: status || null }));
    });

    toObservable(this.filtro)
      .pipe(
        debounceTime(250),
        tap(() => this.carregando.set(true)),
        switchMap((filtro) =>
          this.pedidoService
            .listar({ ...filtro, tamanhoPagina: TAMANHO_DA_PAGINA })
            .pipe(catchError(() => of(null))),
        ),
        takeUntilDestroyed(),
      )
      .subscribe((pagina) => {
        this.pagina.set(pagina);
        this.carregando.set(false);
      });

    // Pedido novo pelo site: a lista se atualiza sozinha.
    toObservable(this.alerta.novosPedidos)
      .pipe(skip(1), takeUntilDestroyed())
      .subscribe(() => this.recarregar());
  }

  /** Qualquer filtro novo volta para a primeira página. */
  alterarFiltro(parcial: Partial<FiltroPedido>): void {
    this.filtro.update((atual) => ({ ...atual, ...parcial, pagina: 1 }));
  }

  irParaPagina(pagina: number): void {
    this.filtro.update((atual) => ({ ...atual, pagina }));
  }

  limparFiltros(): void {
    this.filtro.set({ status: null, busca: '', inicio: '', fim: '', pagina: 1 });
  }

  abrir(pedido: PedidoResumo): void {
    this.dialogo
      .open(PedidoDetalheComponent, {
        data: pedido.id,
        width: '720px',
        maxWidth: 'calc(100vw - 24px)',
        panelClass: 'eirm-dialogo',
        autoFocus: 'dialog',
      })
      .afterClosed()
      .subscribe((alterou) => alterou && this.recarregar());
  }

  novo(): void {
    this.dialogo
      .open(NovoPedidoComponent, {
        width: '760px',
        maxWidth: 'calc(100vw - 24px)',
        panelClass: 'eirm-dialogo',
        autoFocus: 'first-tabbable',
        disableClose: true,
      })
      .afterClosed()
      .subscribe((criado) => criado && this.recarregar());
  }

  private recarregar(): void {
    this.filtro.update((atual) => ({ ...atual }));
  }
}
