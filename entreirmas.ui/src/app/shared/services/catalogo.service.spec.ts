import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '@environments/environment';
import { CATEGORIAS, PRODUTOS } from '@testing/dados-de-teste';
import { CatalogoService } from './catalogo.service';

describe('CatalogoService', () => {
  let servico: CatalogoService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    servico = TestBed.inject(CatalogoService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('faz uma única chamada de categorias para vários inscritos', () => {
    const recebidos: unknown[] = [];
    servico.listarCategorias().subscribe((c) => recebidos.push(c));
    servico.listarCategorias().subscribe((c) => recebidos.push(c));

    http.expectOne(`${environment.apiUrl}/categorias`).flush(CATEGORIAS);

    expect(recebidos).toEqual([CATEGORIAS, CATEGORIAS]);
  });

  it('descarta o cache de categorias quando a chamada falha', () => {
    servico.listarCategorias().subscribe({ error: () => undefined });
    http.expectOne(`${environment.apiUrl}/categorias`).flush(null, { status: 500, statusText: 'Erro' });

    servico.listarCategorias().subscribe();
    http.expectOne(`${environment.apiUrl}/categorias`).flush(CATEGORIAS);
  });

  it('lista produtos com filtro e obtém por slug codificado', () => {
    servico.listarProdutos({ categoria: 'docinhos' }).subscribe((lista) => expect(lista).toEqual(PRODUTOS));
    http.expectOne(`${environment.apiUrl}/produtos?categoria=docinhos`).flush(PRODUTOS);

    servico.obterProduto('bicho de pé').subscribe();
    http.expectOne(`${environment.apiUrl}/produtos/bicho%20de%20p%C3%A9`).flush(PRODUTOS[2]);
  });
});
