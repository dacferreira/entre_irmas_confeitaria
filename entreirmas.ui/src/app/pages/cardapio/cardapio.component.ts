import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { CardProdutoComponent } from '@components/card-produto/card-produto.component';
import { IlustracaoComponent } from '@components/ilustracao/ilustracao.component';
import { PageComponent } from '@components/page/page.component';
import { Categoria } from '@models/categoria.model';
import { Produto } from '@models/produto.model';
import { CatalogoService } from '@services/catalogo.service';
import { catchError, of } from 'rxjs';

export interface GrupoDoCardapio {
  slug: string;
  nome: string;
  produtos: Produto[];
}

/** Remove acentos e caixa para a busca achar "pe" em "Bicho de Pé". */
export function normalizarTexto(texto: string): string {
  return texto
    .normalize('NFD')
    .replace(/[̀-ͯ]/g, '')
    .toLowerCase()
    .trim();
}

@Component({
  selector: 'eirm-cardapio',
  imports: [PageComponent, CardProdutoComponent, IlustracaoComponent],
  templateUrl: './cardapio.component.html',
  styleUrl: './cardapio.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CardapioComponent {
  private readonly catalogo = inject(CatalogoService);
  private readonly router = inject(Router);

  /** Query param ?categoria= (ligado pelo withComponentInputBinding). */
  readonly categoria = input<string | undefined>();
  readonly busca = signal('');
  readonly esqueletos = [1, 2, 3, 4, 5, 6];

  // O cardápio é pequeno: carrega tudo uma vez e filtra na tela, sem nova chamada a cada clique.
  readonly produtos = toSignal<Produto[] | null | undefined>(
    this.catalogo.listarProdutos().pipe(catchError(() => of(null))),
    { initialValue: undefined },
  );

  readonly categorias = toSignal(this.catalogo.listarCategorias().pipe(catchError(() => of([] as Categoria[]))), {
    initialValue: [] as Categoria[],
  });

  readonly carregando = computed(() => this.produtos() === undefined);
  readonly falhou = computed(() => this.produtos() === null);
  readonly categoriaSelecionada = computed(() => this.categoria() ?? null);

  readonly grupos = computed<GrupoDoCardapio[]>(() => {
    const termo = normalizarTexto(this.busca());
    const categoria = this.categoriaSelecionada();
    const grupos = new Map<string, GrupoDoCardapio>();

    for (const produto of this.produtos() ?? []) {
      if (categoria && produto.categoriaSlug !== categoria) {
        continue;
      }
      if (termo && !normalizarTexto(`${produto.nome} ${produto.descricao} ${produto.categoriaNome}`).includes(termo)) {
        continue;
      }
      const grupo = grupos.get(produto.categoriaSlug) ?? {
        slug: produto.categoriaSlug,
        nome: produto.categoriaNome,
        produtos: [],
      };
      grupo.produtos.push(produto);
      grupos.set(produto.categoriaSlug, grupo);
    }

    return [...grupos.values()];
  });

  readonly totalEncontrado = computed(() => this.grupos().reduce((soma, grupo) => soma + grupo.produtos.length, 0));

  selecionar(slug: string | null): void {
    void this.router.navigate([], {
      queryParams: { categoria: slug },
      queryParamsHandling: 'merge',
      replaceUrl: true,
    });
  }

  aoBuscar(evento: Event): void {
    this.busca.set((evento.target as HTMLInputElement).value);
  }

  limparFiltros(): void {
    this.busca.set('');
    this.selecionar(null);
  }
}
