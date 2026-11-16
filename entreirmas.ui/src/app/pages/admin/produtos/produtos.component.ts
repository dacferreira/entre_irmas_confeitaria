import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { confirmar } from '@components/confirmacao/confirmacao.component';
import { IconeComponent } from '@components/icone/icone.component';
import { IlustracaoComponent } from '@components/ilustracao/ilustracao.component';
import { ProdutoAdmin } from '@models/admin.model';
import { UrlImagemPipe } from '@pipes/url-imagem.pipe';
import { CategoriaAdminService, ProdutoAdminService } from '@services/admin.service';
import { NotificacaoService } from '@services/notificacao.service';
import { filter, switchMap } from 'rxjs';
import { ProdutoFormComponent } from './produto-form/produto-form.component';

@Component({
  selector: 'eirm-produtos',
  imports: [
    CurrencyPipe,
    DecimalPipe,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    IconeComponent,
    IlustracaoComponent,
    UrlImagemPipe,
  ],
  templateUrl: './produtos.component.html',
  styleUrl: './produtos.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProdutosComponent {
  private readonly produtoService = inject(ProdutoAdminService);
  private readonly dialogo = inject(MatDialog);
  private readonly notificacao = inject(NotificacaoService);

  readonly categorias = toSignal(inject(CategoriaAdminService).listar(), { initialValue: [] });
  readonly produtos = signal<ProdutoAdmin[] | null>(null);
  readonly falhou = signal(false);
  readonly alterando = signal<number | null>(null);

  readonly busca = signal('');
  readonly categoriaId = signal<number | null>(null);
  readonly situacao = signal<'todos' | 'ativos' | 'inativos'>('todos');

  /** Filtro feito na tela: a lista de uma confeitaria é pequena. */
  readonly filtrados = computed(() => {
    const termo = normalizar(this.busca());
    const categoria = this.categoriaId();
    const situacao = this.situacao();
    return (this.produtos() ?? []).filter(
      (p) =>
        (!termo || normalizar(p.nome).includes(termo)) &&
        (!categoria || p.categoriaId === categoria) &&
        (situacao === 'todos' || p.ativo === (situacao === 'ativos')),
    );
  });

  constructor() {
    this.carregar();
  }

  margem(produto: ProdutoAdmin): number {
    return produto.preco > 0 ? ((produto.preco - produto.custo) / produto.preco) * 100 : 0;
  }

  abrirFormulario(produto?: ProdutoAdmin): void {
    this.dialogo
      .open(ProdutoFormComponent, {
        data: { produto: produto ?? null, categorias: this.categorias() },
        width: '720px',
        maxWidth: 'calc(100vw - 24px)',
        panelClass: 'eirm-dialogo',
        disableClose: true,
      })
      .afterClosed()
      .subscribe((salvo) => salvo && this.carregar());
  }

  alternarSituacao(produto: ProdutoAdmin): void {
    this.alterando.set(produto.id);
    this.produtoService.alterarSituacao(produto.id, !produto.ativo).subscribe({
      next: () => {
        this.produtos.update((lista) =>
          (lista ?? []).map((p) => (p.id === produto.id ? { ...p, ativo: !produto.ativo } : p)),
        );
        this.alterando.set(null);
        this.notificacao.sucesso(produto.ativo ? `${produto.nome} saiu do cardápio.` : `${produto.nome} voltou ao cardápio.`);
      },
      error: () => this.alterando.set(null),
    });
  }

  excluir(produto: ProdutoAdmin): void {
    confirmar(this.dialogo, {
      titulo: `Excluir ${produto.nome}?`,
      mensagem: 'O produto sai do cardápio e desta lista. Os pedidos antigos continuam com ele. Para tirar só por um tempo, desative.',
      confirmar: 'Excluir produto',
      perigo: true,
    })
      .pipe(
        filter(Boolean),
        switchMap(() => this.produtoService.excluir(produto.id)),
      )
      .subscribe(() => {
        this.produtos.update((lista) => (lista ?? []).filter((p) => p.id !== produto.id));
        this.notificacao.sucesso(`${produto.nome} excluído.`);
      });
  }

  carregar(): void {
    this.falhou.set(false);
    this.produtoService.listar().subscribe({
      next: (produtos) => this.produtos.set(produtos),
      error: () => this.falhou.set(true),
    });
  }
}

function normalizar(texto: string): string {
  return texto.normalize('NFD').replace(/[̀-ͯ]/g, '').toLowerCase().trim();
}
