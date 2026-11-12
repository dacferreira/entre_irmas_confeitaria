import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { normalizarIlustracao } from '@constants/ilustracoes';

/** Ilustração vetorial de cada tipo de doce, usada quando o produto ainda não tem foto. */
@Component({
  selector: 'eirm-ilustracao',
  templateUrl: './ilustracao.component.html',
  styleUrl: './ilustracao.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IlustracaoComponent {
  readonly tipo = input<string | null | undefined>('cupcake');
  readonly rotulo = input('');

  readonly tipoNormalizado = computed(() => normalizarIlustracao(this.tipo()));
}
