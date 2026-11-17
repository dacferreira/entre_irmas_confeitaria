import { CurrencyPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { AbstractControl, FormControl, FormGroup, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Router, RouterLink } from '@angular/router';
import { PageComponent } from '@components/page/page.component';
import { ROTAS } from '@constants/rotas';
import { AcompanhamentoPedido, StatusPedido, TipoEntrega, nomeDoParceiro } from '@models/pedido.model';
import { DataBrPipe } from '@pipes/data-br.pipe';
import { ContatoService } from '@services/contato.service';
import { PedidoService, normalizarCodigoPedido } from '@services/pedido.service';
import { EMPTY, Subject, catchError, map, merge, of, switchMap } from 'rxjs';

export const MENSAGEM_CODIGO_INVALIDO = 'Digite o código no formato EI-XXXXXX.';
export const MENSAGEM_PEDIDO_NAO_ENCONTRADO = 'Não encontramos um pedido com esse código. Confira se digitou certinho.';
export const MENSAGEM_MUITAS_CONSULTAS = 'Muitas consultas em pouco tempo. Aguarde um minuto e tente de novo.';
export const MENSAGEM_CONSULTA_INDISPONIVEL = 'Não foi possível consultar agora. Tente novamente em instantes.';

/** Etapas na ordem do fluxo; o cancelamento fica fora da linha do tempo. */
const FLUXO: readonly StatusPedido[] = ['Recebido', 'Confirmado', 'EmPreparo', 'Pronto', 'Entregue'];

export type EstadoEtapa = 'concluida' | 'atual' | 'futura';

export interface Etapa {
  status: StatusPedido;
  titulo: string;
  texto: string;
  estado: EstadoEtapa;
}

/** Linha do tempo do pedido, com os textos de "pronto" e "entregue" conforme a forma de receber. */
export function etapasDoPedido(status: StatusPedido, tipoEntrega: TipoEntrega): Etapa[] {
  const entrega = tipoEntrega === 'Entrega';
  const textos: Record<StatusPedido, [string, string]> = {
    Recebido: ['Pedido recebido', 'Aguardando a nossa confirmação pelo WhatsApp.'],
    Confirmado: ['Confirmado', 'Pagamento e data combinados com a gente.'],
    EmPreparo: ['Em preparo', 'Seus doces estão sendo feitos com carinho.'],
    Pronto: entrega
      ? ['Pronto para entrega', 'Já vai sair para o endereço combinado.']
      : ['Pronto para retirar', 'Já pode buscar na nossa loja.'],
    Entregue: [entrega ? 'Entregue' : 'Retirado', 'Bom apetite!'],
    Cancelado: ['Cancelado', ''],
  };
  const atual = FLUXO.indexOf(status);

  return FLUXO.map((etapa, i) => ({
    status: etapa,
    titulo: textos[etapa][0],
    texto: textos[etapa][1],
    // Depois de entregue não há próxima etapa: todas ficam concluídas.
    estado: i < atual || (i === atual && etapa === 'Entregue') ? 'concluida' : i === atual ? 'atual' : 'futura',
  }));
}

export function mensagemDeErroDaConsulta(erro: unknown): string {
  const status = erro instanceof HttpErrorResponse ? erro.status : 0;
  switch (status) {
    case 404:
      return MENSAGEM_PEDIDO_NAO_ENCONTRADO;
    case 400:
      return MENSAGEM_CODIGO_INVALIDO;
    case 429:
      return MENSAGEM_MUITAS_CONSULTAS;
    default:
      return MENSAGEM_CONSULTA_INDISPONIVEL;
  }
}

const codigoValidator = (controle: AbstractControl<string>): ValidationErrors | null =>
  controle.value && !normalizarCodigoPedido(controle.value) ? { codigo: true } : null;

type Resultado = { pedido: AcompanhamentoPedido } | { erro: string };

/**
 * Consulta da etapa do pedido pelo código. Com o código na rota (/acompanhar-pedido/EI-XXXXXX), consulta direto;
 * os pedidos feitos ou consultados neste aparelho aparecem para um clique.
 */
@Component({
  selector: 'eirm-acompanhar-pedido',
  imports: [CurrencyPipe, ReactiveFormsModule, RouterLink, MatFormFieldModule, MatInputModule, PageComponent, DataBrPipe],
  templateUrl: './acompanhar-pedido.component.html',
  styleUrl: './acompanhar-pedido.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AcompanharPedidoComponent {
  private readonly pedidoService = inject(PedidoService);
  readonly contato = inject(ContatoService);
  private readonly router = inject(Router);

  /** Código vindo da rota. */
  readonly codigo = input<string>();

  readonly rotas = ROTAS;
  readonly nomeDoParceiro = nomeDoParceiro;
  readonly campoCodigo = new FormControl('', { nonNullable: true, validators: [Validators.required, codigoValidator] });
  // O [formGroup] no <form> faz o Enter disparar o ngSubmit em vez de recarregar a página.
  readonly busca = new FormGroup({ codigo: this.campoCodigo });
  readonly carregando = signal(false);
  readonly erro = signal('');
  readonly pedido = signal<AcompanhamentoPedido | null>(null);

  readonly cancelado = computed(() => this.pedido()?.status === 'Cancelado');
  readonly etapas = computed(() => {
    const pedido = this.pedido();
    return pedido ? etapasDoPedido(pedido.status, pedido.tipoEntrega) : [];
  });
  readonly etapaAtual = computed(() => {
    const pedido = this.pedido();
    return pedido ? this.etapas().find((e) => e.status === pedido.status)?.titulo ?? 'Cancelado' : '';
  });
  readonly recentes = computed(() => {
    const atual = this.pedido()?.codigo;
    return this.pedidoService.pedidosRecentes().filter((c) => c !== atual);
  });
  readonly linkWhatsApp = computed(() =>
    this.contato.linkWhatsApp(`Olá! Tenho uma dúvida sobre o pedido ${this.pedido()?.codigo ?? ''}.`),
  );

  private readonly recarregar$ = new Subject<void>();

  constructor() {
    merge(
      toObservable(this.codigo).pipe(map((codigo) => ({ codigo, novo: true }))),
      this.recarregar$.pipe(map(() => ({ codigo: this.codigo(), novo: false }))),
    )
      .pipe(
        // switchMap: trocar de código no meio da consulta descarta a resposta anterior.
        switchMap(({ codigo, novo }) => {
          if (novo) {
            this.pedido.set(null);
          }
          this.erro.set('');
          if (!codigo) {
            return EMPTY;
          }

          const normalizado = normalizarCodigoPedido(codigo);
          this.campoCodigo.setValue(normalizado ?? codigo);
          if (!normalizado) {
            this.erro.set(MENSAGEM_CODIGO_INVALIDO);
            return EMPTY;
          }

          this.carregando.set(true);
          return this.pedidoService.acompanhar(normalizado).pipe(
            map((pedido): Resultado => ({ pedido })),
            catchError((erro: unknown) => of<Resultado>({ erro: mensagemDeErroDaConsulta(erro) })),
          );
        }),
        takeUntilDestroyed(),
      )
      .subscribe((resultado) => {
        this.carregando.set(false);
        if ('erro' in resultado) {
          this.erro.set(resultado.erro);
          return;
        }
        this.pedido.set(resultado.pedido);
      });
  }

  consultar(): void {
    const codigo = normalizarCodigoPedido(this.campoCodigo.value);
    if (!codigo) {
      this.campoCodigo.markAsTouched();
      return;
    }
    // O mesmo código da rota não muda a URL: consulta de novo.
    if (codigo === normalizarCodigoPedido(this.codigo())) {
      this.atualizar();
      return;
    }
    void this.router.navigate(['/', ROTAS.ACOMPANHAR_PEDIDO, codigo]);
  }

  atualizar(): void {
    this.recarregar$.next();
  }
}
