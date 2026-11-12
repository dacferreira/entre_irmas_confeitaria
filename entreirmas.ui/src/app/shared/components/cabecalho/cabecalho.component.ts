import { ChangeDetectionStrategy, Component, HostListener, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { ROTAS } from '@constants/rotas';
import { SacolaService } from '@services/sacola.service';

@Component({
  selector: 'eirm-cabecalho',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './cabecalho.component.html',
  styleUrl: './cabecalho.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CabecalhoComponent {
  readonly sacola = inject(SacolaService);
  readonly rotas = ROTAS;
  readonly menuAberto = signal(false);
  readonly rolado = signal(false);

  @HostListener('window:scroll')
  aoRolar(): void {
    this.rolado.set(window.scrollY > 8);
  }

  alternarMenu(): void {
    this.menuAberto.update((aberto) => !aberto);
  }

  fecharMenu(): void {
    this.menuAberto.set(false);
  }

  abrirSacola(): void {
    this.fecharMenu();
    this.sacola.abrir();
  }
}
