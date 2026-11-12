import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { NovoPedido } from '@models/pedido.model';
import { PedidoService } from '@services/pedido.service';
import { SacolaService } from '@services/sacola.service';
import { BRIGADEIRO, criarPedido, criarProduto } from '@testing/dados-de-teste';
import { Subject, of, throwError } from 'rxjs';
import { FinalizarPedidoComponent } from './finalizar-pedido.component';

describe('FinalizarPedidoComponent', () => {
  const pedidoService = { criar: jest.fn() };
  let fixture: ComponentFixture<FinalizarPedidoComponent>;
  let componente: FinalizarPedidoComponent;
  let sacola: SacolaService;
  let navegar: jest.SpyInstance;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideRouter([]), { provide: PedidoService, useValue: pedidoService }],
    });
    sacola = TestBed.inject(SacolaService);
    sacola.adicionar(criarProduto(), 2);
    sacola.adicionar(BRIGADEIRO);
    navegar = jest.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    fixture = TestBed.createComponent(FinalizarPedidoComponent);
    componente = fixture.componentInstance;
    fixture.detectChanges();
  });

  function preencher(): void {
    componente.formulario.patchValue({ nomeCliente: '  Maria ', telefone: '(21) 99999-8888', observacao: ' ' });
  }

  it('mostra o resumo da sacola e começa com a data mínima', () => {
    const elemento: HTMLElement = fixture.nativeElement;

    expect(elemento.querySelectorAll('.resumo__item').length).toBe(2);
    expect(componente.formulario.controls.dataDesejada.value).toBe(componente.dataMinima);
  });

  it('não envia com dados inválidos e marca os campos', () => {
    componente.enviar();
    fixture.detectChanges();

    expect(pedidoService.criar).not.toHaveBeenCalled();
    expect(componente.formulario.controls.nomeCliente.touched).toBe(true);
    expect(fixture.nativeElement.querySelectorAll('mat-error').length).toBeGreaterThan(0);
  });

  it('exige endereço só para entrega e mostra o campo', () => {
    preencher();
    componente.formulario.controls.tipoEntrega.setValue('Entrega');
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('textarea[formControlName="endereco"]')).not.toBeNull();
    expect(componente.formulario.controls.endereco.hasError('required')).toBe(true);

    componente.formulario.controls.tipoEntrega.setValue('Retirada');
    expect(componente.formulario.controls.endereco.valid).toBe(true);
  });

  it('envia o pedido montado, limpa a sacola e vai para a confirmação', () => {
    pedidoService.criar.mockReturnValue(of(criarPedido()));
    preencher();
    componente.formulario.patchValue({ tipoEntrega: 'Entrega', endereco: ' Rua A, 10 ' });

    componente.enviar();

    const enviado: NovoPedido = pedidoService.criar.mock.calls[0][0];
    expect(enviado).toEqual({
      nomeCliente: 'Maria',
      telefone: '21999998888',
      tipoEntrega: 'Entrega',
      endereco: 'Rua A, 10',
      dataDesejada: componente.dataMinima,
      observacao: null,
      itens: [
        { produtoId: 1, quantidade: 2 },
        { produtoId: 2, quantidade: 25 },
      ],
    });
    expect(sacola.vazia()).toBe(true);
    expect(navegar).toHaveBeenCalledWith(['/', 'pedido-confirmado']);
    expect(componente.enviando()).toBe(false);
  });

  it('mantém a sacola quando o envio falha e bloqueia envio duplo', () => {
    const resposta$ = new Subject<never>();
    pedidoService.criar.mockReturnValue(resposta$);
    preencher();

    componente.enviar();
    componente.enviar();
    fixture.detectChanges();
    expect(pedidoService.criar).toHaveBeenCalledTimes(1);
    expect(fixture.nativeElement.querySelector('.enviar')?.textContent).toContain('Enviando');

    resposta$.error(new Error('falhou'));
    expect(componente.enviando()).toBe(false);
    expect(sacola.vazia()).toBe(false);
  });

  it('abre a sacola para editar', () => {
    fixture.nativeElement.querySelector('.resumo__editar').click();

    expect(sacola.aberta()).toBe(true);
  });

  it('não envia se a sacola esvaziou', () => {
    pedidoService.criar.mockReturnValue(throwError(() => new Error('x')));
    preencher();
    sacola.limpar();

    componente.enviar();

    expect(pedidoService.criar).not.toHaveBeenCalled();
  });
});
