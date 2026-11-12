import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { IlustracaoComponent } from '@components/ilustracao/ilustracao.component';
import { PageComponent } from '@components/page/page.component';
import { ROTAS } from '@constants/rotas';

@Component({
  selector: 'eirm-nao-encontrado',
  imports: [RouterLink, PageComponent, IlustracaoComponent],
  templateUrl: './nao-encontrado.component.html',
  styleUrl: './nao-encontrado.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NaoEncontradoComponent {
  readonly rotas = ROTAS;
}
