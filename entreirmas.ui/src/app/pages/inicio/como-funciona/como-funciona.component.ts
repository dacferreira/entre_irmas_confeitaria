import { ChangeDetectionStrategy, Component } from '@angular/core';
import { environment } from '@environments/environment';

interface Passo {
  titulo: string;
  texto: string;
}

interface Fluxo {
  titulo: string;
  subtitulo: string;
  passos: Passo[];
}

const ANTECEDENCIA = environment.antecedenciaMinimaDias;

/** Os dois jeitos de pedir: pedido normal (pronta entrega, para hoje) e encomenda (data marcada). Âncora #como-funciona. */
@Component({
  selector: 'eirm-como-funciona',
  templateUrl: './como-funciona.component.html',
  styleUrl: './como-funciona.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ComoFuncionaComponent {
  readonly fluxos: Fluxo[] = [
    {
      titulo: 'Pedido normal',
      subtitulo: 'Doces de pronta entrega, para hoje',
      passos: [
        { titulo: 'Peça', texto: 'Monte a sacola com os doces de pronta entrega e envie pelo site.' },
        { titulo: 'Pague', texto: 'A mensagem do pedido já vai pronta no WhatsApp; lá combinamos o pagamento.' },
        { titulo: 'Retire ou receba', texto: 'Avisamos quando estiver pronto. Acompanhe a etapa em Meu pedido.' },
      ],
    },
    {
      titulo: 'Encomenda',
      subtitulo: `Doces sob encomenda, com ${ANTECEDENCIA} dia(s) de antecedência`,
      passos: [
        {
          titulo: 'Peça para a data',
          texto: `Escolha os doces sob encomenda e o dia, a partir de ${ANTECEDENCIA} dia(s) depois do pedido.`,
        },
        { titulo: 'Pague', texto: 'Combinamos o pagamento pelo WhatsApp; a encomenda entra na produção depois dele.' },
        { titulo: 'Retire ou receba', texto: 'No dia marcado, do jeito que você preferir.' },
      ],
    },
  ];
}
