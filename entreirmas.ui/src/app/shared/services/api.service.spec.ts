import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '@environments/environment';
import { ApiService } from './api.service';

describe('ApiService', () => {
  let servico: ApiService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    servico = TestBed.inject(ApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('monta a URL com a base do environment e ignora parâmetros vazios', () => {
    servico.get('/produtos', { categoria: 'bolos', busca: '', destaque: null, pagina: 2 }).subscribe();

    const requisicao = http.expectOne((r) => r.url === `${environment.apiUrl}/produtos`);
    expect(requisicao.request.params.keys()).toEqual(['categoria', 'pagina']);
    expect(requisicao.request.params.get('pagina')).toBe('2');
    requisicao.flush([]);
  });

  it('envia o corpo no POST', () => {
    servico.post('pedidos', { nome: 'Maria' }).subscribe();

    const requisicao = http.expectOne(`${environment.apiUrl}/pedidos`);
    expect(requisicao.request.method).toBe('POST');
    expect(requisicao.request.body).toEqual({ nome: 'Maria' });
    requisicao.flush({});
  });
});
