import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { confirmar } from '@components/confirmacao/confirmacao.component';
import { IconeComponent } from '@components/icone/icone.component';
import { CategoriaAdmin } from '@models/admin.model';
import { CategoriaAdminService } from '@services/admin.service';
import { NotificacaoService } from '@services/notificacao.service';
import { filter, switchMap } from 'rxjs';
import { CategoriaFormComponent } from './categoria-form/categoria-form.component';

@Component({
  selector: 'eirm-categorias',
  imports: [IconeComponent],
  templateUrl: './categorias.component.html',
  styleUrl: './categorias.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CategoriasComponent {
  private readonly categoriaService = inject(CategoriaAdminService);
  private readonly dialogo = inject(MatDialog);
  private readonly notificacao = inject(NotificacaoService);

  readonly categorias = signal<CategoriaAdmin[] | null>(null);
  readonly falhou = signal(false);
  readonly alterando = signal<number | null>(null);

  constructor() {
    this.carregar();
  }

  abrirFormulario(categoria?: CategoriaAdmin): void {
    const lista = this.categorias() ?? [];
    this.dialogo
      .open(CategoriaFormComponent, {
        data: { categoria: categoria ?? null, proximaOrdem: Math.max(0, ...lista.map((c) => c.ordem)) + 1 },
        width: '520px',
        maxWidth: 'calc(100vw - 24px)',
        panelClass: 'eirm-dialogo',
        disableClose: true,
      })
      .afterClosed()
      .subscribe((salva) => salva && this.carregar());
  }

  alternarSituacao(categoria: CategoriaAdmin): void {
    this.alterando.set(categoria.id);
    this.categoriaService.alterarSituacao(categoria.id, !categoria.ativo).subscribe({
      next: () => {
        this.categorias.update((lista) =>
          (lista ?? []).map((c) => (c.id === categoria.id ? { ...c, ativo: !categoria.ativo } : c)),
        );
        this.alterando.set(null);
        this.notificacao.sucesso(
          categoria.ativo ? `${categoria.nome} e os produtos dela saíram do site.` : `${categoria.nome} voltou ao site.`,
        );
      },
      error: () => this.alterando.set(null),
    });
  }

  excluir(categoria: CategoriaAdmin): void {
    if (categoria.quantidadeProdutos > 0) {
      this.notificacao.erro(
        `${categoria.nome} tem ${categoria.quantidadeProdutos} produto(s). Mova ou exclua os produtos antes.`,
      );
      return;
    }
    confirmar(this.dialogo, {
      titulo: `Excluir ${categoria.nome}?`,
      mensagem: 'A categoria sai do site e desta lista.',
      confirmar: 'Excluir categoria',
      perigo: true,
    })
      .pipe(
        filter(Boolean),
        switchMap(() => this.categoriaService.excluir(categoria.id)),
      )
      .subscribe(() => {
        this.categorias.update((lista) => (lista ?? []).filter((c) => c.id !== categoria.id));
        this.notificacao.sucesso(`${categoria.nome} excluída.`);
      });
  }

  carregar(): void {
    this.falhou.set(false);
    this.categoriaService.listar().subscribe({
      next: (categorias) => this.categorias.set(categorias),
      error: () => this.falhou.set(true),
    });
  }
}
