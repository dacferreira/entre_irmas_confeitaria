import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { UsuarioService } from '@services/admin.service';
import { AuthService } from '@services/auth.service';
import { NotificacaoService } from '@services/notificacao.service';
import { criarDialogoFalso, criarNotificacaoFalsa, criarUsuario } from '@testing/dados-admin';
import { of, throwError } from 'rxjs';
import { UsuariosComponent } from './usuarios.component';

describe('UsuariosComponent', () => {
  const usuarioService = { listar: jest.fn(), alterarSituacao: jest.fn(), excluir: jest.fn() };
  const suellen = criarUsuario();
  const tatiane = criarUsuario({ id: 2, login: 'tatiane', nome: 'Tatiane', ultimoAcesso: null });
  let notificacao: ReturnType<typeof criarNotificacaoFalsa>;
  let dialogo: ReturnType<typeof criarDialogoFalso>;
  let fixture: ComponentFixture<UsuariosComponent>;
  let componente: UsuariosComponent;

  function criar(): void {
    notificacao = criarNotificacaoFalsa();
    dialogo = criarDialogoFalso(true);
    TestBed.configureTestingModule({
      providers: [
        { provide: UsuarioService, useValue: usuarioService },
        { provide: AuthService, useValue: { login: signal('suellen') } },
        { provide: NotificacaoService, useValue: notificacao },
        { provide: MatDialog, useValue: dialogo },
      ],
    });
    fixture = TestBed.createComponent(UsuariosComponent);
    componente = fixture.componentInstance;
    fixture.detectChanges();
  }

  beforeEach(() => usuarioService.listar.mockReturnValue(of([suellen, tatiane])));

  it('marca quem está logado e não deixa a pessoa se desativar ou se excluir', () => {
    criar();
    const linhas = fixture.nativeElement.querySelectorAll('tbody tr');

    expect(linhas[0].querySelector('.voce')).not.toBeNull();
    expect(linhas[0].querySelector('.interruptor').disabled).toBe(true);
    expect(linhas[0].querySelector('.botao-icone--perigo')).toBeNull();
    expect(linhas[1].textContent).toContain('nunca entrou');
  });

  it('bloqueia e libera o acesso de outra pessoa', () => {
    usuarioService.alterarSituacao.mockReturnValue(of(undefined));
    criar();

    componente.alternarSituacao(tatiane);
    componente.alternarSituacao({ ...tatiane, ativo: false });

    expect(usuarioService.alterarSituacao).toHaveBeenCalledWith(2, false);
    expect(notificacao.sucesso).toHaveBeenCalledWith('Acesso de Tatiane desativado.');
    expect(notificacao.sucesso).toHaveBeenCalledWith('Acesso de Tatiane liberado.');
  });

  it('exclui depois de confirmar e recarrega ao salvar no formulário', () => {
    usuarioService.excluir.mockReturnValue(of(undefined));
    criar();

    componente.excluir(tatiane);
    componente.abrirFormulario();

    expect(usuarioService.excluir).toHaveBeenCalledWith(2);
    expect(dialogo.open.mock.calls[1][1].data).toBeNull();
    expect(usuarioService.listar).toHaveBeenCalledTimes(2);
  });

  it('trata as falhas', () => {
    usuarioService.alterarSituacao.mockReturnValue(throwError(() => new Error('x')));
    usuarioService.listar.mockReturnValue(throwError(() => new Error('x')));
    criar();

    componente.alternarSituacao(tatiane);

    expect(componente.alterando()).toBeNull();
    expect(fixture.nativeElement.querySelector('[role="alert"]')).not.toBeNull();
  });
});
