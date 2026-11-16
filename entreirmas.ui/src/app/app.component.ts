import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { AvisoLojaComponent } from '@components/aviso-loja/aviso-loja.component';
import { CabecalhoComponent } from '@components/cabecalho/cabecalho.component';
import { RodapeComponent } from '@components/rodape/rodape.component';
import { SacolaComponent } from '@components/sacola/sacola.component';
import { ROTAS_ADMIN } from '@constants/admin';
import { filter, map } from 'rxjs';

@Component({
  selector: 'eirm-root',
  imports: [RouterOutlet, CabecalhoComponent, AvisoLojaComponent, RodapeComponent, SacolaComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppComponent {
  private readonly router = inject(Router);

  /** A área administrativa tem layout próprio: sem cabeçalho, rodapé e sacola do site. */
  readonly areaAdmin = toSignal(
    this.router.events.pipe(
      filter((evento) => evento instanceof NavigationEnd),
      map((evento) => ehAreaAdmin(evento.urlAfterRedirects)),
    ),
    { initialValue: ehAreaAdmin(this.router.url) },
  );
}

function ehAreaAdmin(url: string): boolean {
  return url === `/${ROTAS_ADMIN.BASE}` || url.startsWith(`/${ROTAS_ADMIN.BASE}/`) || url.startsWith(`/${ROTAS_ADMIN.BASE}?`);
}
