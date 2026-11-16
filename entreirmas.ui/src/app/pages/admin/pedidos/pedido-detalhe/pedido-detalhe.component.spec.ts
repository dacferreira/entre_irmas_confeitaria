import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material/dialog';
import { PedidoAdminService } from '@services/admin.service';
import { ImpressoraService } from '@services/impressora.service';
import { NotificacaoService } from '@services/notificacao.service';
import { criarDialogoFalso, criarNotificacaoFalsa, criarPedidoDetalhe } from '@testing/dados-admin';
import { of, throwError } from 'rxjs';
import { PedidoDetalheComponent } from './pedido-detalhe.component';

describe('PedidoDetalheComponent', () => {
  const pedidoService = { obter: jest.fn(), alterarStatus: jest.fn(), excluir: jest.fn() };
  const referencia = { close: jest.fn() };
  let notificacao: ReturnType<typeof criarNotificacaoFalsa>;
  let dialogo: ReturnType<typeof criarDialogoFalso>;
  let fixture: ComponentFixture<PedidoDetalheComponent>;
  let componente: PedidoDetalheComponent;

  const conectada = signal(false);
  const impressora = {
    suportada: true,
    conectada,
    imprimindo: signal(false),
    conectar: jest.fn(async () => true),
    imprimir: jest.fn(async () => undefined),
  };

  function criar(): void {
    notificacao = criarNotificacaoFalsa();
    dialogo = criarDialogoFalso(true);
    conectada.set(false);
    TestBed.configureTestingModule({
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: 5 },
        { provide: MatDialogRef, useValue: referencia },
        { provide: MatDialog, useValue: dialogo },
        { provide: PedidoAdminService, useValue: pedidoService },
        { provide: NotificacaoService, useValue: notificacao },
        { provide: ImpressoraService, useValue: impressora },
      ],
    });
    fixture = TestBed.createComponent(PedidoDetalheComponent);
    componente = fixture.componentInstance;
    fixture.detectChanges();
  }

  it('carrega o pedido com itens, custo, lucro e o link do WhatsApp do cliente', () => {
    pedidoService.obter.mockReturnValue(of(criarPedidoDetalhe()));
    criar();
    const elemento: HTMLElement = fixture.nativeElement;

    expect(pedidoService.obter).toHaveBeenCalledWith(5);
    expect(componente.lucro()).toBe(54);
    expect(componente.telefone()).toBe('(21) 99999-8888');
    expect(componente.linkWhatsApp()).toMatch(/^https:\/\/wa\.me\/5521999998888\?text=Ol%C3%A1%2C%20Maria/);
    expect(elemento.querySelector('.observacao')?.textContent).toContain('Sem lactose');
    expect(elemento.querySelector('.etapa--ativa')?.textContent).toContain('Aguardando confirmação');
  });

  it('muda a etapa e avisa a lista ao fechar', () => {
    pedidoService.obter.mockReturnValue(of(criarPedidoDetalhe()));
    pedidoService.alterarStatus.mockReturnValue(of(criarPedidoDetalhe({ status: 'Confirmado' })));
    criar();

    componente.alterarStatus('Recebido'); // mesma etapa: nada a fazer
    componente.alterarStatus('Confirmado');
    componente.fechar();

    expect(pedidoService.alterarStatus).toHaveBeenCalledTimes(1);
    expect(componente.pedido()?.status).toBe('Confirmado');
    expect(notificacao.sucesso).toHaveBeenCalledWith('Pedido EI-ABC234: confirmado.');
    expect(referencia.close).toHaveBeenLastCalledWith(true);
  });

  it('exclui depois de confirmar', () => {
    pedidoService.obter.mockReturnValue(of(criarPedidoDetalhe()));
    pedidoService.excluir.mockReturnValue(of(undefined));
    criar();

    componente.excluir();

    expect(pedidoService.excluir).toHaveBeenCalledWith(5);
    expect(referencia.close).toHaveBeenLastCalledWith(true);
  });

  it('imprime o recibo, pedindo antes a impressora se nenhuma estiver conectada', async () => {
    const pedido = criarPedidoDetalhe();
    pedidoService.obter.mockReturnValue(of(pedido));
    criar();
    const botao = fixture.nativeElement.querySelector('.imprimir') as HTMLButtonElement;
    expect(botao.textContent).toContain('Imprimir recibo');

    impressora.conectar.mockResolvedValueOnce(false);
    await componente.imprimirRecibo(); // desistiu de escolher a porta
    expect(impressora.imprimir).not.toHaveBeenCalled();

    await componente.imprimirRecibo();
    expect(impressora.conectar).toHaveBeenCalledTimes(2);
    expect(impressora.imprimir).toHaveBeenCalledWith(pedido);
    expect(notificacao.sucesso).toHaveBeenCalledWith('Recibo do pedido EI-ABC234 enviado para a impressora.');

    conectada.set(true);
    impressora.imprimir.mockRejectedValueOnce(new Error('desligada'));
    await componente.imprimirRecibo();
    expect(impressora.conectar).toHaveBeenCalledTimes(2);
    expect(notificacao.erro).toHaveBeenCalledWith(expect.stringContaining('Não foi possível imprimir'));
  });

  it('não exclui se a pessoa desistir e mostra o erro de carregamento', () => {
    pedidoService.obter.mockReturnValue(throwError(() => new Error('x')));
    criar();

    componente.excluir();
    componente.alterarStatus('Entregue');
    fixture.detectChanges();

    expect(pedidoService.excluir).not.toHaveBeenCalled();
    expect(componente.linkWhatsApp()).toBe('');
    expect(fixture.nativeElement.querySelector('[role="alert"]')?.textContent).toContain('Não foi possível');
  });
});
