import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/** Layout padrão das telas internas: cabeçalho com títulos e área de conteúdo. */
@Component({
  selector: 'eirm-page',
  templateUrl: './page.component.html',
  styleUrl: './page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PageComponent {
  readonly titulo = input.required<string>();
  readonly sobretitulo = input('');
  readonly subtitulo = input('');
  readonly largura = input<'padrao' | 'estreita'>('padrao');
  readonly alinhamento = input<'esquerda' | 'centro'>('esquerda');
}
