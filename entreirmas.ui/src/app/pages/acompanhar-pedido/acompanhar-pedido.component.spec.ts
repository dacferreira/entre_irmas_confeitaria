import { HttpErrorResponse } from '@angular/common/http';
import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { AcompanhamentoPedido } from '@models/pedido.model';
import { PedidoService } from '@services/pedido.service';
import { criarAcompanhamento } from '@testing/dados-de-teste';
import { Subject, of, throwError } from 'rxjs';
import {
  AcompanharPedidoComponent,
  MENSAGEM_CODIGO_INVALIDO,
  MENSAGEM_CONSULTA_INDISPONIVEL,
  MENSAGEM_MUITAS_CONSULTAS,
  MENSAGEM_PEDIDO_NAO_ENCONTRADO,
  etapasDoPedido,
  mensagemDeErroDaConsulta,
} from './acompanhar-pedido.component';

describe('etapasDoPedido', () => {
  it('marca as etapas anteriores como concluídas e a do status como atual', () => {
    const etapas = etapasDoPedido('EmPreparo', 'Retirada');

    expect(etapas.map((e) => e.estado)).toEqual(['concluida', 'concluida', 'atual', 'futura', 'futura']);
    expect(etapas[3].titulo).toBe('Pronto para retirar');
    expect(etapas[4].titulo).toBe('Retirado');
  });

  it('usa os textos da entrega e conclui tudo quando entregue', () => {
    const etapas = etapasDoPedido('Entregue', 'Entrega');

    expect(etapas.every((e) => e.estado === 'concluida')).toBe(true);
    expect(etapas[3].titulo).toBe('Pronto para entrega');
    expect(etapas[4].titulo).toBe('Entregue');
  });
});

describe('mensagemDeErroDaConsulta', () => {
  it.each([
    [404, MENSAGEM_PEDIDO_NAO_ENCONTRADO],
    [400, MENSAGEM_CODIGO_INVALIDO],
    [429, MENSAGEM_MUITAS_CONSULTAS],
    [500, MENSAGEM_CONSULTA_INDISPONIVEL],
    [0, MENSAGEM_CONSULTA_INDISPONIVEL],
  ])('status %s', (status, mensagem) => {
    expect(mensagemDeErroDaConsulta(new HttpErrorResponse({ status }))).toBe(mensagem);
  });

  it('erro que não é HTTP vira indisponível', () => {
    expect(mensagemDeErroDaConsulta(new Error('x'))).toBe(MENSAGEM_CONSULTA_INDISPONIVEL);
  });
});

