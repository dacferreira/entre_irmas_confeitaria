import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { environment } from '@environments/environment';
import { criarSessao } from '@testing/dados-admin';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let controlador: HttpTestingController;
  let navegar: jest.SpyInstance;

  function criar(): AuthService {
    TestBed.configureTestingModule({
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
    controlador = TestBed.inject(HttpTestingController);
    navegar = jest.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    return TestBed.inject(AuthService);
  }

  beforeEach(() => localStorage.clear());

  it('entra, guarda a sessão e expõe nome, login e token', () => {
    const auth = criar();
    let nome = '';

    auth.entrar('suellen', 'segredo').subscribe((sessao) => (nome = sessao.nome));
    const requisicao = controlador.expectOne(`${environment.apiUrl}/autenticacao/login`);
    expect(requisicao.request.body).toEqual({ login: 'suellen', senha: 'segredo' });
    requisicao.flush(criarSessao());

    expect(nome).toBe('Suellen Alves');
    expect(auth.autenticado()).toBe(true);
    expect(auth.login()).toBe('suellen');
    expect(auth.token()).toBe('token-de-teste');
    expect(JSON.parse(localStorage.getItem('eirm.sessao') ?? '{}').login).toBe('suellen');
  });

  it('recupera a sessão salva e ignora a vencida', () => {
    localStorage.setItem('eirm.sessao', JSON.stringify(criarSessao()));
    expect(criar().autenticado()).toBe(true);

    TestBed.resetTestingModule();
    localStorage.setItem('eirm.sessao', JSON.stringify(criarSessao({ expiraEm: '2020-01-01T00:00:00Z' })));
    const vencida = criar();
    expect(vencida.autenticado()).toBe(false);
    expect(vencida.token()).toBeNull();
  });

  it('ignora sessão salva corrompida', () => {
    localStorage.setItem('eirm.sessao', '{quebrado');

    expect(criar().autenticado()).toBe(false);
  });

  it('sai, apaga a sessão e vai para o login com o retorno', () => {
    localStorage.setItem('eirm.sessao', JSON.stringify(criarSessao()));
    const auth = criar();

    auth.sair('/admin/pedidos');

    expect(auth.autenticado()).toBe(false);
    expect(localStorage.getItem('eirm.sessao')).toBeNull();
    expect(navegar).toHaveBeenCalledWith(['/', 'admin', 'entrar'], { queryParams: { retorno: '/admin/pedidos' } });

    auth.sair();
    expect(navegar).toHaveBeenLastCalledWith(['/', 'admin', 'entrar'], { queryParams: {} });
  });

  it('continua funcionando quando o armazenamento está bloqueado', () => {
    const auth = criar();
    jest.spyOn(Storage.prototype, 'setItem').mockImplementation(() => {
      throw new Error('bloqueado');
    });

    auth.entrar('suellen', 'x').subscribe();
    controlador.expectOne(`${environment.apiUrl}/autenticacao/login`).flush(criarSessao());

    expect(auth.autenticado()).toBe(true);
  });
});
