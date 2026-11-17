import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { CardProdutoComponent } from '@components/card-produto/card-produto.component';
import { IlustracaoComponent } from '@components/ilustracao/ilustracao.component';
import { MapaLojaComponent } from '@components/mapa-loja/mapa-loja.component';
import { Ilustracao, normalizarIlustracao } from '@constants/ilustracoes';
import { ROTAS } from '@constants/rotas';
import { Categoria } from '@models/categoria.model';
import { Produto } from '@models/produto.model';
import { CatalogoService } from '@services/catalogo.service';
import { ContatoService } from '@services/contato.service';
import { catchError, of } from 'rxjs';
import { ComoFuncionaComponent } from './como-funciona/como-funciona.component';
import { HeroComponent } from './hero/hero.component';

const MAXIMO_DE_DESTAQUES = 6;
const FAIXA_PADRAO = ['Bolos recheados', 'Brownies', 'Brigadeiros', 'Cupcakes', 'Fudge', 'Salgados'];

export interface VitrineCategoria {
  categoria: Categoria;
  ilustracao: Ilustracao;
}

@Component({
  selector: 'eirm-inicio',
  imports: [RouterLink, CardProdutoComponent, IlustracaoComponent, HeroComponent, ComoFuncionaComponent, MapaLojaComponent],
  templateUrl: './inicio.component.html',
  styleUrl: './inicio.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class InicioComponent {
  private readonly catalogo = inject(CatalogoService);
  readonly contato = inject(ContatoService);
  readonly rotas = ROTAS;
  readonly esqueletos = [1, 2, 3];

  /** undefined = carregando; null = falhou. */
  readonly produtos = toSignal<Produto[] | null | undefined>(
    this.catalogo.listarProdutos().pipe(catchError(() => of(null))),
    { initialValue: undefined },
  );

  readonly categorias = toSignal(this.catalogo.listarCategorias().pipe(catchError(() => of([] as Categoria[]))), {
    initialValue: [] as Categoria[],
  });

  readonly carregando = computed(() => this.produtos() === undefined);
  readonly falhou = computed(() => this.produtos() === null);
  readonly destaques = computed(() => (this.produtos() ?? []).filter((p) => p.destaque).slice(0, MAXIMO_DE_DESTAQUES));

  /** Cada categoria ganha a ilustração do seu primeiro produto. */
  readonly vitrine = computed<VitrineCategoria[]>(() => {
    const produtos = this.produtos() ?? [];
    return this.categorias().map((categoria) => ({
      categoria,
      ilustracao: normalizarIlustracao(produtos.find((p) => p.categoriaSlug === categoria.slug)?.ilustracao),
    }));
  });

  /** Palavras da faixa animada (o conteúdo é duplicado para o loop não ter emenda). */
  readonly faixa = computed(() => {
    const nomes = (this.produtos() ?? []).map((p) => p.nome).slice(0, 8);
    const palavras = nomes.length > 0 ? nomes : FAIXA_PADRAO;
    return [...palavras, ...palavras];
  });
}
