import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { IlustracaoComponent } from '@components/ilustracao/ilustracao.component';
import { normalizarIlustracao } from '@constants/ilustracoes';
import { ROTAS } from '@constants/rotas';
import { Produto } from '@models/produto.model';
import { NotificacaoService } from '@services/notificacao.service';
import { SacolaService } from '@services/sacola.service';

@Component({
  selector: 'eirm-card-produto',
  imports: [CurrencyPipe, RouterLink, IlustracaoComponent],
  templateUrl: './card-produto.component.html',
  styleUrl: './card-produto.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CardProdutoComponent {
  private readonly sacola = inject(SacolaService);
  private readonly notificacao = inject(NotificacaoService);

  readonly produto = input.required<Produto>();

  readonly rotaDoProduto = computed(() => ['/', ROTAS.PRODUTO, this.produto().slug]);
  readonly sabor = computed(() => normalizarIlustracao(this.produto().ilustracao));
  readonly quantidadeNaSacola = computed(() => this.sacola.quantidadeDe(this.produto().id));

  adicionar(): void {
    const produto = this.produto();
    this.sacola.adicionar(produto);
    this.notificacao
      .sucesso(`${produto.nome} foi para a sacola!`, 'Ver sacola')
      .subscribe(() => this.sacola.abrir());
  }
}
