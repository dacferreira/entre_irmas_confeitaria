import { signal } from '@angular/core';
import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { PedidoAdminService } from '@services/admin.service';
import { AlertaPedidosService } from '@services/alerta-pedidos.service';
import { criarDialogoFalso, criarPedidoResumo } from '@testing/dados-admin';
import { of, throwError } from 'rxjs';
import { PedidosComponent } from './pedidos.component';

describe('PedidosComponent', () => {
  const pedidoService = { listar: jest.fn() };
  const novosPedidos = signal(0);
  let dialogo: ReturnType<typeof criarDialogoFalso>;
  let fixture: ComponentFixture<PedidosComponent>;
  let componente: PedidosComponent;

  function pagina(total = 1): unknown {
    return of({ itens: [criarPedidoResumo()], total, pagina: 1, tamanhoPagina: 20 });
  }

  beforeEach(() => {
    dialogo = criarDialogoFalso(true);
    pedidoService.listar.mockReturnValue(pagina(45));
    TestBed.configureTestingModule({
      providers: [
        { provide: PedidoAdminService, useValue: pedidoService },
        { provide: MatDialog, useValue: dialogo },
        { provide: AlertaPedidosService, useValue: { novosPedidos } },
      ],
    });
    fixture = TestBed.createComponent(PedidosComponent);
    componente = fixture.componentInstance;
  });

  function avancar(): void {
    fixture.detectChanges();
    tick(300);
    fixture.detectChanges();
  }

  function iniciar(status = ''): void {
    fixture.componentRef.setInput('status', status);
    avancar();
  }

  it('lista os pedidos com o filtro de status vindo da URL', fakeAsync(() => {
    iniciar('Recebido');
    const elemento: HTMLElement = fixture.nativeElement;

    expect(pedidoService.listar).toHaveBeenLastCalledWith(expect.objectContaining({ status: 'Recebido', pagina: 1, tamanhoPagina: 20 }));
    expect(elemento.querySelector('tbody .principal')?.textContent).toContain('Maria Souza');
    expect(elemento.querySelector('.selo')?.textContent).toContain('Aguardando confirmação');
    expect(componente.totalDePaginas()).toBe(3);
  }));

  it('pagina e volta para a primeira página ao filtrar', fakeAsync(() => {
    iniciar();

    componente.irParaPagina(2);
    avancar();
    expect(pedidoService.listar).toHaveBeenLastCalledWith(expect.objectContaining({ pagina: 2 }));

    componente.alterarFiltro({ busca: 'maria' });
    avancar();
    expect(pedidoService.listar).toHaveBeenLastCalledWith(expect.objectContaining({ busca: 'maria', pagina: 1 }));

    componente.limparFiltros();
    avancar();
    expect(componente.filtro().busca).toBe('');
  }));

  it('abre o detalhe e o lançamento manual, recarregando quando algo muda', fakeAsync(() => {
    iniciar();
    const chamadas = pedidoService.listar.mock.calls.length;

    componente.abrir(criarPedidoResumo());
    componente.novo();
    avancar();

    expect(dialogo.open).toHaveBeenCalledTimes(2);
    expect(dialogo.open.mock.calls[0][1].data).toBe(5);
    expect(pedidoService.listar.mock.calls.length).toBeGreaterThan(chamadas);
  }));

  it('mostra o estado vazio e o erro', fakeAsync(() => {
    pedidoService.listar.mockReturnValue(of({ itens: [], total: 0, pagina: 1, tamanhoPagina: 20 }));
    iniciar();
    expect(fixture.nativeElement.querySelector('.admin-vazio')?.textContent).toContain('Nenhum pedido');

    pedidoService.listar.mockReturnValue(throwError(() => new Error('x')));
    componente.alterarFiltro({});
    avancar();
    expect(fixture.nativeElement.querySelector('[role="alert"]')?.textContent).toContain('Não foi possível');
  }));

  it('recarrega a lista quando chega pedido novo pelo site', fakeAsync(() => {
    novosPedidos.set(0);
    fixture.detectChanges();
    tick(300);
    const chamadas = pedidoService.listar.mock.calls.length;

    novosPedidos.set(1);
    fixture.detectChanges();
    tick(300);

    expect(pedidoService.listar.mock.calls.length).toBe(chamadas + 1);
  }));
});
