import { ChangeDetectionStrategy, Component, inject, input, signal, viewChild } from '@angular/core';
import { FormGroupDirective, NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Router, RouterLink } from '@angular/router';
import { IconeComponent } from '@components/icone/icone.component';
import { IlustracaoComponent } from '@components/ilustracao/ilustracao.component';
import { ROTAS_ADMIN } from '@constants/admin';
import { AuthService } from '@services/auth.service';
import { finalize } from 'rxjs';

@Component({
  selector: 'eirm-login',
  imports: [ReactiveFormsModule, MatFormFieldModule, MatInputModule, RouterLink, IconeComponent, IlustracaoComponent],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  /** Tela que a pessoa tentou abrir antes do login (?retorno=). */
  readonly retorno = input<string>('');

  readonly entrando = signal(false);
  readonly senhaVisivel = signal(false);
  private readonly diretivaDoFormulario = viewChild(FormGroupDirective);

  readonly formulario = this.fb.group({
    login: ['', [Validators.required, Validators.maxLength(30)]],
    senha: ['', [Validators.required, Validators.maxLength(128)]],
  });

  entrar(): void {
    if (this.formulario.invalid || this.entrando()) {
      this.formulario.markAllAsTouched();
      return;
    }

    const { login, senha } = this.formulario.getRawValue();
    this.entrando.set(true);
    this.auth
      .entrar(login.trim(), senha)
      .pipe(finalize(() => this.entrando.set(false)))
      .subscribe({
        next: () => void this.router.navigateByUrl(this.destino()),
        // A mensagem do erro aparece pelo erroInterceptor. Limpa só a senha e desfaz o "enviado",
        // para o campo vazio não aparecer em vermelho ao lado do aviso.
        error: () => this.diretivaDoFormulario()?.resetForm({ login, senha: '' }),
      });
  }

  alternarSenha(): void {
    this.senhaVisivel.update((visivel) => !visivel);
  }

  // Só aceita retorno para dentro da área administrativa (evita redirecionar para outro site).
  private destino(): string {
    const retorno = this.retorno();
    const area = `/${ROTAS_ADMIN.BASE}/`;
    return retorno.startsWith(area) && !retorno.includes(ROTAS_ADMIN.LOGIN)
      ? retorno
      : `${area}${ROTAS_ADMIN.PAINEL}`;
  }
}
