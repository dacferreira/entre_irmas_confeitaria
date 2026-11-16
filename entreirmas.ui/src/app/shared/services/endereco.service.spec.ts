import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '@environments/environment';
import { SEM_AVISO_DE_ERRO } from '@interceptors/erro.interceptor';
import { EnderecoService } from './endereco.service';

describe('EnderecoService', () => {
  let servico: EnderecoService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    servico = TestBed.inject(EnderecoService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('consulta o CEP só com dígitos e sem o aviso geral de erro', () => {
    const recebido = jest.fn();
    servico.consultarCep('01001-000').subscribe(recebido);

    const requisicao = http.expectOne(`${environment.apiUrl}/enderecos/cep/01001000`);
    expect(requisicao.request.method).toBe('GET');
    expect(requisicao.request.context.get(SEM_AVISO_DE_ERRO)).toBe(true);
    requisicao.flush({ cep: '01001000', logradouro: 'Praça da Sé' });

    expect(recebido).toHaveBeenCalledWith(expect.objectContaining({ logradouro: 'Praça da Sé' }));
  });
});
