import { TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material/dialog';
import { of } from 'rxjs';
import { ConfirmacaoComponent, confirmar } from './confirmacao.component';

describe('ConfirmacaoComponent', () => {
  const referencia = { close: jest.fn() };

  function criar(perigo: boolean): HTMLElement {
    TestBed.configureTestingModule({
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: { titulo: 'Excluir?', mensagem: 'Não tem volta.', perigo } },
        { provide: MatDialogRef, useValue: referencia },
      ],
    });
    const fixture = TestBed.createComponent(ConfirmacaoComponent);
    fixture.detectChanges();
    return fixture.nativeElement;
  }

  it('mostra título e mensagem e fecha com a resposta', () => {
    const elemento = criar(true);
    const [voltar, confirmarBotao] = Array.from(elemento.querySelectorAll('button'));

    expect(elemento.querySelector('h2')?.textContent).toContain('Excluir?');
    expect(confirmarBotao.textContent).toContain('Confirmar');
    expect(confirmarBotao.classList).toContain('botao--perigo');

    voltar.click();
    expect(referencia.close).toHaveBeenLastCalledWith(false);
    confirmarBotao.click();
    expect(referencia.close).toHaveBeenLastCalledWith(true);
  });

  it('confirmar() só emite true quando a pessoa confirma', () => {
    const dialogo = { open: jest.fn(() => ({ afterClosed: () => of(undefined) })) } as unknown as MatDialog;
    let resposta: boolean | undefined;

    confirmar(dialogo, { titulo: 't', mensagem: 'm' }).subscribe((r) => (resposta = r));

    expect(resposta).toBe(false);
    expect((dialogo.open as jest.Mock).mock.calls[0][1].data).toEqual({ titulo: 't', mensagem: 'm' });
  });
});
