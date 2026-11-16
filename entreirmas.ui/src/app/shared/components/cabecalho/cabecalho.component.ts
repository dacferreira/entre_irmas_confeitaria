import { ChangeDetectionStrategy, Component, HostListener, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { ROTAS_ADMIN } from '@constants/admin';
import { ROTAS } from '@constants/rotas';
import { AuthService } from '@services/auth.service';
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
  readonly auth = inject(AuthService);
  readonly rotas = ROTAS;
  readonly rotasAdmin = ROTAS_ADMIN;
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
