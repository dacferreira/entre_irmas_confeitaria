import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { periodoDoMes } from '@constants/admin';
import { LancamentoService } from '@services/admin.service';
import { NotificacaoService } from '@services/notificacao.service';
import { criarDialogoFalso, criarLancamento, criarNotificacaoFalsa } from '@testing/dados-admin';
import { of, throwError } from 'rxjs';
import { LancamentosComponent } from './lancamentos.component';

describe('LancamentosComponent', () => {
  const lancamentoService = { listar: jest.fn(), excluir: jest.fn() };
  const chocolate = criarLancamento();
  const descarte = criarLancamento({ id: 4, tipo: 'Perda', descricao: 'Bolo que caiu', valor: 45 });
  let dialogo: ReturnType<typeof criarDialogoFalso>;
  let fixture: ComponentFixture<LancamentosComponent>;
  let componente: LancamentosComponent;

  function criar(): void {
    dialogo = criarDialogoFalso(true);
    TestBed.configureTestingModule({
      providers: [
        { provide: LancamentoService, useValue: lancamentoService },
        { provide: NotificacaoService, useValue: criarNotificacaoFalsa() },
        { provide: MatDialog, useValue: dialogo },
      ],
    });
    fixture = TestBed.createComponent(LancamentosComponent);
    componente = fixture.componentInstance;
    fixture.detectChanges();
  }

  beforeEach(() => lancamentoService.listar.mockReturnValue(of([chocolate, descarte])));

  it('busca o mês corrente e soma despesas e perdas', () => {
    criar();

    expect(lancamentoService.listar).toHaveBeenCalledWith({ ...periodoDoMes(new Date()), tipo: null });
    expect(componente.totais()).toEqual({ despesas: 80, perdas: 45 });
    expect(fixture.nativeElement.querySelectorAll('tbody tr').length).toBe(2);
  });

  it('filtra pelo tipo e recarrega depois de salvar no formulário', () => {
    criar();

    componente.alterarFiltro({ tipo: 'Perda' });
    fixture.detectChanges();
    componente.abrirFormulario(chocolate);
    fixture.detectChanges();

    expect(lancamentoService.listar).toHaveBeenCalledWith(expect.objectContaining({ tipo: 'Perda' }));
    expect(dialogo.open.mock.calls[0][1].data).toBe(chocolate);
    expect(lancamentoService.listar).toHaveBeenCalledTimes(3);
  });

  it('exclui depois de confirmar', () => {
    lancamentoService.excluir.mockReturnValue(of(undefined));
    criar();

    componente.excluir(chocolate);

    expect(lancamentoService.excluir).toHaveBeenCalledWith(3);
    expect(componente.lancamentos()?.map((l) => l.id)).toEqual([4]);
  });

  it('mostra o erro e o estado vazio', () => {
    lancamentoService.listar.mockReturnValue(throwError(() => new Error('x')));
    criar();
    expect(fixture.nativeElement.querySelector('[role="alert"]')).not.toBeNull();

    lancamentoService.listar.mockReturnValue(of([]));
    componente.recarregar();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.admin-vazio')?.textContent).toContain('Nenhum lançamento');
  });
});
