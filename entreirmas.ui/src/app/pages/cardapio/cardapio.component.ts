import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { CardProdutoComponent } from '@components/card-produto/card-produto.component';
import { IlustracaoComponent } from '@components/ilustracao/ilustracao.component';
import { PageComponent } from '@components/page/page.component';
import { environment } from '@environments/environment';
import { Categoria } from '@models/categoria.model';
import { Produto } from '@models/produto.model';
import { CatalogoService } from '@services/catalogo.service';
import { catchError, of } from 'rxjs';

export interface GrupoDoCardapio {
  slug: string;
  nome: string;
  produtos: Produto[];
}

/** Pronta entrega primeiro; os doces sob encomenda ficam por último. */
export interface BlocoDoCardapio {
  encomenda: boolean;
  grupos: GrupoDoCardapio[];
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
  readonly antecedencia = environment.antecedenciaMinimaDias;

  readonly blocos = computed<BlocoDoCardapio[]>(() => {
    const termo = normalizarTexto(this.busca());
    const categoria = this.categoriaSelecionada();
    const filtrados = (this.produtos() ?? []).filter(
      (produto) =>
        (!categoria || produto.categoriaSlug === categoria) &&
        (!termo || normalizarTexto(`${produto.nome} ${produto.descricao} ${produto.categoriaNome}`).includes(termo)),
    );

    return [false, true]
      .map((encomenda) => ({
        encomenda,
        grupos: agruparPorCategoria(filtrados.filter((produto) => !!produto.encomenda === encomenda)),
      }))
      .filter((bloco) => bloco.grupos.length > 0);
  });

  /** O título dos blocos só aparece quando há doces sob encomenda (senão é tudo pronta entrega). */
  readonly separarBlocos = computed(() => this.blocos().some((bloco) => bloco.encomenda));

  readonly totalEncontrado = computed(() =>
    this.blocos().reduce((soma, bloco) => soma + bloco.grupos.reduce((s, grupo) => s + grupo.produtos.length, 0), 0),
  );

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

/** Agrupa por categoria mantendo a ordem em que a API devolveu os produtos. */
function agruparPorCategoria(produtos: Produto[]): GrupoDoCardapio[] {
  const grupos = new Map<string, GrupoDoCardapio>();
  for (const produto of produtos) {
    const grupo = grupos.get(produto.categoriaSlug) ?? { slug: produto.categoriaSlug, nome: produto.categoriaNome, produtos: [] };
    grupo.produtos.push(produto);
    grupos.set(produto.categoriaSlug, grupo);
  }
  return [...grupos.values()];
}
