import { CurrencyPipe, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material/dialog';
import { confirmar } from '@components/confirmacao/confirmacao.component';
import { IconeComponent } from '@components/icone/icone.component';
import { STATUS_PEDIDO, opcaoStatus } from '@constants/admin';
import { formatarTelefone } from '@directives/mascara-telefone.directive';
import { PedidoDetalhe, StatusPedido } from '@models/admin.model';
import { nomeDoParceiro } from '@models/pedido.model';
import { DataBrPipe } from '@pipes/data-br.pipe';
import { PedidoAdminService } from '@services/admin.service';
import { ImpressoraService } from '@services/impressora.service';
import { NotificacaoService } from '@services/notificacao.service';
import { filter, finalize, switchMap } from 'rxjs';

/** Detalhe do pedido. Fecha com true quando algo mudou (para a lista recarregar). */
@Component({
  selector: 'eirm-pedido-detalhe',
  imports: [CurrencyPipe, DatePipe, DataBrPipe, IconeComponent],
  templateUrl: './pedido-detalhe.component.html',
  styleUrl: './pedido-detalhe.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PedidoDetalheComponent {
  private readonly id = inject<number>(MAT_DIALOG_DATA);
  private readonly referencia = inject<MatDialogRef<PedidoDetalheComponent, boolean>>(MatDialogRef);
  private readonly dialogo = inject(MatDialog);
  private readonly pedidoService = inject(PedidoAdminService);
  private readonly notificacao = inject(NotificacaoService);
  readonly impressora = inject(ImpressoraService);

  readonly etapas = STATUS_PEDIDO;
  readonly opcaoStatus = opcaoStatus;
  readonly pedido = signal<PedidoDetalhe | null>(null);
  readonly salvando = signal(false);
  readonly chamandoEntregador = signal(false);
  readonly falhou = signal(false);
  readonly nomeDoParceiro = nomeDoParceiro;
  private alterou = false;

  readonly lucro = computed(() => {
    const pedido = this.pedido();
    return pedido ? pedido.valorTotal - pedido.custoTotal : 0;
  });

  readonly linkWhatsApp = computed(() => {
    const pedido = this.pedido();
    if (!pedido) {
      return '';
    }
    const texto = `Olá, ${pedido.nomeCliente.split(' ')[0]}! Aqui é da Entre Irmãs, sobre o pedido ${pedido.codigo}.`;
    return `https://wa.me/55${pedido.telefone}?text=${encodeURIComponent(texto)}`;
  });

  readonly telefone = computed(() => formatarTelefone(this.pedido()?.telefone ?? ''));

  constructor() {
    this.pedidoService.obter(this.id).subscribe({
      next: (pedido) => this.pedido.set(pedido),
      error: () => this.falhou.set(true),
    });
  }

  alterarStatus(status: StatusPedido): void {
    const pedido = this.pedido();
    if (!pedido || pedido.status === status || this.salvando()) {
      return;
    }
    this.salvando.set(true);
    this.pedidoService
      .alterarStatus(pedido.id, status)
      .pipe(finalize(() => this.salvando.set(false)))
      .subscribe((atualizado) => {
        const chamouAgora = atualizado.situacaoEntrega !== pedido.situacaoEntrega;
        this.pedido.set(atualizado);
        this.alterou = true;
        if (chamouAgora && atualizado.situacaoEntrega === 'Falhou') {
          this.notificacao.erro(`Pedido ${atualizado.codigo} pronto, mas o entregador não foi chamado: ${atualizado.erroEntrega}`);
          return;
        }
        const entregador =
          chamouAgora && atualizado.situacaoEntrega === 'Solicitada'
            ? ` Entregador da ${nomeDoParceiro(atualizado.parceiroEntrega)} chamado.`
            : '';
        this.notificacao.sucesso(`Pedido ${atualizado.codigo}: ${opcaoStatus(status).rotulo.toLowerCase()}.${entregador}`);
      });
  }

  /** Tenta de novo chamar o entregador da Uber/99 depois de uma falha. */
  chamarEntregador(): void {
    const pedido = this.pedido();
    if (!pedido || this.chamandoEntregador()) {
      return;
    }
    this.chamandoEntregador.set(true);
    this.pedidoService
      .chamarEntregador(pedido.id)
      .pipe(finalize(() => this.chamandoEntregador.set(false)))
      .subscribe((atualizado) => {
        this.pedido.set(atualizado);
        this.alterou = true;
        if (atualizado.situacaoEntrega === 'Solicitada') {
          this.notificacao.sucesso(`Entregador da ${nomeDoParceiro(atualizado.parceiroEntrega)} chamado.`);
        } else {
          this.notificacao.erro(`O entregador não foi chamado: ${atualizado.erroEntrega}`);
        }
      });
  }

  excluir(): void {
    const pedido = this.pedido();
    if (!pedido) {
      return;
    }
    confirmar(this.dialogo, {
      titulo: 'Excluir este pedido?',
      mensagem: `O pedido ${pedido.codigo} some da lista e do painel. Para desistência do cliente, prefira a etapa "Cancelado".`,
      confirmar: 'Excluir pedido',
      perigo: true,
    })
      .pipe(
        filter(Boolean),
        switchMap(() => this.pedidoService.excluir(pedido.id)),
      )
      .subscribe(() => {
        this.notificacao.sucesso(`Pedido ${pedido.codigo} excluído.`);
        this.referencia.close(true);
      });
  }

  /** Sem impressora escolhida, abre a lista de portas antes (o clique libera o pedido de permissão). */
  async imprimirRecibo(): Promise<void> {
    const pedido = this.pedido();
    if (!pedido || (!this.impressora.conectada() && !(await this.impressora.conectar()))) {
      return;
    }
    try {
      await this.impressora.imprimir(pedido);
      this.notificacao.sucesso(`Recibo do pedido ${pedido.codigo} enviado para a impressora.`);
    } catch {
      this.notificacao.erro('Não foi possível imprimir. Confira se a impressora está ligada e pareada no computador.');
    }
  }

  fechar(): void {
    this.referencia.close(this.alterou);
  }
}
