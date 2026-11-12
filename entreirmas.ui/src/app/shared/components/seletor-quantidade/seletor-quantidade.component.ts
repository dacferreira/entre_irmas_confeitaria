import { ChangeDetectionStrategy, Component, input, model } from '@angular/core';

/** Botões de menos/mais com o número no meio, respeitando o pedido mínimo do produto. */
@Component({
  selector: 'eirm-seletor-quantidade',
  templateUrl: './seletor-quantidade.component.html',
  styleUrl: './seletor-quantidade.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { '[class.compacto]': "tamanho() === 'p'" },
})
export class SeletorQuantidadeComponent {
  readonly valor = model.required<number>();
  readonly minimo = input(1);
  readonly maximo = input(1000);
  readonly rotulo = input('Quantidade');
  readonly tamanho = input<'p' | 'm'>('m');

  diminuir(): void {
    this.definir(this.valor() - 1);
  }

  aumentar(): void {
    this.definir(this.valor() + 1);
  }

  aoDigitar(evento: Event): void {
    const campo = evento.target as HTMLInputElement;
    this.definir(Number(campo.value));
    // Se o valor foi ajustado (ex.: abaixo do mínimo), o campo volta a mostrar o valor válido.
    campo.value = String(this.valor());
  }

  definir(quantidade: number): void {
    const inteira = Number.isFinite(quantidade) ? Math.round(quantidade) : this.minimo();
    this.valor.set(Math.min(this.maximo(), Math.max(this.minimo(), inteira)));
  }
}
