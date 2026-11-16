import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '@environments/environment';
import { SEM_AVISO_DE_ERRO } from '@interceptors/erro.interceptor';
import { LojaService } from './loja.service';

describe('LojaService', () => {
  let servico: LojaService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    servico = TestBed.inject(LojaService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('começa aberta e atualiza pela API sem o aviso geral de erro', () => {
    expect(servico.aberta()).toBe(true);

    servico.atualizar();
    const requisicao = http.expectOne(`${environment.apiUrl}/loja`);
    expect(requisicao.request.context.get(SEM_AVISO_DE_ERRO)).toBe(true);
    requisicao.flush({ aberta: false, mensagem: 'Voltamos amanhã' });

    expect(servico.aberta()).toBe(false);
    expect(servico.situacao().mensagem).toBe('Voltamos amanhã');
  });

  it('se a consulta falhar, mantém a última situação', () => {
    servico.atualizar();
    http.expectOne(`${environment.apiUrl}/loja`).flush(null, { status: 503, statusText: 'Indisponível' });

    expect(servico.aberta()).toBe(true);
  });

  it('na área administrativa consulta e altera, atualizando a situação do site', () => {
    const recebido = jest.fn();
    servico.obterAdmin().subscribe(recebido);
    http.expectOne(`${environment.apiUrl}/admin/loja`).flush({ aberta: true, mensagem: null });
    expect(recebido).toHaveBeenCalled();

    servico.alterar({ aberta: false, mensagem: 'Feriado' }).subscribe();
    const requisicao = http.expectOne(`${environment.apiUrl}/admin/loja`);
    expect(requisicao.request.method).toBe('PUT');
    expect(requisicao.request.body).toEqual({ aberta: false, mensagem: 'Feriado' });
    requisicao.flush({ aberta: false, mensagem: 'Feriado', alteradoPor: 'suellen', dataDaAlteracao: '2026-11-15T10:00:00' });

    expect(servico.aberta()).toBe(false);
    expect(servico.situacao().mensagem).toBe('Feriado');
  });
});
