import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { IlustracaoComponent } from '@components/ilustracao/ilustracao.component';
import { PageComponent } from '@components/page/page.component';
import { ROTAS } from '@constants/rotas';
import { nomeDoParceiro } from '@models/pedido.model';
import { DataBrPipe } from '@pipes/data-br.pipe';
import { ContatoService } from '@services/contato.service';
import { PedidoService } from '@services/pedido.service';

const CORES_DO_CONFETE = ['#ec6a91', '#f8e3b0', '#c5dc9a', '#d9954a', '#c2385f'];

interface Confete {
  esquerda: number;
  atraso: number;
  cor: string;
  giro: number;
}

@Component({
  selector: 'eirm-pedido-confirmado',
  imports: [CurrencyPipe, RouterLink, PageComponent, IlustracaoComponent, DataBrPipe],
  templateUrl: './pedido-confirmado.component.html',
  styleUrl: './pedido-confirmado.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PedidoConfirmadoComponent {
  readonly pedido = inject(PedidoService).ultimoPedido;
  readonly contato = inject(ContatoService);
  readonly rotas = ROTAS;
  readonly nomeDoParceiro = nomeDoParceiro;

  // Posições espalhadas de forma determinística (sem Math.random, para o teste ser estável).
  readonly confetes: Confete[] = Array.from({ length: 24 }, (_, i) => ({
    esquerda: (i * 41) % 100,
    atraso: (i % 8) * 0.12,
    cor: CORES_DO_CONFETE[i % CORES_DO_CONFETE.length],
    giro: (i * 67) % 360,
  }));
}
