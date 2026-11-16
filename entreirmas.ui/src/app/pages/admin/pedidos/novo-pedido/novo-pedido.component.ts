import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import {
  CamposEnderecoComponent,
  criarFormularioEndereco,
  enderecoDoFormulario,
} from '@components/campos-endereco/campos-endereco.component';
import { IconeComponent } from '@components/icone/icone.component';
import { dataIso } from '@constants/admin';
import { MascaraTelefoneDirective } from '@directives/mascara-telefone.directive';
import { PedidoDetalhe, ProdutoAdmin } from '@models/admin.model';
import { TipoEntrega } from '@models/pedido.model';
import { PedidoAdminService, ProdutoAdminService } from '@services/admin.service';
import { NotificacaoService } from '@services/notificacao.service';
import { telefoneValidator } from '@validators/pedido.validators';
import { finalize, map, startWith } from 'rxjs';

interface ItemDoFormulario {
  produtoId: FormControl<number | null>;
  quantidade: FormControl<number>;
}

/** Lança um pedido combinado fora do site. Fecha com o pedido criado. */
@Component({
  selector: 'eirm-novo-pedido',
  imports: [
    CurrencyPipe,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    IconeComponent,
    MascaraTelefoneDirective,
    CamposEnderecoComponent,
  ],
  templateUrl: './novo-pedido.component.html',
  styleUrl: './novo-pedido.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NovoPedidoComponent {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly referencia = inject<MatDialogRef<NovoPedidoComponent, PedidoDetalhe>>(MatDialogRef);
  private readonly pedidoService = inject(PedidoAdminService);
  private readonly notificacao = inject(NotificacaoService);

  readonly produtos = toSignal(inject(ProdutoAdminService).listar({ ativo: true }), { initialValue: [] });
  readonly salvando = signal(false);

  readonly formulario = this.fb.group({
    nomeCliente: ['', [Validators.required, Validators.maxLength(100)]],
    telefone: ['', [Validators.required, telefoneValidator]],
    tipoEntrega: this.fb.control<TipoEntrega>('Retirada'),
    // Começa desligado (retirada): grupo desligado não conta na validação do formulário.
    endereco: criarFormularioEndereco(this.fb),
    dataDesejada: [dataIso(new Date()), [Validators.required]],
    observacao: ['', [Validators.maxLength(500)]],
    itens: this.fb.array([this.novoItem()]),
  });

  readonly tipoEntrega = toSignal(this.formulario.controls.tipoEntrega.valueChanges, {
    initialValue: this.formulario.controls.tipoEntrega.value,
  });

  private readonly itens = toSignal(
    this.formulario.controls.itens.valueChanges.pipe(
      startWith(null),
      map(() => this.formulario.controls.itens.getRawValue()),
    ),
    { requireSync: true },
  );

  readonly total = computed(() =>
    this.itens().reduce((soma, item) => soma + (this.produto(item.produtoId)?.preco ?? 0) * (item.quantidade || 0), 0),
  );

  constructor() {
    this.formulario.controls.endereco.disable();
    this.formulario.controls.tipoEntrega.valueChanges.pipe(takeUntilDestroyed()).subscribe((tipo) => {
      const endereco = this.formulario.controls.endereco;
      if (tipo === 'Entrega') {
        endereco.enable();
      } else {
        endereco.disable();
      }
    });
  }

  produto(id: number | null): ProdutoAdmin | undefined {
    return this.produtos().find((p) => p.id === id);
  }

  adicionarItem(): void {
    this.formulario.controls.itens.push(this.novoItem());
  }

  removerItem(indice: number): void {
    if (this.formulario.controls.itens.length > 1) {
      this.formulario.controls.itens.removeAt(indice);
    }
  }

  /** Ao escolher o produto, sugere a quantidade mínima dele. */
  aoEscolherProduto(indice: number): void {
    const item = this.formulario.controls.itens.at(indice);
    const produto = this.produto(item.controls.produtoId.value);
    if (produto && item.controls.quantidade.value < produto.quantidadeMinima) {
      item.controls.quantidade.setValue(produto.quantidadeMinima);
    }
  }

  salvar(): void {
    const entrega = this.formulario.controls.tipoEntrega.value === 'Entrega';

    if (this.formulario.invalid || this.salvando()) {
      this.formulario.markAllAsTouched();
      return;
    }

    const dados = this.formulario.getRawValue();
    this.salvando.set(true);
    this.pedidoService
      .criar({
        nomeCliente: dados.nomeCliente.trim(),
        telefone: dados.telefone,
        tipoEntrega: dados.tipoEntrega,
        endereco: entrega ? enderecoDoFormulario(this.formulario.controls.endereco) : null,
        dataDesejada: dados.dataDesejada,
        observacao: dados.observacao.trim() || null,
        itens: dados.itens.map((item) => ({ produtoId: item.produtoId ?? 0, quantidade: item.quantidade })),
      })
      .pipe(finalize(() => this.salvando.set(false)))
      .subscribe((pedido) => {
        this.notificacao.sucesso(`Pedido ${pedido.codigo} lançado como confirmado.`);
        this.referencia.close(pedido);
      });
  }

  cancelar(): void {
    this.referencia.close();
  }

  private novoItem(): FormGroup<ItemDoFormulario> {
    return this.fb.group({
      produtoId: this.fb.control<number | null>(null, Validators.required),
      quantidade: [1, [Validators.required, Validators.min(1), Validators.max(1000)]],
    });
  }
}
