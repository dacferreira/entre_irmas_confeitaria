import { HttpClient, HttpContext, HttpErrorResponse, HttpRequest, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { NotificacaoService } from '@services/notificacao.service';
import { TimeoutError } from 'rxjs';
import { SEM_AVISO_DE_ERRO, erroInterceptor, mensagemDeErro } from './erro.interceptor';

const GET = new HttpRequest('GET', '/api/produtos/x');
const POST = new HttpRequest('POST', '/api/pedidos', {});

function erroHttp(status: number, corpo: unknown = null): HttpErrorResponse {
  return new HttpErrorResponse({ status, error: corpo });
}

describe('mensagemDeErro', () => {
  it('trata timeout, falta de conexão e erros desconhecidos', () => {
    expect(mensagemDeErro(new TimeoutError(), GET)).toContain('demorou');
    expect(mensagemDeErro(erroHttp(0), GET)).toContain('Sem conexão');
    expect(mensagemDeErro(new Error('x'), GET)).toContain('Não foi possível');
  });

  it('deixa o 404 de GET para a tela tratar', () => {
    expect(mensagemDeErro(erroHttp(404), GET)).toBeNull();
    expect(mensagemDeErro(erroHttp(404, { error: 'Não achou' }), POST)).toBe('Não achou');
  });

  it('usa as mensagens dos campos ou a mensagem geral no 400', () => {
    const comCampos = { mensagem: 'Verifique', erros: [{ campo: 'nome', mensagem: 'Nome é obrigatório.' }] };
    expect(mensagemDeErro(erroHttp(400, comCampos), POST)).toBe('Nome é obrigatório.');
    expect(mensagemDeErro(erroHttp(400, { mensagem: 'Pedido mínimo', erros: [] }), POST)).toBe('Pedido mínimo');
    expect(mensagemDeErro(erroHttp(400), POST)).toBe('Verifique os dados informados.');
  });

  it('usa o corpo do 429 e mensagem genérica no 500', () => {
    expect(mensagemDeErro(erroHttp(429, { error: 'Aguarde um minuto' }), POST)).toBe('Aguarde um minuto');
    expect(mensagemDeErro(erroHttp(429), POST)).toContain('Não foi possível');
    expect(mensagemDeErro(erroHttp(500, { error: 'detalhe interno' }), POST)).toContain('Tente novamente');
  });

  it('no 401 usa a mensagem do login ou avisa que a sessão expirou', () => {
    expect(mensagemDeErro(erroHttp(401, { error: 'Usuário ou senha inválidos.' }), POST)).toBe('Usuário ou senha inválidos.');
    expect(mensagemDeErro(erroHttp(401), GET)).toBe('Sua sessão expirou. Entre novamente.');
  });
});

describe('erroInterceptor', () => {
  const notificacao = { erro: jest.fn() };
  let http: HttpClient;
  let controlador: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([erroInterceptor])),
        provideHttpClientTesting(),
        { provide: NotificacaoService, useValue: notificacao },
      ],
    });
    http = TestBed.inject(HttpClient);
    controlador = TestBed.inject(HttpTestingController);
  });

  it('notifica e repassa o erro', () => {
    const aoFalhar = jest.fn();
    http.post('/api/pedidos', {}).subscribe({ error: aoFalhar });

    controlador.expectOne('/api/pedidos').flush({ error: 'Calma' }, { status: 429, statusText: 'Too Many' });

    expect(notificacao.erro).toHaveBeenCalledWith('Calma');
    expect(aoFalhar).toHaveBeenCalled();
  });

  it('não notifica 404 de consulta', () => {
    http.get('/api/produtos/x').subscribe({ error: () => undefined });

    controlador.expectOne('/api/produtos/x').flush(null, { status: 404, statusText: 'Not Found' });

    expect(notificacao.erro).not.toHaveBeenCalled();
  });

  it('não notifica quando a chamada pede silêncio, mas repassa o erro', () => {
    const aoFalhar = jest.fn();
    http.get('/api/enderecos/cep/1', { context: new HttpContext().set(SEM_AVISO_DE_ERRO, true) }).subscribe({ error: aoFalhar });

    controlador.expectOne('/api/enderecos/cep/1').flush(null, { status: 503, statusText: 'Unavailable' });

    expect(notificacao.erro).not.toHaveBeenCalled();
    expect(aoFalhar).toHaveBeenCalled();
  });
});
