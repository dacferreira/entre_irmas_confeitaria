import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { IconeComponent } from '@components/icone/icone.component';
import { TIPOS_LANCAMENTO, dataIso } from '@constants/admin';
import { Lancamento, TipoLancamento } from '@models/admin.model';
import { LancamentoService } from '@services/admin.service';
import { NotificacaoService } from '@services/notificacao.service';
import { finalize } from 'rxjs';

@Component({
  selector: 'eirm-lancamento-form',
  imports: [ReactiveFormsModule, MatFormFieldModule, MatInputModule, IconeComponent],
  templateUrl: './lancamento-form.component.html',
  styleUrl: './lancamento-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LancamentoFormComponent {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly referencia = inject<MatDialogRef<LancamentoFormComponent, Lancamento>>(MatDialogRef);
  private readonly lancamentoService = inject(LancamentoService);
  private readonly notificacao = inject(NotificacaoService);

  /** Nulo para lançar um novo. */
  readonly lancamento = inject<Lancamento | null>(MAT_DIALOG_DATA);
  readonly tipos = TIPOS_LANCAMENTO;
  readonly salvando = signal(false);

  readonly formulario = this.fb.group({
    tipo: this.fb.control<TipoLancamento>(this.lancamento?.tipo ?? 'Despesa'),
    descricao: [this.lancamento?.descricao ?? '', [Validators.required, Validators.maxLength(200)]],
    valor: [this.lancamento?.valor ?? 0, [Validators.required, Validators.min(0.01)]],
    data: [this.lancamento?.data ?? dataIso(new Date()), [Validators.required]],
  });

  salvar(): void {
    if (this.formulario.invalid || this.salvando()) {
      this.formulario.markAllAsTouched();
      return;
    }

    const valores = this.formulario.getRawValue();
    const dados = { ...valores, descricao: valores.descricao.trim() };
    const salvar$ = this.lancamento
      ? this.lancamentoService.atualizar(this.lancamento.id, dados)
      : this.lancamentoService.criar(dados);

    this.salvando.set(true);
    salvar$.pipe(finalize(() => this.salvando.set(false))).subscribe((salvo) => {
      this.notificacao.sucesso(`${salvo.tipo} ${this.lancamento ? 'atualizada' : 'lançada'}.`);
      this.referencia.close(salvo);
    });
  }

  cancelar(): void {
    this.referencia.close();
  }
}
