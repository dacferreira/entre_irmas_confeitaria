import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { environment } from '@environments/environment';
import { TimeoutError } from 'rxjs';
import { timeoutInterceptor } from './timeout.interceptor';

describe('timeoutInterceptor', () => {
  let http: HttpClient;
  let controlador: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(withInterceptors([timeoutInterceptor])), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpClient);
    controlador = TestBed.inject(HttpTestingController);
  });

  it('cancela a requisição que passa do tempo máximo', fakeAsync(() => {
    let erro: unknown;
    http.get('/api/lenta').subscribe({ error: (e: unknown) => (erro = e) });

    tick(environment.timeoutMs + 1);

    expect(erro).toBeInstanceOf(TimeoutError);
    expect(controlador.expectOne('/api/lenta').cancelled).toBe(true);
  }));

  it('deixa passar a resposta dentro do prazo', () => {
    let resposta: unknown;
    http.get('/api/rapida').subscribe((r) => (resposta = r));

    controlador.expectOne('/api/rapida').flush({ ok: true });

    expect(resposta).toEqual({ ok: true });
  });
});
