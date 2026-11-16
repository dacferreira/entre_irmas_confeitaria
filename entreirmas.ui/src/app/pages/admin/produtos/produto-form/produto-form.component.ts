import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { IconeComponent } from '@components/icone/icone.component';
import { IlustracaoComponent } from '@components/ilustracao/ilustracao.component';
import { ILUSTRACOES } from '@constants/ilustracoes';
import { environment } from '@environments/environment';
import { CategoriaAdmin, ProdutoAdmin, SalvarProduto } from '@models/admin.model';
import { UrlImagemPipe } from '@pipes/url-imagem.pipe';
import { ProdutoAdminService } from '@services/admin.service';
import { ImagemService } from '@services/imagem.service';
import { NotificacaoService } from '@services/notificacao.service';
import { Observable, catchError, finalize, map, of, startWith, switchMap } from 'rxjs';

export interface DadosProdutoForm {
  /** Nulo para cadastrar um produto novo. */
  produto: ProdutoAdmin | null;
  categorias: CategoriaAdmin[];
}

interface FotoEscolhida {
  arquivo: Blob;
  /** URL local (blob:) para a prévia, liberada ao trocar ou fechar. */
  previa: string;
}

@Component({
  selector: 'eirm-produto-form',
  imports: [
    CurrencyPipe,
    DecimalPipe,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    IconeComponent,
    IlustracaoComponent,
    UrlImagemPipe,
  ],
  templateUrl: './produto-form.component.html',
  styleUrl: './produto-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProdutoFormComponent {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly referencia = inject<MatDialogRef<ProdutoFormComponent, ProdutoAdmin>>(MatDialogRef);
  private readonly produtoService = inject(ProdutoAdminService);
  private readonly imagemService = inject(ImagemService);
  private readonly notificacao = inject(NotificacaoService);

  readonly dados = inject<DadosProdutoForm>(MAT_DIALOG_DATA);
  readonly ilustracoes = ILUSTRACOES;
  readonly salvando = signal(false);
  readonly edicao = !!this.dados.produto;
  readonly antecedencia = environment.antecedenciaMinimaDias;

  // Foto: a escolhida só é enviada ao salvar, depois dos dados do produto (o produto novo ainda não tem id).
  private readonly fotoAtual = this.dados.produto?.urlImagem ?? null;
  readonly fotoNova = signal<FotoEscolhida | null>(null);
  readonly removerFoto = signal(false);
  readonly preparandoFoto = signal(false);
  readonly erroFoto = signal('');

  /** O que a prévia mostra: a foto escolhida agora, a já salva ou nada (ilustração). */
  readonly previa = computed(() => this.fotoNova()?.previa ?? (this.removerFoto() ? null : this.fotoAtual));

  readonly formulario = this.fb.group({
    nome: [this.dados.produto?.nome ?? '', [Validators.required, Validators.maxLength(100)]],
    categoriaId: this.fb.control<number | null>(
      this.dados.produto?.categoriaId ?? this.dados.categorias[0]?.id ?? null,
      Validators.required,
    ),
    descricao: [this.dados.produto?.descricao ?? '', [Validators.required, Validators.maxLength(1000)]],
    preco: [this.dados.produto?.preco ?? 0, [Validators.required, Validators.min(0.01)]],
    custo: [this.dados.produto?.custo ?? 0, [Validators.required, Validators.min(0)]],
    unidade: [this.dados.produto?.unidade ?? 'unidade', [Validators.required, Validators.maxLength(40)]],
    quantidadeMinima: [
      this.dados.produto?.quantidadeMinima ?? 1,
      [Validators.required, Validators.min(1), Validators.max(1000)],
    ],
    ilustracao: [this.dados.produto?.ilustracao ?? 'cupcake', Validators.required],
    destaque: [this.dados.produto?.destaque ?? false],
    encomenda: [this.dados.produto?.encomenda ?? false],
  });

  private readonly valores = toSignal(
    this.formulario.valueChanges.pipe(
      startWith(null),
      map(() => this.formulario.getRawValue()),
    ),
    { requireSync: true },
  );

  /** Lucro e margem por unidade de venda, atualizados enquanto digita. */
  readonly lucroUnitario = computed(() => (this.valores().preco || 0) - (this.valores().custo || 0));
  readonly margem = computed(() => {
    const preco = this.valores().preco || 0;
    return preco > 0 ? (this.lucroUnitario() / preco) * 100 : 0;
  });
  readonly ilustracaoEscolhida = computed(() => this.valores().ilustracao);

  constructor() {
    inject(DestroyRef).onDestroy(() => this.liberarPrevia());
  }

  async escolherFoto(evento: Event): Promise<void> {
    const campo = evento.target as HTMLInputElement;
    const arquivo = campo.files?.[0];
    campo.value = ''; // permite escolher o mesmo arquivo de novo
    if (!arquivo) {
      return;
    }

    this.erroFoto.set('');
    this.preparandoFoto.set(true);
    try {
      const reduzida = await this.imagemService.reduzir(arquivo);
      this.liberarPrevia();
      this.fotoNova.set({ arquivo: reduzida, previa: URL.createObjectURL(reduzida) });
      this.removerFoto.set(false);
    } catch (erro) {
      this.erroFoto.set(erro instanceof Error ? erro.message : 'Não foi possível usar esta foto.');
    } finally {
      this.preparandoFoto.set(false);
    }
  }

  /** Desfaz a foto escolhida agora ou marca a foto salva para ser apagada ao salvar. */
  tirarFoto(): void {
    if (this.fotoNova()) {
      this.liberarPrevia();
      this.fotoNova.set(null);
      return;
    }
    this.removerFoto.set(true);
  }

  salvar(): void {
    if (this.formulario.invalid || this.salvando() || this.preparandoFoto()) {
      this.formulario.markAllAsTouched();
      return;
    }

    const valores = this.formulario.getRawValue();
    const dados: SalvarProduto = {
      ...valores,
      categoriaId: valores.categoriaId ?? 0,
      nome: valores.nome.trim(),
      descricao: valores.descricao.trim(),
      unidade: valores.unidade.trim(),
    };
    const salvar$ = this.dados.produto
      ? this.produtoService.atualizar(this.dados.produto.id, dados)
      : this.produtoService.criar(dados);

    this.salvando.set(true);
    salvar$
      .pipe(
        switchMap((produto) => this.salvarFoto(produto)),
        finalize(() => this.salvando.set(false)),
      )
      .subscribe((produto) => {
        this.notificacao.sucesso(this.edicao ? `${produto.nome} atualizado.` : `${produto.nome} cadastrado.`);
        this.referencia.close(produto);
      });
  }

  cancelar(): void {
    this.referencia.close();
  }

  // Os dados já foram salvos: se a foto falhar, o produto fica salvo sem ela (e a pessoa é avisada).
  private salvarFoto(produto: ProdutoAdmin): Observable<ProdutoAdmin> {
    const nova = this.fotoNova();
    const envio$ = nova
      ? this.produtoService.enviarImagem(produto.id, nova.arquivo)
      : this.removerFoto() && produto.urlImagem
        ? this.produtoService.removerImagem(produto.id)
        : null;

    return envio$
      ? envio$.pipe(
          catchError(() => {
            this.notificacao.erro(`${produto.nome} foi salvo, mas a foto não. Edite o produto para tentar de novo.`);
            this.referencia.close(produto);
            return of();
          }),
        )
      : of(produto);
  }

  private liberarPrevia(): void {
    const atual = this.fotoNova();
    if (atual) {
      URL.revokeObjectURL(atual.previa);
    }
  }
}
