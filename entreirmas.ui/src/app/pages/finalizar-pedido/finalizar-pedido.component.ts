import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, ElementRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Router } from '@angular/router';
import { IlustracaoComponent } from '@components/ilustracao/ilustracao.component';
import { PageComponent } from '@components/page/page.component';
import { ROTAS } from '@constants/rotas';
import { MascaraTelefoneDirective } from '@directives/mascara-telefone.directive';
import { environment } from '@environments/environment';
import { NovoPedido, TipoEntrega } from '@models/pedido.model';
import { PedidoService } from '@services/pedido.service';
import { SacolaService } from '@services/sacola.service';
import { dataDaquiA, dataMinimaValidator, telefoneValidator } from '@validators/pedido.validators';
import { finalize } from 'rxjs';

const VALIDADORES_ENDERECO_OPCIONAL = [Validators.maxLength(300)];
const VALIDADORES_ENDERECO_OBRIGATORIO = [Validators.required, Validators.maxLength(300)];

@Component({
  selector: 'eirm-finalizar-pedido',
  imports: [
    CurrencyPipe,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    PageComponent,
    IlustracaoComponent,
    MascaraTelefoneDirective,
  ],
  templateUrl: './finalizar-pedido.component.html',
  styleUrl: './finalizar-pedido.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FinalizarPedidoComponent {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly pedidoService = inject(PedidoService);
  private readonly router = inject(Router);
  private readonly elemento = inject<ElementRef<HTMLElement>>(ElementRef);

  readonly sacola = inject(SacolaService);
  readonly dataMinima = dataDaquiA(environment.antecedenciaMinimaDias);
  readonly antecedencia = environment.antecedenciaMinimaDias;
  readonly enviando = signal(false);

  readonly formulario = this.fb.group({
    nomeCliente: ['', [Validators.required, Validators.maxLength(100)]],
    telefone: ['', [Validators.required, telefoneValidator]],
    tipoEntrega: this.fb.control<TipoEntrega>('Retirada'),
    endereco: ['', VALIDADORES_ENDERECO_OPCIONAL],
    dataDesejada: [this.dataMinima, [Validators.required, dataMinimaValidator(this.dataMinima)]],
    observacao: ['', [Validators.maxLength(500)]],
  });

  readonly tipoEntrega = toSignal(this.formulario.controls.tipoEntrega.valueChanges, {
    initialValue: this.formulario.controls.tipoEntrega.value,
  });

  constructor() {
    // O endereço só é obrigatório para entrega.
    this.formulario.controls.tipoEntrega.valueChanges.pipe(takeUntilDestroyed()).subscribe((tipo) => {
      const endereco = this.formulario.controls.endereco;
      endereco.setValidators(tipo === 'Entrega' ? VALIDADORES_ENDERECO_OBRIGATORIO : VALIDADORES_ENDERECO_OPCIONAL);
      endereco.updateValueAndValidity();
    });
  }

  enviar(): void {
    if (this.formulario.invalid) {
      this.formulario.markAllAsTouched();
      this.focarPrimeiroErro();
      return;
    }
    if (this.sacola.vazia() || this.enviando()) {
      return;
    }

    this.enviando.set(true);
    this.pedidoService
      .criar(this.montarPedido())
      .pipe(finalize(() => this.enviando.set(false)))
      .subscribe({
        next: () => {
          this.sacola.limpar();
          void this.router.navigate(['/', ROTAS.PEDIDO_CONFIRMADO]);
        },
        // A mensagem de erro é exibida pelo interceptor; a sacola é mantida para tentar de novo.
        error: () => undefined,
      });
  }

  editarSacola(): void {
    this.sacola.abrir();
  }

  private montarPedido(): NovoPedido {
    const valores = this.formulario.getRawValue();
    return {
      nomeCliente: valores.nomeCliente.trim(),
      telefone: valores.telefone.replace(/\D/g, ''),
      tipoEntrega: valores.tipoEntrega,
      endereco: valores.tipoEntrega === 'Entrega' ? valores.endereco.trim() : null,
      dataDesejada: valores.dataDesejada,
      observacao: valores.observacao.trim() || null,
      itens: this.sacola.itens().map((item) => ({ produtoId: item.produto.id, quantidade: item.quantidade })),
    };
  }

  private focarPrimeiroErro(): void {
    this.elemento.nativeElement.querySelector<HTMLElement>('.ng-invalid[formControlName]')?.focus();
  }
}
