import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { IconeComponent } from '@components/icone/icone.component';
import { ROTAS_ADMIN, STATUS_PEDIDO, dataIso, opcaoStatus, periodoDoMes } from '@constants/admin';
import { Painel } from '@models/admin.model';
import { DataBrPipe } from '@pipes/data-br.pipe';
import { PainelService } from '@services/admin.service';
import { catchError, of, switchMap, tap } from 'rxjs';

type AtalhoDePeriodo = 'mes' | 'mes-passado' | '30-dias' | 'ano' | 'personalizado';

interface Periodo {
  inicio: string;
  fim: string;
}

interface FatiaDaReceita {
  rotulo: string;
  valor: number;
  percentual: number;
  classe: string;
}

@Component({
  selector: 'eirm-painel',
  imports: [CurrencyPipe, DecimalPipe, DataBrPipe, RouterLink, IconeComponent],
  templateUrl: './painel.component.html',
  styleUrl: './painel.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PainelComponent {
  private readonly painelService = inject(PainelService);

  readonly rotas = ROTAS_ADMIN;
  readonly atalhos: readonly { valor: AtalhoDePeriodo; rotulo: string }[] = [
    { valor: 'mes', rotulo: 'Este mês' },
    { valor: 'mes-passado', rotulo: 'Mês passado' },
    { valor: '30-dias', rotulo: 'Últimos 30 dias' },
    { valor: 'ano', rotulo: 'Este ano' },
  ];

  readonly atalho = signal<AtalhoDePeriodo>('mes');
  readonly periodo = signal<Periodo>(periodoDoMes(new Date()));
  readonly painel = signal<Painel | null>(null);
  readonly carregando = signal(true);
  readonly falhou = signal(false);

  /** Altura de cada coluna do gráfico, em % do maior valor do período. */
  readonly colunas = computed(() => {
    const painel = this.painel();
    if (!painel) {
      return [];
    }
    const maior = Math.max(1, ...painel.serie.map((p) => Math.max(p.receita, p.custos)));
    return painel.serie.map((ponto) => ({
      ...ponto,
      alturaReceita: (ponto.receita / maior) * 100,
      alturaCustos: (ponto.custos / maior) * 100,
    }));
  });

  readonly temMovimento = computed(() => {
    const painel = this.painel();
    return !!painel && (painel.receita > 0 || painel.despesas > 0 || painel.perdas > 0);
  });

  /** Para onde vai a receita: custo dos produtos, despesas, perdas e o que sobra de lucro. */
  readonly fatias = computed<FatiaDaReceita[]>(() => {
    const painel = this.painel();
    if (!painel) {
      return [];
    }
    const partes = [
      { rotulo: 'Custo dos produtos', valor: painel.custoProdutos, classe: 'fatia--custo' },
      { rotulo: 'Despesas', valor: painel.despesas, classe: 'fatia--despesa' },
      { rotulo: 'Perdas', valor: painel.perdas, classe: 'fatia--perda' },
      { rotulo: 'Lucro', valor: Math.max(0, painel.lucro), classe: 'fatia--lucro' },
    ];
    const base = Math.max(painel.receita, partes.reduce((soma, p) => soma + p.valor, 0), 1);
    return partes.map((parte) => ({ ...parte, percentual: (parte.valor / base) * 100 }));
  });

  readonly maiorVenda = computed(() =>
    Math.max(1, ...(this.painel()?.produtosMaisVendidos ?? []).map((p) => p.receita)),
  );

  readonly statusComPedidos = computed(() =>
    (this.painel()?.porStatus ?? [])
      .filter((s) => s.quantidade > 0)
      .map((s) => ({ ...s, opcao: opcaoStatus(s.status) })),
  );

  readonly opcoesStatus = STATUS_PEDIDO;

  constructor() {
    toObservable(this.periodo)
      .pipe(
        tap(() => {
          this.carregando.set(true);
          this.falhou.set(false);
        }),
        switchMap(({ inicio, fim }) =>
          this.painelService.obter(inicio, fim).pipe(
            catchError(() => {
              this.falhou.set(true);
              return of(null);
            }),
          ),
        ),
        takeUntilDestroyed(),
      )
      .subscribe((painel) => {
        this.painel.set(painel);
        this.carregando.set(false);
      });
  }

  escolherAtalho(atalho: AtalhoDePeriodo): void {
    const hoje = new Date();
    this.atalho.set(atalho);
    switch (atalho) {
      case 'mes':
        this.periodo.set(periodoDoMes(hoje));
        break;
      case 'mes-passado':
        this.periodo.set(periodoDoMes(new Date(hoje.getFullYear(), hoje.getMonth() - 1, 1)));
        break;
      case '30-dias':
        this.periodo.set({ inicio: dataIso(new Date(hoje.getFullYear(), hoje.getMonth(), hoje.getDate() - 29)), fim: dataIso(hoje) });
        break;
      case 'ano':
        this.periodo.set({ inicio: `${hoje.getFullYear()}-01-01`, fim: `${hoje.getFullYear()}-12-31` });
        break;
    }
  }

  alterarData(campo: keyof Periodo, valor: string): void {
    if (!valor) {
      return;
    }
    const novo = { ...this.periodo(), [campo]: valor };
    if (novo.inicio > novo.fim) {
      // Mantém o período válido: a outra ponta acompanha a data escolhida.
      novo[campo === 'inicio' ? 'fim' : 'inicio'] = valor;
    }
    this.atalho.set('personalizado');
    this.periodo.set(novo);
  }

  recarregar(): void {
    this.periodo.set({ ...this.periodo() });
  }

  /** Rótulo do eixo: dia (12) ou mês (nov). Datas yyyy-MM-dd tratadas como locais. */
  rotuloDoPonto(data: string, agrupamento: Painel['agrupamento']): string {
    const [ano, mes, dia] = data.split('-').map(Number);
    const local = new Date(ano, mes - 1, dia);
    return agrupamento === 'Mes'
      ? local.toLocaleDateString('pt-BR', { month: 'short' }).replace('.', '')
      : String(dia);
  }
}
