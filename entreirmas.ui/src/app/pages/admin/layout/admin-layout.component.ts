import { A11yModule } from '@angular/cdk/a11y';
import { ChangeDetectionStrategy, Component, HostListener, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { IconeComponent, NomeIcone } from '@components/icone/icone.component';
import { ROTAS_ADMIN } from '@constants/admin';
import { AlertaPedidosService } from '@services/alerta-pedidos.service';
import { AuthService } from '@services/auth.service';
import { ImpressoraService } from '@services/impressora.service';
import { filter } from 'rxjs';

export const CHAVE_MENU_RECOLHIDO = 'eirm.menu-recolhido';

interface ItemDoMenu {
  rota: string;
  rotulo: string;
  icone: NomeIcone;
}

/** Moldura da área logada: menu com os cadastros, nome de quem entrou e o botão de sair. */
@Component({
  selector: 'eirm-admin-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, IconeComponent, A11yModule],
  templateUrl: './admin-layout.component.html',
  styleUrl: './admin-layout.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminLayoutComponent {
  readonly auth = inject(AuthService);
  readonly alerta = inject(AlertaPedidosService);
  private readonly impressora = inject(ImpressoraService);
  readonly menuAberto = signal(false);
  /** Menu do computador só com os ícones (a escolha fica no aparelho). */
  readonly recolhido = signal(lerMenuRecolhido());

  readonly menu: readonly ItemDoMenu[] = [
    { rota: ROTAS_ADMIN.PAINEL, rotulo: 'Painel', icone: 'painel' },
    { rota: ROTAS_ADMIN.PEDIDOS, rotulo: 'Pedidos', icone: 'pedidos' },
    { rota: ROTAS_ADMIN.PRODUTOS, rotulo: 'Produtos', icone: 'produtos' },
    { rota: ROTAS_ADMIN.CATEGORIAS, rotulo: 'Categorias', icone: 'categorias' },
    { rota: ROTAS_ADMIN.LANCAMENTOS, rotulo: 'Despesas e perdas', icone: 'despesas' },
    { rota: ROTAS_ADMIN.LOJA, rotulo: 'Loja', icone: 'loja' },
    { rota: ROTAS_ADMIN.USUARIOS, rotulo: 'Usuários', icone: 'usuarios' },
  ];

  constructor() {
    // Fecha a gaveta do celular ao trocar de tela.
    inject(Router)
      .events.pipe(
        filter((evento) => evento instanceof NavigationEnd),
        takeUntilDestroyed(),
      )
      .subscribe(() => this.menuAberto.set(false));

    // Aviso de pedido novo enquanto a área logada estiver aberta (qualquer tela), com o recibo na impressora.
    void this.impressora.reconectar();
    this.alerta
      .observar()
      .pipe(takeUntilDestroyed())
      .subscribe((pedido) => void this.impressora.imprimirPedidoNovo(pedido));
  }

  /** O navegador só libera o som depois de um gesto da pessoa na página. */
  @HostListener('document:pointerdown')
  @HostListener('document:keydown')
  aoInteragir(): void {
    this.alerta.prepararAudio();
  }

  @HostListener('document:keydown.escape')
  aoApertarEsc(): void {
    this.menuAberto.set(false);
  }

  alternarMenu(): void {
    this.menuAberto.update((aberto) => !aberto);
  }

  alternarRecolhido(): void {
    const recolhido = !this.recolhido();
    this.recolhido.set(recolhido);
    try {
      localStorage.setItem(CHAVE_MENU_RECOLHIDO, recolhido ? 'sim' : 'nao');
    } catch {
      // Sem localStorage a escolha vale só nesta aba.
    }
  }

  sair(): void {
    this.auth.sair();
  }

  iniciais(): string {
    return this.auth
      .nome()
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((parte) => parte[0].toUpperCase())
      .join('');
  }
}

function lerMenuRecolhido(): boolean {
  try {
    return localStorage.getItem(CHAVE_MENU_RECOLHIDO) === 'sim';
  } catch {
    return false;
  }
}
