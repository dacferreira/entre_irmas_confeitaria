import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '@environments/environment';
import { CotacaoEntrega } from '@models/pedido.model';
import { ENDERECO_ENVIADO } from '@testing/dados-de-teste';
import { EntregaService } from './entrega.service';

describe('EntregaService', () => {
  let servico: EntregaService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    servico = TestBed.inject(EntregaService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('pede a cotação da entrega para o endereço', () => {
    const cotacao: CotacaoEntrega = { disponivel: true, parceiro: 'Uber', valor: 10.21, prazoMinutos: 21, mensagem: null };
    let recebida: CotacaoEntrega | undefined;

    servico.cotar(ENDERECO_ENVIADO).subscribe((c) => (recebida = c));

    const requisicao = http.expectOne(`${environment.apiUrl}/entregas/cotacao`);
    expect(requisicao.request.method).toBe('POST');
    expect(requisicao.request.body).toEqual(ENDERECO_ENVIADO);
    requisicao.flush(cotacao);
    expect(recebida).toEqual(cotacao);
  });

  it('monta o mapa do trajeto saindo das coordenadas da loja até o endereço', () => {
    const url = servico.urlDoTrajeto(ENDERECO_ENVIADO);

    expect(url).toBe(
      `https://maps.google.com/maps?saddr=${environment.coordenadasLoja}&daddr=` +
        `${encodeURIComponent('Rua das Flores, 10 - Centro, Rio de Janeiro - RJ, 20000000')}&output=embed`,
    );
  });
});
