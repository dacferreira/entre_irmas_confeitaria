import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { IconeComponent } from '@components/icone/icone.component';
import { ImpressoraService } from '@services/impressora.service';
import { NotificacaoService } from '@services/notificacao.service';

/** Liga a impressora térmica de recibos e a impressão automática dos pedidos novos do site. */
@Component({
  selector: 'eirm-impressora-recibos',
  imports: [IconeComponent],
  templateUrl: './impressora-recibos.component.html',
  styleUrl: './impressora-recibos.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ImpressoraRecibosComponent {
  readonly impressora = inject(ImpressoraService);
  private readonly notificacao = inject(NotificacaoService);

  async conectar(): Promise<void> {
    if (await this.impressora.conectar()) {
      this.notificacao.sucesso('Impressora conectada. Imprima um teste para conferir.');
    }
  }

  async imprimirTeste(): Promise<void> {
    try {
      await this.impressora.imprimirTeste();
      this.notificacao.sucesso('Teste enviado para a impressora.');
    } catch {
      this.notificacao.erro('Não foi possível imprimir. Confira se a impressora está ligada e pareada no computador.');
    }
  }
}
