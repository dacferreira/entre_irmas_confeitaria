import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, ElementRef, computed, effect, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { Router } from '@angular/router';
import {
  CamposEnderecoComponent,
  criarFormularioEndereco,
  enderecoDoFormulario,
} from '@components/campos-endereco/campos-endereco.component';
import { IlustracaoComponent } from '@components/ilustracao/ilustracao.component';
import { PageComponent } from '@components/page/page.component';
import { ROTAS } from '@constants/rotas';
import { MascaraTelefoneDirective } from '@directives/mascara-telefone.directive';
import { environment } from '@environments/environment';
import { CotacaoEntrega, NovoPedido, TipoEntrega, nomeDoParceiro } from '@models/pedido.model';
import { ContatoService } from '@services/contato.service';
import { EntregaService } from '@services/entrega.service';
import { LojaService } from '@services/loja.service';
import { PedidoService } from '@services/pedido.service';
import { SacolaService } from '@services/sacola.service';
import { dataDaquiA, dataMinimaValidator, telefoneValidator } from '@validators/pedido.validators';
import { finalize } from 'rxjs';

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
    CamposEnderecoComponent,
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
  private readonly entregaService = inject(EntregaService);
  private readonly sanitizer = inject(DomSanitizer);

  readonly sacola = inject(SacolaService);
  readonly loja = inject(LojaService);
  readonly contato = inject(ContatoService);
  readonly antecedencia = environment.antecedenciaMinimaDias;

  /**
   * Com doce sob encomenda é encomenda: escolhe a data, com antecedência. Só com pronta entrega é pedido
   * normal: é para hoje, sem campo de data.
   */
  readonly encomenda = this.sacola.temEncomenda;
  readonly dataMinima = computed(() => dataDaquiA(this.encomenda() ? this.antecedencia : 0));

  /** Loja fechada: pedido normal não pode ser enviado (encomenda pode). */
  readonly bloqueado = computed(() => !this.loja.aberta() && !this.encomenda());
  readonly enviando = signal(false);

  // Entrega por Uber/99: a pessoa clica em "Calcular entrega"; mudar o endereço apaga o cálculo.
  readonly nomeDoParceiro = nomeDoParceiro;
  readonly cotacao = signal<CotacaoEntrega | null>(null);
  readonly calculandoEntrega = signal(false);
  readonly faltaCalcular = signal(false);
  readonly urlTrajeto = signal<SafeResourceUrl | null>(null);

  readonly formulario = this.fb.group({
    nomeCliente: ['', [Validators.required, Validators.maxLength(100)]],
    telefone: ['', [Validators.required, telefoneValidator]],
    tipoEntrega: this.fb.control<TipoEntrega>('Retirada'),
    // Começa desligado (retirada): grupo desligado não conta na validação do formulário.
    endereco: criarFormularioEndereco(this.fb),
    dataDesejada: [this.dataMinima(), [Validators.required, dataMinimaValidator(this.dataMinima())]],
    observacao: ['', [Validators.maxLength(500)]],
  });

  readonly tipoEntrega = toSignal(this.formulario.controls.tipoEntrega.valueChanges, {
    initialValue: this.formulario.controls.tipoEntrega.value,
  });

  /** Taxa calculada (nula na retirada, antes de calcular e quando é combinada pelo WhatsApp). */
  readonly taxaEntrega = computed(() => {
    const cotacao = this.cotacao();
    return this.tipoEntrega() === 'Entrega' && cotacao?.disponivel ? (cotacao.valor ?? 0) : null;
  });

  /** O que o cliente paga: sacola + entrega. A API confere tudo de novo no envio. */
  readonly total = computed(() => this.sacola.total() + (this.taxaEntrega() ?? 0));

  constructor() {
    // Se a sacola mudar (ex.: entra um doce sob encomenda), o campo de data e a validação acompanham.
    effect(() => {
      const minima = this.dataMinima();
      const data = this.formulario.controls.dataDesejada;
      if (!this.encomenda()) {
        // Pedido normal: hoje, sem campo (desligado não conta na validação).
        data.setValue(minima);
        data.disable();
        return;
      }
      data.enable();
      data.setValidators([Validators.required, dataMinimaValidator(minima)]);
      if (!data.value || data.value < minima) {
        data.setValue(minima);
      }
      data.updateValueAndValidity();
    });

    // Confere se a loja abriu ou fechou desde que o site foi carregado.
    this.loja.atualizar();

    // O endereço só vale (e só é validado) na entrega.
    this.formulario.controls.endereco.disable();
    this.formulario.controls.tipoEntrega.valueChanges.pipe(takeUntilDestroyed()).subscribe((tipo) => {
      const endereco = this.formulario.controls.endereco;
      if (tipo === 'Entrega') {
        endereco.enable();
      } else {
        endereco.disable();
      }
    });

    this.formulario.controls.endereco.valueChanges.pipe(takeUntilDestroyed()).subscribe(() => this.descartarCalculo());
  }

  calcularEntrega(): void {
    const endereco = this.formulario.controls.endereco;
    if (endereco.invalid) {
      endereco.markAllAsTouched();
      this.focarPrimeiroErro();
      return;
    }
    if (this.calculandoEntrega()) {
      return;
    }

    const destino = enderecoDoFormulario(endereco);
    this.calculandoEntrega.set(true);
    this.entregaService
      .cotar(destino)
      .pipe(finalize(() => this.calculandoEntrega.set(false)))
      .subscribe({
        next: (cotacao) => {
          this.cotacao.set(cotacao);
          this.faltaCalcular.set(false);
          // A URL é montada pelo serviço, com o endereço do formulário, só para o mapa do Google.
          this.urlTrajeto.set(this.sanitizer.bypassSecurityTrustResourceUrl(this.entregaService.urlDoTrajeto(destino)));
        },
        // A mensagem de erro é exibida pelo interceptor.
        error: () => undefined,
      });
  }

  enviar(): void {
    if (this.formulario.invalid) {
      this.formulario.markAllAsTouched();
      this.focarPrimeiroErro();
      return;
    }
    if (this.sacola.vazia() || this.enviando() || this.bloqueado()) {
      return;
    }
    if (this.tipoEntrega() === 'Entrega' && !this.cotacao()) {
      this.faltaCalcular.set(true);
      this.elemento.nativeElement.querySelector<HTMLElement>('.entrega__calcular')?.focus();
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
        // A loja pode ter fechado enquanto a pessoa preenchia: atualiza o aviso.
        error: () => this.loja.atualizar(),
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
      endereco: valores.tipoEntrega === 'Entrega' ? enderecoDoFormulario(this.formulario.controls.endereco) : null,
      dataDesejada: valores.dataDesejada,
      observacao: valores.observacao.trim() || null,
      itens: this.sacola.itens().map((item) => ({ produtoId: item.produto.id, quantidade: item.quantidade })),
    };
  }

  private descartarCalculo(): void {
    if (this.cotacao() || this.urlTrajeto()) {
      this.cotacao.set(null);
      this.urlTrajeto.set(null);
    }
  }

  private focarPrimeiroErro(): void {
    this.elemento.nativeElement.querySelector<HTMLElement>('.ng-invalid[formControlName]')?.focus();
  }
}
