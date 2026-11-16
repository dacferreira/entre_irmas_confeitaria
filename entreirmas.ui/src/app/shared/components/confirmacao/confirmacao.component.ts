import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material/dialog';
import { Observable, map } from 'rxjs';

export interface DadosConfirmacao {
  titulo: string;
  mensagem: string;
  /** Texto do botão de confirmação (padrão: "Confirmar"). */
  confirmar?: string;
  /** Botão vermelho, para ações que apagam algo. */
  perigo?: boolean;
}

/** Diálogo "tem certeza?" das ações que não têm volta. */
@Component({
  selector: 'eirm-confirmacao',
  template: `
    <div class="dialogo" role="alertdialog" aria-labelledby="confirmacao-titulo" aria-describedby="confirmacao-mensagem">
      <div class="dialogo__topo">
        <h2 id="confirmacao-titulo">{{ dados.titulo }}</h2>
      </div>
      <div class="dialogo__corpo">
        <p id="confirmacao-mensagem">{{ dados.mensagem }}</p>
      </div>
      <div class="dialogo__rodape">
        <button type="button" class="botao botao--contorno botao--pequeno" (click)="fechar(false)">Voltar</button>
        <button
          type="button"
          class="botao botao--pequeno"
          [class.botao--perigo]="dados.perigo"
          cdkFocusInitial
          (click)="fechar(true)">
          {{ dados.confirmar ?? 'Confirmar' }}
        </button>
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfirmacaoComponent {
  readonly dados = inject<DadosConfirmacao>(MAT_DIALOG_DATA);
  private readonly referencia = inject<MatDialogRef<ConfirmacaoComponent, boolean>>(MatDialogRef);

  fechar(confirmado: boolean): void {
    this.referencia.close(confirmado);
  }
}

/** Abre a confirmação e emite true só se a pessoa confirmar. */
export function confirmar(dialogo: MatDialog, dados: DadosConfirmacao): Observable<boolean> {
  return dialogo
    .open<ConfirmacaoComponent, DadosConfirmacao, boolean>(ConfirmacaoComponent, {
      data: dados,
      width: '440px',
      maxWidth: 'calc(100vw - 32px)',
      panelClass: 'eirm-dialogo',
      autoFocus: 'dialog',
    })
    .afterClosed()
    .pipe(map((resposta) => resposta === true));
}
