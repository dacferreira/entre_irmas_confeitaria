import { A11yModule } from '@angular/cdk/a11y';
import { CurrencyPipe, DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, HostListener, computed, effect, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink } from '@angular/router';
import { IlustracaoComponent } from '@components/ilustracao/ilustracao.component';
import { SeletorQuantidadeComponent } from '@components/seletor-quantidade/seletor-quantidade.component';
import { normalizarIlustracao } from '@constants/ilustracoes';
import { ROTAS } from '@constants/rotas';
import { ItemSacola } from '@models/sacola.model';
import { SacolaService } from '@services/sacola.service';
import { filter, map } from 'rxjs';

// Nestas telas a barra flutuante da sacola só atrapalharia.
const ROTAS_SEM_BARRA = new RegExp(`^/(${ROTAS.FINALIZAR_PEDIDO}|${ROTAS.PEDIDO_CONFIRMADO})`);

/** Painel da sacola (lateral no desktop, folha inferior no celular) e barra flutuante no celular. */
@Component({
  selector: 'eirm-sacola',
  imports: [A11yModule, CurrencyPipe, RouterLink, IlustracaoComponent, SeletorQuantidadeComponent],
  templateUrl: './sacola.component.html',
  styleUrl: './sacola.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SacolaComponent {
  private readonly router = inject(Router);
  private readonly documento = inject(DOCUMENT);

  readonly sacola = inject(SacolaService);
  readonly rotas = ROTAS;

  private readonly url = toSignal(
    this.router.events.pipe(
      filter((evento): evento is NavigationEnd => evento instanceof NavigationEnd),
      map((evento) => evento.urlAfterRedirects),
    ),
    { initialValue: this.router.url },
  );

  readonly mostrarBarra = computed(
    () => !this.sacola.vazia() && !this.sacola.aberta() && !ROTAS_SEM_BARRA.test(this.url()),
  );

  constructor() {
    // Com o painel aberto, a página de trás não rola.
    effect(() => this.documento.body.classList.toggle('sem-rolagem', this.sacola.aberta()));
  }

  @HostListener('document:keydown.escape')
  aoPressionarEsc(): void {
    this.sacola.fechar();
  }

  sabor(item: ItemSacola): string {
    return normalizarIlustracao(item.produto.ilustracao);
  }

  alterar(item: ItemSacola, quantidade: number): void {
    this.sacola.alterarQuantidade(item.produto.id, quantidade);
  }

  finalizar(): void {
    this.sacola.fechar();
    void this.router.navigate(['/', ROTAS.FINALIZAR_PEDIDO]);
  }
}
