import { ChangeDetectionStrategy, Component, HostListener, OnInit, inject } from '@angular/core';
import { LojaService } from '@services/loja.service';

/**
 * Faixa no topo do site com a loja fechada: avisa que só encomendas estão sendo aceitas.
 * Consulta a situação ao abrir o site e sempre que a aba volta ao foco.
 */
@Component({
  selector: 'eirm-aviso-loja',
  templateUrl: './aviso-loja.component.html',
  styleUrl: './aviso-loja.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AvisoLojaComponent implements OnInit {
  readonly loja = inject(LojaService);

  ngOnInit(): void {
    this.loja.atualizar();
  }

  @HostListener('window:focus')
  aoVoltarParaAba(): void {
    this.loja.atualizar();
  }
}
