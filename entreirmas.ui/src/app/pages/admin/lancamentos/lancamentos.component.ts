import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { confirmar } from '@components/confirmacao/confirmacao.component';
import { IconeComponent } from '@components/icone/icone.component';
import { TIPOS_LANCAMENTO, periodoDoMes } from '@constants/admin';
import { FiltroLancamento, Lancamento } from '@models/admin.model';
import { DataBrPipe } from '@pipes/data-br.pipe';
import { LancamentoService } from '@services/admin.service';
import { NotificacaoService } from '@services/notificacao.service';
import { catchError, filter, of, switchMap } from 'rxjs';
import { LancamentoFormComponent } from './lancamento-form/lancamento-form.component';

@Component({
  selector: 'eirm-lancamentos',
  imports: [CurrencyPipe, FormsModule, MatFormFieldModule, MatInputModule, MatSelectModule, IconeComponent, DataBrPipe],
  templateUrl: './lancamentos.component.html',
  styleUrl: './lancamentos.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LancamentosComponent {
  private readonly lancamentoService = inject(LancamentoService);
  private readonly dialogo = inject(MatDialog);
  private readonly notificacao = inject(NotificacaoService);

  readonly tipos = TIPOS_LANCAMENTO;
  readonly filtro = signal<FiltroLancamento>({ ...periodoDoMes(new Date()), tipo: null });
  readonly lancamentos = signal<Lancamento[] | null>(null);
  readonly falhou = signal(false);

  readonly totais = computed(() => {
    const lista = this.lancamentos() ?? [];
    const soma = (tipo: string): number => lista.filter((l) => l.tipo === tipo).reduce((t, l) => t + l.valor, 0);
    return { despesas: soma('Despesa'), perdas: soma('Perda') };
  });

  constructor() {
    toObservable(this.filtro)
      .pipe(
        switchMap((filtro) => {
          this.falhou.set(false);
          return this.lancamentoService.listar(filtro).pipe(
            catchError(() => {
              this.falhou.set(true);
              return of(null);
            }),
          );
        }),
        takeUntilDestroyed(),
      )
      .subscribe((lista) => this.lancamentos.set(lista));
  }

  alterarFiltro(parcial: Partial<FiltroLancamento>): void {
    this.filtro.update((atual) => ({ ...atual, ...parcial }));
  }

  abrirFormulario(lancamento?: Lancamento): void {
    this.dialogo
      .open(LancamentoFormComponent, {
        data: lancamento ?? null,
        width: '520px',
        maxWidth: 'calc(100vw - 24px)',
        panelClass: 'eirm-dialogo',
        disableClose: true,
      })
      .afterClosed()
      .subscribe((salvo) => salvo && this.recarregar());
  }

  excluir(lancamento: Lancamento): void {
    confirmar(this.dialogo, {
      titulo: 'Excluir este lançamento?',
      mensagem: `"${lancamento.descricao}" deixa de entrar no cálculo do lucro.`,
      confirmar: 'Excluir',
      perigo: true,
    })
      .pipe(
        filter(Boolean),
        switchMap(() => this.lancamentoService.excluir(lancamento.id)),
      )
      .subscribe(() => {
        this.lancamentos.update((lista) => (lista ?? []).filter((l) => l.id !== lancamento.id));
        this.notificacao.sucesso('Lançamento excluído.');
      });
  }

  recarregar(): void {
    this.filtro.update((atual) => ({ ...atual }));
  }
}
