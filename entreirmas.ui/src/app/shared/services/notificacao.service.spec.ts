import { TestBed } from '@angular/core/testing';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Subject } from 'rxjs';
import { NotificacaoService } from './notificacao.service';

describe('NotificacaoService', () => {
  const acao$ = new Subject<void>();
  const snackBar = { open: jest.fn(() => ({ onAction: () => acao$.asObservable() })) };
  let servico: NotificacaoService;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [{ provide: MatSnackBar, useValue: snackBar }] });
    servico = TestBed.inject(NotificacaoService);
  });

  it('mostra sucesso e emite quando a ação é clicada', () => {
    const aoClicar = jest.fn();

    servico.sucesso('Foi para a sacola!', 'Ver sacola').subscribe(aoClicar);
    acao$.next();

    expect(snackBar.open).toHaveBeenCalledWith('Foi para a sacola!', 'Ver sacola', expect.any(Object));
    expect(aoClicar).toHaveBeenCalledTimes(1);
  });

  it('mostra erro com a classe de erro', () => {
    servico.erro('Falhou');

    expect(snackBar.open).toHaveBeenCalledWith(
      'Falhou',
      'Fechar',
      expect.objectContaining({ panelClass: ['eirm-snack', 'eirm-snack--erro'] }),
    );
  });
});
