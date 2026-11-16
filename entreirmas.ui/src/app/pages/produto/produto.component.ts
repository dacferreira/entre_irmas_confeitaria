import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, effect, inject, input, linkedSignal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { Title } from '@angular/platform-browser';
import { RouterLink } from '@angular/router';
import { CardProdutoComponent } from '@components/card-produto/card-produto.component';
import { IlustracaoComponent } from '@components/ilustracao/ilustracao.component';
import { SeletorQuantidadeComponent } from '@components/seletor-quantidade/seletor-quantidade.component';
import { normalizarIlustracao } from '@constants/ilustracoes';
import { environment } from '@environments/environment';
import { ROTAS } from '@constants/rotas';
import { Produto } from '@models/produto.model';
import { UrlImagemPipe } from '@pipes/url-imagem.pipe';
import { CatalogoService } from '@services/catalogo.service';
import { NotificacaoService } from '@services/notificacao.service';
import { SacolaService } from '@services/sacola.service';
import { catchError, map, of, startWith, switchMap } from 'rxjs';

const MAXIMO_DE_RELACIONADOS = 3;

export interface EstadoProduto {
  situacao: 'carregando' | 'pronto' | 'erro';
  produto: Produto | null;
}

@Component({
  selector: 'eirm-produto',
  imports: [
    CurrencyPipe,
    RouterLink,
    CardProdutoComponent,
    IlustracaoComponent,
    SeletorQuantidadeComponent,
    UrlImagemPipe,
  ],
  templateUrl: './produto.component.html',
  styleUrl: './produto.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProdutoComponent {
  readonly antecedencia = environment.antecedenciaMinimaDias;
  private readonly catalogo = inject(CatalogoService);
  private readonly sacola = inject(SacolaService);
  private readonly notificacao = inject(NotificacaoService);
  private readonly titulo = inject(Title);

  /** Parâmetro :slug da rota. */
  readonly slug = input.required<string>();
  readonly rotas = ROTAS;

  readonly estado = toSignal(
    toObservable(this.slug).pipe(
      switchMap((slug) =>
        this.catalogo.obterProduto(slug).pipe(
          map((produto): EstadoProduto => ({ situacao: 'pronto', produto })),
          catchError(() => of<EstadoProduto>({ situacao: 'erro', produto: null })),
          startWith<EstadoProduto>({ situacao: 'carregando', produto: null }),
        ),
      ),
    ),
    { initialValue: { situacao: 'carregando', produto: null } as EstadoProduto },
  );

  readonly produto = computed(() => this.estado().produto);
  readonly sabor = computed(() => normalizarIlustracao(this.produto()?.ilustracao));

  /** Volta para o pedido mínimo sempre que o produto muda. */
  readonly quantidade = linkedSignal(() => this.produto()?.quantidadeMinima ?? 1);
  readonly subtotal = computed(() => (this.produto()?.preco ?? 0) * this.quantidade());
  readonly naSacola = computed(() => {
    const produto = this.produto();
    return produto ? this.sacola.quantidadeDe(produto.id) : 0;
  });

  readonly relacionados = toSignal(
    toObservable(this.produto).pipe(
      switchMap((produto) =>
        produto
          ? this.catalogo.listarProdutos({ categoria: produto.categoriaSlug }).pipe(
              map((lista) => lista.filter((p) => p.id !== produto.id).slice(0, MAXIMO_DE_RELACIONADOS)),
              catchError(() => of([] as Produto[])),
            )
          : of([] as Produto[]),
      ),
    ),
    { initialValue: [] as Produto[] },
  );

  constructor() {
    effect(() => {
      const produto = this.produto();
      if (produto) {
        this.titulo.setTitle(`${produto.nome} · Entre Irmãs Confeitaria`);
      }
    });
  }

  adicionar(): void {
    const produto = this.produto();
    if (!produto) {
      return;
    }
    this.sacola.adicionar(produto, this.quantidade());
    this.notificacao
      .sucesso(`${this.quantidade()}× ${produto.nome} na sacola!`, 'Ver sacola')
      .subscribe(() => this.sacola.abrir());
  }
}
