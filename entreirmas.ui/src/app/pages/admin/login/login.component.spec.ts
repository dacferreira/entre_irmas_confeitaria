import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { AuthService } from '@services/auth.service';
import { criarSessao } from '@testing/dados-admin';
import { of, throwError } from 'rxjs';
import { LoginComponent } from './login.component';

describe('LoginComponent', () => {
  const auth = { entrar: jest.fn() };
  let fixture: ComponentFixture<LoginComponent>;
  let componente: LoginComponent;
  let navegar: jest.SpyInstance;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideRouter([]), { provide: AuthService, useValue: auth }] });
    navegar = jest.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    fixture = TestBed.createComponent(LoginComponent);
    componente = fixture.componentInstance;
    fixture.detectChanges();
  });

  function preencher(): void {
    componente.formulario.setValue({ login: ' suellen ', senha: 'segredo' });
  }

  it('não envia sem usuário e senha', () => {
    componente.entrar();
    fixture.detectChanges();

    expect(auth.entrar).not.toHaveBeenCalled();
    expect(fixture.nativeElement.querySelectorAll('mat-error').length).toBe(2);
  });

  it('entra e vai para o painel', () => {
    auth.entrar.mockReturnValue(of(criarSessao()));
    preencher();

    componente.entrar();

    expect(auth.entrar).toHaveBeenCalledWith('suellen', 'segredo');
    expect(navegar).toHaveBeenCalledWith('/admin/painel');
  });

  it('volta para a tela pedida, mas só dentro da área administrativa', () => {
    auth.entrar.mockReturnValue(of(criarSessao()));
    preencher();

    fixture.componentRef.setInput('retorno', '/admin/pedidos?status=Recebido');
    componente.entrar();
    expect(navegar).toHaveBeenLastCalledWith('/admin/pedidos?status=Recebido');

    fixture.componentRef.setInput('retorno', 'https://site-malicioso.com');
    componente.entrar();
    expect(navegar).toHaveBeenLastCalledWith('/admin/painel');
  });

  it('limpa a senha quando o login falha', () => {
    auth.entrar.mockReturnValue(throwError(() => new Error('401')));
    preencher();

    componente.entrar();

    expect(componente.formulario.controls.senha.value).toBe('');
    expect(componente.entrando()).toBe(false);
    expect(navegar).not.toHaveBeenCalled();
  });

  it('mostra e esconde a senha', () => {
    const campo = (): HTMLInputElement => fixture.nativeElement.querySelector('input[formControlName="senha"]');
    expect(campo().type).toBe('password');

    componente.alternarSenha();
    fixture.detectChanges();

    expect(campo().type).toBe('text');
  });
});
