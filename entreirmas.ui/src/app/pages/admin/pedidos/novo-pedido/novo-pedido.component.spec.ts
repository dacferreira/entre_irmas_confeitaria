import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialogRef } from '@angular/material/dialog';
import { PedidoAdminService, ProdutoAdminService } from '@services/admin.service';
import { EnderecoService } from '@services/endereco.service';
import { NotificacaoService } from '@services/notificacao.service';
import { criarNotificacaoFalsa, criarPedidoDetalhe, criarProdutoAdmin } from '@testing/dados-admin';
import { ENDERECO_DIGITADO, ENDERECO_ENVIADO } from '@testing/dados-de-teste';
import { of } from 'rxjs';
import { NovoPedidoComponent } from './novo-pedido.component';

describe('NovoPedidoComponent', () => {
  const pedidoService = { criar: jest.fn() };
  const produtoService = { listar: jest.fn() };
  const referencia = { close: jest.fn() };
  const brigadeiro = criarProdutoAdmin({ id: 20, nome: 'Brigadeiro', preco: 2.5, quantidadeMinima: 25 });
  let fixture: ComponentFixture<NovoPedidoComponent>;
  let componente: NovoPedidoComponent;

  beforeEach(() => {
    produtoService.listar.mockReturnValue(of([criarProdutoAdmin(), brigadeiro]));
    TestBed.configureTestingModule({
      providers: [
        { provide: MatDialogRef, useValue: referencia },
        { provide: PedidoAdminService, useValue: pedidoService },
        { provide: ProdutoAdminService, useValue: produtoService },
        { provide: NotificacaoService, useValue: criarNotificacaoFalsa() },
        { provide: EnderecoService, useValue: { consultarCep: jest.fn() } },
      ],
    });
    fixture = TestBed.createComponent(NovoPedidoComponent);
    componente = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('lista só produtos ativos e sugere o mínimo ao escolher', () => {
    const item = componente.formulario.controls.itens.at(0);

    item.controls.produtoId.setValue(20);
    componente.aoEscolherProduto(0);

    expect(produtoService.listar).toHaveBeenCalledWith({ ativo: true });
    expect(item.controls.quantidade.value).toBe(25);
    expect(componente.total()).toBe(62.5);
  });

  it('adiciona e remove itens, mantendo pelo menos um', () => {
    componente.adicionarItem();
    expect(componente.formulario.controls.itens.length).toBe(2);

    componente.removerItem(1);
    componente.removerItem(0);
    expect(componente.formulario.controls.itens.length).toBe(1);
  });

  it('exige endereço para entrega e não envia inválido', () => {
    componente.formulario.patchValue({ nomeCliente: 'Ana', telefone: '(21) 98888-7777', tipoEntrega: 'Entrega' });
    componente.formulario.controls.itens.at(0).patchValue({ produtoId: 10, quantidade: 1 });

    componente.salvar();

    expect(componente.formulario.controls.endereco.controls.cep.hasError('required')).toBe(true);
    expect(pedidoService.criar).not.toHaveBeenCalled();
  });

  it('na entrega envia o endereço em partes', () => {
    pedidoService.criar.mockReturnValue(of(criarPedidoDetalhe()));
    componente.formulario.patchValue({ nomeCliente: 'Ana', telefone: '(21) 98888-7777', tipoEntrega: 'Entrega' });
    componente.formulario.controls.endereco.setValue(ENDERECO_DIGITADO);
    componente.formulario.controls.itens.at(0).patchValue({ produtoId: 10, quantidade: 1 });

    componente.salvar();

    expect(pedidoService.criar).toHaveBeenCalledWith(expect.objectContaining({ tipoEntrega: 'Entrega', endereco: ENDERECO_ENVIADO }));
  });

  it('envia o pedido e fecha com o pedido criado', () => {
    pedidoService.criar.mockReturnValue(of(criarPedidoDetalhe()));
    componente.formulario.patchValue({ nomeCliente: ' Ana ', telefone: '(21) 98888-7777', observacao: ' ' });
    componente.formulario.controls.itens.at(0).patchValue({ produtoId: 10, quantidade: 2 });

    componente.salvar();

    expect(pedidoService.criar).toHaveBeenCalledWith(
      expect.objectContaining({
        nomeCliente: 'Ana',
        tipoEntrega: 'Retirada',
        endereco: null,
        observacao: null,
        itens: [{ produtoId: 10, quantidade: 2 }],
      }),
    );
    expect(referencia.close).toHaveBeenCalledWith(expect.objectContaining({ codigo: 'EI-ABC234' }));
  });

  it('cancela sem salvar', () => {
    componente.cancelar();

    expect(referencia.close).toHaveBeenCalledWith();
  });
});
