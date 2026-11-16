import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { environment } from '@environments/environment';
import { AuthService } from '@services/auth.service';
import { authInterceptor } from './auth.interceptor';

describe('authInterceptor', () => {
  const auth = { token: jest.fn(), sair: jest.fn() };
  const api = environment.apiUrl;
  let http: HttpClient;
  let controlador: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: auth },
      ],
    });
    http = TestBed.inject(HttpClient);
    controlador = TestBed.inject(HttpTestingController);
  });

  it('envia o token só para a API', () => {
    auth.token.mockReturnValue('abc');

    http.get(`${api}/admin/painel`).subscribe();
    http.get('https://outro-site.com/dados').subscribe();

    expect(controlador.expectOne(`${api}/admin/painel`).request.headers.get('Authorization')).toBe('Bearer abc');
    expect(controlador.expectOne('https://outro-site.com/dados').request.headers.has('Authorization')).toBe(false);
  });

  it('não envia cabeçalho sem sessão', () => {
    auth.token.mockReturnValue(null);

    http.get(`${api}/produtos`).subscribe();

    expect(controlador.expectOne(`${api}/produtos`).request.headers.has('Authorization')).toBe(false);
  });

  it('encerra a sessão no 401 da área administrativa, mas não no erro de login', () => {
    auth.token.mockReturnValue('vencido');
    const ignorar = { error: () => undefined };

    http.get(`${api}/admin/painel`).subscribe(ignorar);
    controlador.expectOne(`${api}/admin/painel`).flush(null, { status: 401, statusText: 'Unauthorized' });
    expect(auth.sair).toHaveBeenCalledTimes(1);

    http.post(`${api}/autenticacao/login`, {}).subscribe(ignorar);
    controlador.expectOne(`${api}/autenticacao/login`).flush({ error: 'x' }, { status: 401, statusText: 'Unauthorized' });
    http.get(`${api}/admin/pedidos`).subscribe(ignorar);
    controlador.expectOne(`${api}/admin/pedidos`).flush(null, { status: 500, statusText: 'Erro' });
    expect(auth.sair).toHaveBeenCalledTimes(1);
  });
});
