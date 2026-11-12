import { Injectable, inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Observable, map } from 'rxjs';

const DURACAO_MS = 4500;

/** Mensagens rápidas no rodapé da tela (snackbar). */
@Injectable({ providedIn: 'root' })
export class NotificacaoService {
  private readonly snackBar = inject(MatSnackBar);

  /** Exibe a mensagem; o observable emite quando a pessoa clica na ação. */
  sucesso(mensagem: string, acao = 'Ok'): Observable<void> {
    return this.snackBar
      .open(mensagem, acao, { duration: DURACAO_MS, panelClass: ['eirm-snack'] })
      .onAction()
      .pipe(map(() => undefined));
  }

  erro(mensagem: string): void {
    this.snackBar.open(mensagem, 'Fechar', {
      duration: DURACAO_MS + 2000,
      panelClass: ['eirm-snack', 'eirm-snack--erro'],
      politeness: 'assertive',
    });
  }
}
