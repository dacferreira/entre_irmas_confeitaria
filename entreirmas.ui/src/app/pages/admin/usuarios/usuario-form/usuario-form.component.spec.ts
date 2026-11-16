import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Usuario } from '@models/admin.model';
import { UsuarioService } from '@services/admin.service';
import { NotificacaoService } from '@services/notificacao.service';
import { criarNotificacaoFalsa, criarUsuario } from '@testing/dados-admin';
import { of } from 'rxjs';
import { UsuarioFormComponent } from './usuario-form.component';

describe('UsuarioFormComponent', () => {
  const usuarioService = { criar: jest.fn(), atualizar: jest.fn() };
  const referencia = { close: jest.fn() };
  let notificacao: ReturnType<typeof criarNotificacaoFalsa>;
  let fixture: ComponentFixture<UsuarioFormComponent>;

  function criar(usuario: Usuario | null): UsuarioFormComponent {
    notificacao = criarNotificacaoFalsa();
    TestBed.configureTestingModule({
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: usuario },
        { provide: MatDialogRef, useValue: referencia },
        { provide: UsuarioService, useValue: usuarioService },
        { provide: NotificacaoService, useValue: notificacao },
      ],
    });
    fixture = TestBed.createComponent(UsuarioFormComponent);
    fixture.detectChanges();
    return fixture.componentInstance;
  }

  it('novo usuário exige senha de 8 caracteres e login sem espaços', () => {
    usuarioService.criar.mockReturnValue(of(criarUsuario({ nome: 'Tati' })));
    const componente = criar(null);

    componente.formulario.setValue({ login: 'com espaco', nome: 'Tati', senha: '123' });
    componente.salvar();
    expect(usuarioService.criar).not.toHaveBeenCalled();
    expect(componente.formulario.controls.login.hasError('pattern')).toBe(true);
    expect(componente.formulario.controls.senha.hasError('minlength')).toBe(true);

    componente.formulario.setValue({ login: 'tatiane', nome: ' Tati ', senha: 'senha-boa-1' });
    componente.salvar();
    expect(usuarioService.criar).toHaveBeenCalledWith({ login: 'tatiane', nome: 'Tati', senha: 'senha-boa-1' });
    expect(notificacao.sucesso).toHaveBeenCalledWith('Tati já pode entrar.');
  });

  it('na edição o login fica travado e a senha é opcional', () => {
    usuarioService.atualizar.mockReturnValue(of(criarUsuario()));
    const componente = criar(criarUsuario());

    expect(componente.formulario.controls.login.disabled).toBe(true);
    componente.salvar();

    expect(usuarioService.atualizar).toHaveBeenCalledWith(1, { login: 'suellen', nome: 'Suellen Alves', senha: null });
    expect(fixture.nativeElement.querySelector('mat-hint')?.textContent).toContain('não pode ser alterado');
  });

  it('mostra a senha e cancela', () => {
    const componente = criar(null);
    const campo = (): HTMLInputElement => fixture.nativeElement.querySelector('input[formControlName="senha"]');

    componente.alternarSenha();
    fixture.detectChanges();
    componente.cancelar();

    expect(campo().type).toBe('text');
    expect(referencia.close).toHaveBeenCalledWith();
  });
});
