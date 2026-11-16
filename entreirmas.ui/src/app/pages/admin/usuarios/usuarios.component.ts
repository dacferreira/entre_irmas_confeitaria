import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { confirmar } from '@components/confirmacao/confirmacao.component';
import { IconeComponent } from '@components/icone/icone.component';
import { Usuario } from '@models/admin.model';
import { UsuarioService } from '@services/admin.service';
import { AuthService } from '@services/auth.service';
import { NotificacaoService } from '@services/notificacao.service';
import { filter, switchMap } from 'rxjs';
import { UsuarioFormComponent } from './usuario-form/usuario-form.component';

@Component({
  selector: 'eirm-usuarios',
  imports: [DatePipe, IconeComponent],
  templateUrl: './usuarios.component.html',
  styleUrl: './usuarios.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UsuariosComponent {
  private readonly usuarioService = inject(UsuarioService);
  private readonly dialogo = inject(MatDialog);
  private readonly notificacao = inject(NotificacaoService);

  readonly auth = inject(AuthService);
  readonly usuarios = signal<Usuario[] | null>(null);
  readonly falhou = signal(false);
  readonly alterando = signal<number | null>(null);

  constructor() {
    this.carregar();
  }

  ehVoce(usuario: Usuario): boolean {
    return usuario.login === this.auth.login();
  }

  abrirFormulario(usuario?: Usuario): void {
    this.dialogo
      .open(UsuarioFormComponent, {
        data: usuario ?? null,
        width: '480px',
        maxWidth: 'calc(100vw - 24px)',
        panelClass: 'eirm-dialogo',
        disableClose: true,
      })
      .afterClosed()
      .subscribe((salvo) => salvo && this.carregar());
  }

  alternarSituacao(usuario: Usuario): void {
    this.alterando.set(usuario.id);
    this.usuarioService.alterarSituacao(usuario.id, !usuario.ativo).subscribe({
      next: () => {
        this.usuarios.update((lista) =>
          (lista ?? []).map((u) => (u.id === usuario.id ? { ...u, ativo: !usuario.ativo } : u)),
        );
        this.alterando.set(null);
        this.notificacao.sucesso(usuario.ativo ? `Acesso de ${usuario.nome} desativado.` : `Acesso de ${usuario.nome} liberado.`);
      },
      error: () => this.alterando.set(null),
    });
  }

  excluir(usuario: Usuario): void {
    confirmar(this.dialogo, {
      titulo: `Excluir ${usuario.nome}?`,
      mensagem: `O login "${usuario.login}" deixa de funcionar. O que essa pessoa cadastrou continua no sistema.`,
      confirmar: 'Excluir usuário',
      perigo: true,
    })
      .pipe(
        filter(Boolean),
        switchMap(() => this.usuarioService.excluir(usuario.id)),
      )
      .subscribe(() => {
        this.usuarios.update((lista) => (lista ?? []).filter((u) => u.id !== usuario.id));
        this.notificacao.sucesso(`${usuario.nome} excluído.`);
      });
  }

  carregar(): void {
    this.falhou.set(false);
    this.usuarioService.listar().subscribe({
      next: (usuarios) => this.usuarios.set(usuarios),
      error: () => this.falhou.set(true),
    });
  }
}