describe('AcompanharPedidoComponent', () => {
  const pedidoService = { acompanhar: jest.fn(), pedidosRecentes: signal<string[]>([]) };
  let fixture: ComponentFixture<AcompanharPedidoComponent>;
  let componente: AcompanharPedidoComponent;
  let navegar: jest.SpyInstance;

  beforeEach(() => {
    pedidoService.pedidosRecentes.set([]);
    TestBed.configureTestingModule({
      providers: [provideRouter([]), { provide: PedidoService, useValue: pedidoService }],
    });
    navegar = jest.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    fixture = TestBed.createComponent(AcompanharPedidoComponent);
    componente = fixture.componentInstance;
  });

  const elemento = (): HTMLElement => fixture.nativeElement;

  function abrirCom(codigo?: string): void {
    fixture.componentRef.setInput('codigo', codigo);
    fixture.detectChanges();
  }

  it('sem código na rota mostra só a busca', () => {
    abrirCom(undefined);

    expect(pedidoService.acompanhar).not.toHaveBeenCalled();
    expect(elemento().querySelector('input')).not.toBeNull();
    expect(elemento().querySelector('.cartao')).toBeNull();
  });

  it('com código na rota consulta e mostra a linha do tempo com a etapa atual', () => {
    pedidoService.acompanhar.mockReturnValue(of(criarAcompanhamento({ status: 'Confirmado' })));

    abrirCom('ei-abc234');

    expect(pedidoService.acompanhar).toHaveBeenCalledWith('EI-ABC234');
    expect(componente.campoCodigo.value).toBe('EI-ABC234');
    expect(elemento().querySelector('.cartao__titulo')?.textContent).toContain('Olá, Maria!');
    expect(elemento().querySelector('.selo')?.textContent).toContain('Confirmado');
    const atual = elemento().querySelector('[aria-current="step"]');
    expect(atual?.textContent).toContain('Confirmado');
    expect(elemento().querySelectorAll('.etapa--concluida').length).toBe(1);
    expect(elemento().querySelector('.detalhes')?.textContent).toContain('20/11/2026');
    expect(elemento().querySelector('.botao--whatsapp')?.getAttribute('href')).toContain(encodeURIComponent('EI-ABC234'));
  });

  it('mostra o carregamento enquanto consulta', () => {
    const resposta = new Subject<AcompanhamentoPedido>();
    pedidoService.acompanhar.mockReturnValue(resposta);

    abrirCom('EI-ABC234');
    expect(elemento().querySelector('.carregando')?.textContent).toContain('Consultando');

    resposta.next(criarAcompanhamento());
    fixture.detectChanges();
    expect(elemento().querySelector('.carregando')).toBeNull();
    expect(elemento().querySelector('.cartao')).not.toBeNull();
  });

  it('pedido cancelado mostra o aviso no lugar da linha do tempo', () => {
    pedidoService.acompanhar.mockReturnValue(of(criarAcompanhamento({ status: 'Cancelado' })));

    abrirCom('EI-ABC234');

    expect(elemento().querySelector('.cancelado')?.textContent).toContain('cancelado');
    expect(elemento().querySelector('.etapas')).toBeNull();
    expect(elemento().querySelector('.selo--cancelado')?.textContent).toContain('Cancelado');
  });

  it('código inválido na rota avisa sem consultar', () => {
    abrirCom('abc');

    expect(pedidoService.acompanhar).not.toHaveBeenCalled();
    expect(elemento().querySelector('[role="alert"]')?.textContent).toContain(MENSAGEM_CODIGO_INVALIDO);
  });

  it('pedido inexistente mostra a mensagem no lugar do resultado', () => {
    pedidoService.acompanhar.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 404 })));

    abrirCom('EI-ZZZZZZ');

    expect(elemento().querySelector('[role="alert"]')?.textContent).toContain(MENSAGEM_PEDIDO_NAO_ENCONTRADO);
    expect(elemento().querySelector('.cartao')).toBeNull();
    expect(componente.carregando()).toBe(false);
  });

  it('atualizar consulta de novo e mantém o pedido na tela se falhar', () => {
    pedidoService.acompanhar.mockReturnValue(of(criarAcompanhamento({ status: 'Confirmado' })));
    abrirCom('EI-ABC234');

    pedidoService.acompanhar.mockReturnValue(of(criarAcompanhamento({ status: 'Pronto' })));
    componente.atualizar();
    fixture.detectChanges();
    expect(elemento().querySelector('.selo')?.textContent).toContain('Pronto para retirar');

    pedidoService.acompanhar.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 503 })));
    componente.atualizar();
    fixture.detectChanges();
    expect(elemento().querySelector('[role="alert"]')?.textContent).toContain(MENSAGEM_CONSULTA_INDISPONIVEL);
    expect(elemento().querySelector('.cartao')).not.toBeNull();
    expect(pedidoService.acompanhar).toHaveBeenCalledTimes(3);
  });

  it('consultar valida o formato, vai para a rota do código e repete a consulta do mesmo código', () => {
    abrirCom(undefined);

    componente.campoCodigo.setValue('xyz');
    componente.consultar();
    expect(navegar).not.toHaveBeenCalled();
    expect(componente.campoCodigo.hasError('codigo')).toBe(true);

    componente.campoCodigo.setValue('abc 234');
    componente.consultar();
    expect(navegar).toHaveBeenCalledWith(['/', 'acompanhar-pedido', 'EI-ABC234']);

    pedidoService.acompanhar.mockReturnValue(of(criarAcompanhamento()));
    abrirCom('EI-ABC234');
    navegar.mockClear();
    componente.consultar();
    expect(navegar).not.toHaveBeenCalled();
    expect(pedidoService.acompanhar).toHaveBeenCalledTimes(2);
  });

  it('enviar o formulário (Enter ou botão) consulta sem recarregar a página', () => {
    abrirCom(undefined);
    const campo = elemento().querySelector<HTMLInputElement>('input')!;
    campo.value = 'ei-abc234';
    campo.dispatchEvent(new Event('input'));

    const envio = new Event('submit', { cancelable: true });
    elemento().querySelector('form')!.dispatchEvent(envio);

    expect(envio.defaultPrevented).toBe(true);
    expect(navegar).toHaveBeenCalledWith(['/', 'acompanhar-pedido', 'EI-ABC234']);
  });

  it('lista os pedidos do aparelho, sem repetir o que está na tela', () => {
    pedidoService.pedidosRecentes.set(['EI-ABC234', 'EI-DEF567']);
    pedidoService.acompanhar.mockReturnValue(of(criarAcompanhamento()));

    abrirCom('EI-ABC234');

    const links = [...elemento().querySelectorAll<HTMLAnchorElement>('.recentes__codigo')];
    expect(links.map((a) => a.textContent?.trim())).toEqual(['EI-DEF567']);
    expect(links[0].getAttribute('href')).toBe('/acompanhar-pedido/EI-DEF567');
  });
});
