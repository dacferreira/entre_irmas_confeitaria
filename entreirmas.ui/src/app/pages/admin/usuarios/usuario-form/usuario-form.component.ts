import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { IconeComponent } from '@components/icone/icone.component';
import { Usuario } from '@models/admin.model';
import { UsuarioService } from '@services/admin.service';
import { NotificacaoService } from '@services/notificacao.service';
import { finalize } from 'rxjs';

const TAMANHO_MINIMO_DA_SENHA = 8;

@Component({
  selector: 'eirm-usuario-form',
  imports: [ReactiveFormsModule, MatFormFieldModule, MatInputModule, IconeComponent],
  templateUrl: './usuario-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UsuarioFormComponent {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly referencia = inject<MatDialogRef<UsuarioFormComponent, Usuario>>(MatDialogRef);
  private readonly usuarioService = inject(UsuarioService);
  private readonly notificacao = inject(NotificacaoService);

  /** Nulo para criar um usuário novo. */
  readonly usuario = inject<Usuario | null>(MAT_DIALOG_DATA);
  readonly tamanhoMinimo = TAMANHO_MINIMO_DA_SENHA;
  readonly salvando = signal(false);
  readonly senhaVisivel = signal(false);

  readonly formulario = this.fb.group({
    // O login não muda depois de criado.
    login: [
      { value: this.usuario?.login ?? '', disabled: !!this.usuario },
      [Validators.required, Validators.pattern(/^[a-zA-Z0-9._-]{3,30}$/)],
    ],
    nome: [this.usuario?.nome ?? '', [Validators.required, Validators.maxLength(100)]],
    senha: [
      '',
      this.usuario
        ? [Validators.minLength(TAMANHO_MINIMO_DA_SENHA), Validators.maxLength(128)]
        : [Validators.required, Validators.minLength(TAMANHO_MINIMO_DA_SENHA), Validators.maxLength(128)],
    ],
  });

  salvar(): void {
    if (this.formulario.invalid || this.salvando()) {
      this.formulario.markAllAsTouched();
      return;
    }

    const { login, nome, senha } = this.formulario.getRawValue();
    const dados = { login: login.trim(), nome: nome.trim(), senha: senha || null };
    const salvar$ = this.usuario
      ? this.usuarioService.atualizar(this.usuario.id, dados)
      : this.usuarioService.criar(dados);

    this.salvando.set(true);
    salvar$.pipe(finalize(() => this.salvando.set(false))).subscribe((usuario) => {
      this.notificacao.sucesso(this.usuario ? `${usuario.nome} atualizado.` : `${usuario.nome} já pode entrar.`);
      this.referencia.close(usuario);
    });
  }

  alternarSenha(): void {
    this.senhaVisivel.update((visivel) => !visivel);
  }

  cancelar(): void {
    this.referencia.close();
  }
}
