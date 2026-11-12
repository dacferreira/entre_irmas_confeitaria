import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { IlustracaoComponent } from '@components/ilustracao/ilustracao.component';
import { ROTAS } from '@constants/rotas';
import { ContatoService } from '@services/contato.service';

interface Granulado {
  x: number;
  y: number;
  giro: number;
  cor: string;
}

/** Abertura da página inicial: chamada principal e composição ilustrada. */
@Component({
  selector: 'eirm-hero',
  imports: [RouterLink, IlustracaoComponent],
  templateUrl: './hero.component.html',
  styleUrl: './hero.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HeroComponent {
  readonly rotas = ROTAS;
  readonly linkWhatsApp = inject(ContatoService).linkWhatsApp();

  // Granulados decorativos em volta do bolo (posições em %).
  readonly granulados: Granulado[] = [
    { x: 8, y: 22, giro: 20, cor: '#ec6a91' },
    { x: 18, y: 70, giro: -35, cor: '#c5dc9a' },
    { x: 30, y: 8, giro: 60, cor: '#d9954a' },
    { x: 70, y: 6, giro: -15, cor: '#c5dc9a' },
    { x: 90, y: 30, giro: 45, cor: '#f8e3b0' },
    { x: 86, y: 78, giro: -60, cor: '#ec6a91' },
    { x: 52, y: 94, giro: 10, cor: '#d9954a' },
    { x: 4, y: 48, giro: 80, cor: '#f8e3b0' },
  ];
}
