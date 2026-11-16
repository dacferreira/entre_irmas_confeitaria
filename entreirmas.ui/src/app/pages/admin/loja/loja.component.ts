import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { LojaAdmin, TAMANHO_MAXIMO_MENSAGEM_LOJA } from '@models/loja.model';
import { LojaService } from '@services/loja.service';
import { NotificacaoService } from '@services/notificacao.service';
import { finalize } from 'rxjs';
import { ImpressoraRecibosComponent } from './impressora-recibos/impressora-recibos.component';

/** Abre e fecha a loja no site. Fechada, o site mostra o aviso e só aceita encomendas. */
@Component({
  selector: 'eirm-loja',
  imports: [DatePipe, ReactiveFormsModule, MatFormFieldModule, MatInputModule, ImpressoraRecibosComponent],
  templateUrl: './loja.component.html',
  styleUrl: './loja.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LojaComponent {
  private readonly lojaService = inject(LojaService);
  private readonly notificacao = inject(NotificacaoService);

  readonly tamanhoMaximo = TAMANHO_MAXIMO_MENSAGEM_LOJA;
  readonly loja = signal<LojaAdmin | null>(null);
  readonly carregando = signal(true);
  readonly salvando = signal(false);
  readonly mensagem = new FormControl('', {
    nonNullable: true,
    validators: [Validators.maxLength(TAMANHO_MAXIMO_MENSAGEM_LOJA)],
  });

  constructor() {
    this.lojaService
      .obterAdmin()
      .pipe(finalize(() => this.carregando.set(false)))
      .subscribe({ next: (loja) => this.exibir(loja), error: () => undefined });
  }

  abrir(): void {
    this.salvar(true);
  }

  /** Fecha a loja ou, se já estiver fechada, troca o aviso. */
  fechar(): void {
    this.salvar(false);
  }

  private salvar(aberta: boolean): void {
    if (this.salvando() || (!aberta && this.mensagem.invalid)) {
      this.mensagem.markAsTouched();
      return;
    }

    this.salvando.set(true);
    this.lojaService
      .alterar({ aberta, mensagem: aberta ? null : this.mensagem.value.trim() || null })
      .pipe(finalize(() => this.salvando.set(false)))
      .subscribe({
        next: (loja) => {
          const jaEstavaFechada = !aberta && this.loja()?.aberta === false;
          this.exibir(loja);
          this.notificacao.sucesso(
            aberta ? 'Loja aberta: o site voltou a aceitar pedidos.' : jaEstavaFechada ? 'Aviso atualizado.' : 'Loja fechada: o site só aceita encomendas.',
          );
        },
        // O erro aparece pelo aviso geral.
        error: () => undefined,
      });
  }

  private exibir(loja: LojaAdmin): void {
    this.loja.set(loja);
    this.mensagem.setValue(loja.mensagem ?? '');
  }
}
