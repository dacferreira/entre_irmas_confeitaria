import { TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { dataIso } from '@constants/admin';
import { Lancamento } from '@models/admin.model';
import { LancamentoService } from '@services/admin.service';
import { NotificacaoService } from '@services/notificacao.service';
import { criarLancamento, criarNotificacaoFalsa } from '@testing/dados-admin';
import { of } from 'rxjs';
import { LancamentoFormComponent } from './lancamento-form.component';

describe('LancamentoFormComponent', () => {
  const lancamentoService = { criar: jest.fn(), atualizar: jest.fn() };
  const referencia = { close: jest.fn() };
  let notificacao: ReturnType<typeof criarNotificacaoFalsa>;

  function criar(lancamento: Lancamento | null): LancamentoFormComponent {
    notificacao = criarNotificacaoFalsa();
    TestBed.configureTestingModule({
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: lancamento },
        { provide: MatDialogRef, useValue: referencia },
        { provide: LancamentoService, useValue: lancamentoService },
        { provide: NotificacaoService, useValue: notificacao },
      ],
    });
    const fixture = TestBed.createComponent(LancamentoFormComponent);
    fixture.detectChanges();
    return fixture.componentInstance;
  }

  it('novo lançamento começa como despesa de hoje e exige descrição e valor', () => {
    lancamentoService.criar.mockReturnValue(of(criarLancamento()));
    const componente = criar(null);

    expect(componente.formulario.getRawValue()).toEqual({ tipo: 'Despesa', descricao: '', valor: 0, data: dataIso(new Date()) });
    componente.salvar();
    expect(lancamentoService.criar).not.toHaveBeenCalled();

    componente.formulario.patchValue({ descricao: ' Gás ', valor: 120 });
    componente.salvar();
    expect(lancamentoService.criar).toHaveBeenCalledWith(expect.objectContaining({ descricao: 'Gás', valor: 120 }));
    expect(notificacao.sucesso).toHaveBeenCalledWith('Despesa lançada.');
  });

  it('edita o lançamento existente e cancela', () => {
    lancamentoService.atualizar.mockReturnValue(of(criarLancamento({ tipo: 'Perda' })));
    const componente = criar(criarLancamento());

    componente.formulario.controls.tipo.setValue('Perda');
    componente.salvar();
    componente.cancelar();

    expect(lancamentoService.atualizar).toHaveBeenCalledWith(3, expect.objectContaining({ tipo: 'Perda' }));
    expect(notificacao.sucesso).toHaveBeenCalledWith('Perda atualizada.');
    expect(referencia.close).toHaveBeenLastCalledWith();
  });
});
