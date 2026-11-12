import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ROTAS } from '@constants/rotas';
import { environment } from '@environments/environment';
import { ContatoService } from '@services/contato.service';

@Component({
  selector: 'eirm-rodape',
  imports: [RouterLink],
  templateUrl: './rodape.component.html',
  styleUrl: './rodape.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RodapeComponent {
  readonly contato = inject(ContatoService);
  readonly rotas = ROTAS;
  readonly ano = new Date().getFullYear();
  readonly antecedencia = environment.antecedenciaMinimaDias;
  readonly linkWhatsApp = this.contato.linkWhatsApp();
}
