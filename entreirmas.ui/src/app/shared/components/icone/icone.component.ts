import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

/** Traços (viewBox 24x24, stroke) dos ícones da área administrativa. */
const ICONES = {
  painel: ['M4 13h6V4H4z', 'M14 20h6v-9h-6z', 'M14 4h6v4h-6z', 'M4 20h6v-3H4z'],
  pedidos: ['M6 3h12v18l-3-2-3 2-3-2-3 2z', 'M9 8h6', 'M9 12h6'],
  produtos: ['M5 11h14l-1.5 9h-11z', 'M4 11c0-3 3-5 8-5s8 2 8 5', 'M12 3v3'],
  categorias: ['M4 4h7v7H4z', 'M13 4h7v7h-7z', 'M4 13h7v7H4z', 'M13 13h7v7h-7z'],
  despesas: ['M4 7h16v12H4z', 'M4 11h16', 'M8 15h3', 'M7 7l3-3h4l3 3'],
  usuarios: ['M9 11a3.5 3.5 0 1 0 0-7 3.5 3.5 0 0 0 0 7z', 'M3 20c0-3.5 2.7-6 6-6s6 2.5 6 6', 'M16 4.5a3.5 3.5 0 0 1 0 6.5', 'M18 14.5c1.8.8 3 2.8 3 5.5'],
  sair: ['M14 4h5v16h-5', 'M10 8l-4 4 4 4', 'M6 12h10'],
  site: ['M12 21a9 9 0 1 0 0-18 9 9 0 0 0 0 18z', 'M3 12h18', 'M12 3c2.5 2.5 3.5 5.5 3.5 9s-1 6.5-3.5 9c-2.5-2.5-3.5-5.5-3.5-9s1-6.5 3.5-9z'],
  mais: ['M12 5v14', 'M5 12h14'],
  editar: ['M4 20h4L19 9l-4-4L4 16z', 'M13 7l4 4'],
  lixeira: ['M5 7h14', 'M10 7V4h4v3', 'M7 7l1 13h8l1-13', 'M10 11v6', 'M14 11v6'],
  ver: ['M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7S2 12 2 12z', 'M12 15a3 3 0 1 0 0-6 3 3 0 0 0 0 6z'],
  ocultar: ['M3 3l18 18', 'M10.6 5.1A10 10 0 0 1 12 5c6.5 0 10 7 10 7a17 17 0 0 1-3 3.9', 'M6.2 6.3C3.6 8 2 12 2 12s3.5 7 10 7a9.7 9.7 0 0 0 4.4-1'],
  fechar: ['M6 6l12 12', 'M18 6L6 18'],
  menu: ['M4 7h16', 'M4 12h16', 'M4 17h16'],
  recolher: ['M11 7l-5 5 5 5', 'M18 7l-5 5 5 5'],
  expandir: ['M6 7l5 5-5 5', 'M13 7l5 5-5 5'],
  whatsapp: ['M4 20l1.3-4A8 8 0 1 1 8.5 19z', 'M9 9c0 3 3 6 6 6l1-1.5-2-1-1 .8a4 4 0 0 1-2.3-2.3l.8-1-1-2z'],
  alta: ['M4 17l6-6 4 4 6-7', 'M15 8h5v5'],
  baixa: ['M4 7l6 6 4-4 6 7', 'M15 16h5v-5'],
  foto: ['M4 8h3l2-3h6l2 3h3v11H4z', 'M12 16.5a3.5 3.5 0 1 0 0-7 3.5 3.5 0 0 0 0 7z'],
  loja: ['M4 10h16l-1.5-5h-13z', 'M5 10v10h14V10', 'M10 20v-5h4v5'],
  som: ['M4 10h4l5-4v12l-5-4H4z', 'M16 9a4 4 0 0 1 0 6', 'M18.5 6.5a7.5 7.5 0 0 1 0 11'],
  mudo: ['M4 10h4l5-4v12l-5-4H4z', 'M16 10l5 5', 'M21 10l-5 5'],
  impressora: ['M7 9V3h10v6', 'M7 17H4v-6a2 2 0 0 1 2-2h12a2 2 0 0 1 2 2v6h-3', 'M7 14h10v7H7z'],
} as const;

export type NomeIcone = keyof typeof ICONES;

@Component({
  selector: 'eirm-icone',
  template: `
    <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false">
      @for (traco of tracos(); track $index) {
        <path [attr.d]="traco" />
      }
    </svg>
  `,
  styles: `
    :host {
      display: inline-block;
      width: 1.25em;
      height: 1.25em;
      flex-shrink: 0;
    }

    svg {
      width: 100%;
      height: 100%;
      fill: none;
      stroke: currentColor;
      stroke-width: 1.8;
      stroke-linecap: round;
      stroke-linejoin: round;
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IconeComponent {
  readonly nome = input.required<NomeIcone>();
  readonly tracos = computed<readonly string[]>(() => ICONES[this.nome()] ?? []);
}
