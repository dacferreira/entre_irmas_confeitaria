import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { IconeComponent } from '@components/icone/icone.component';
import { CategoriaAdmin } from '@models/admin.model';
import { CategoriaAdminService } from '@services/admin.service';
import { NotificacaoService } from '@services/notificacao.service';
import { finalize } from 'rxjs';

export interface DadosCategoriaForm {
  /** Nula para criar uma categoria nova. */
  categoria: CategoriaAdmin | null;
  proximaOrdem: number;
}

@Component({
  selector: 'eirm-categoria-form',
  imports: [ReactiveFormsModule, MatFormFieldModule, MatInputModule, IconeComponent],
  templateUrl: './categoria-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CategoriaFormComponent {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly referencia = inject<MatDialogRef<CategoriaFormComponent, CategoriaAdmin>>(MatDialogRef);
  private readonly categoriaService = inject(CategoriaAdminService);
  private readonly notificacao = inject(NotificacaoService);

  readonly dados = inject<DadosCategoriaForm>(MAT_DIALOG_DATA);
  readonly edicao = !!this.dados.categoria;
  readonly salvando = signal(false);

  readonly formulario = this.fb.group({
    nome: [this.dados.categoria?.nome ?? '', [Validators.required, Validators.maxLength(100)]],
    descricao: [this.dados.categoria?.descricao ?? '', [Validators.maxLength(300)]],
    ordem: [this.dados.categoria?.ordem ?? this.dados.proximaOrdem, [Validators.required, Validators.min(0), Validators.max(999)]],
  });

  salvar(): void {
    if (this.formulario.invalid || this.salvando()) {
      this.formulario.markAllAsTouched();
      return;
    }

    const { nome, descricao, ordem } = this.formulario.getRawValue();
    const dados = { nome: nome.trim(), descricao: descricao.trim() || null, ordem };
    const salvar$ = this.dados.categoria
      ? this.categoriaService.atualizar(this.dados.categoria.id, dados)
      : this.categoriaService.criar(dados);

    this.salvando.set(true);
    salvar$.pipe(finalize(() => this.salvando.set(false))).subscribe((categoria) => {
      this.notificacao.sucesso(this.edicao ? `${categoria.nome} atualizada.` : `${categoria.nome} criada.`);
      this.referencia.close(categoria);
    });
  }

  cancelar(): void {
    this.referencia.close();
  }
}
