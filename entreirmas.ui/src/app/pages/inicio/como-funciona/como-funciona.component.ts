import { ChangeDetectionStrategy, Component } from '@angular/core';
import { environment } from '@environments/environment';

interface Passo {
  titulo: string;
  texto: string;
}

/** Passo a passo do pedido (âncora #como-funciona). */
@Component({
  selector: 'eirm-como-funciona',
  templateUrl: './como-funciona.component.html',
  styleUrl: './como-funciona.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ComoFuncionaComponent {
  readonly passos: Passo[] = [
    {
      titulo: 'Monte sua sacola',
      texto: 'Escolha os doces no cardápio e ajuste as quantidades. Os docinhos têm pedido mínimo.',
    },
    {
      titulo: 'Envie pelo site',
      texto: `Informe a data desejada (com ${environment.antecedenciaMinimaDias} dia(s) de antecedência) e se prefere retirar ou receber.`,
    },
    {
      titulo: 'Confirme no WhatsApp',
      texto: 'A mensagem do pedido já vai pronta. Lá combinamos pagamento, entrega e qualquer detalhe.',
    },
  ];
}
