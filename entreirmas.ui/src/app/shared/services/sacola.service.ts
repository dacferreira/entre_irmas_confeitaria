import { Injectable, computed, signal } from '@angular/core';
import { Produto } from '@models/produto.model';
import { ItemSacola } from '@models/sacola.model';

export const CHAVE_SACOLA = 'eirm.sacola';
export const QUANTIDADE_MAXIMA = 1000;

/** Sacola de compras, guardada no navegador para não se perder ao recarregar a página. */
@Injectable({ providedIn: 'root' })
export class SacolaService {
  private readonly _itens = signal<ItemSacola[]>(this.carregar());

  readonly itens = this._itens.asReadonly();
  readonly aberta = signal(false);
  readonly quantidadeDeItens = computed(() => this._itens().length);
  readonly quantidadeDeUnidades = computed(() => this._itens().reduce((soma, item) => soma + item.quantidade, 0));
  readonly total = computed(() =>
    this._itens().reduce((soma, item) => soma + item.produto.preco * item.quantidade, 0),
  );
  readonly vazia = computed(() => this._itens().length === 0);

  /** Adiciona o produto; se já estiver na sacola, soma a quantidade. */
  adicionar(produto: Produto, quantidade: number = produto.quantidadeMinima): void {
    this.atualizar((itens) => {
      const existente = itens.find((item) => item.produto.id === produto.id);
      if (!existente) {
        return [...itens, { produto, quantidade: this.limitar(quantidade, produto) }];
      }
      return itens.map((item) =>
        item.produto.id === produto.id
          ? { produto, quantidade: this.limitar(item.quantidade + quantidade, produto) }
          : item,
      );
    });
  }

  alterarQuantidade(produtoId: number, quantidade: number): void {
    this.atualizar((itens) =>
      itens.map((item) =>
        item.produto.id === produtoId ? { ...item, quantidade: this.limitar(quantidade, item.produto) } : item,
      ),
    );
  }

  remover(produtoId: number): void {
    this.atualizar((itens) => itens.filter((item) => item.produto.id !== produtoId));
  }

  limpar(): void {
    this.atualizar(() => []);
  }

  quantidadeDe(produtoId: number): number {
    return this._itens().find((item) => item.produto.id === produtoId)?.quantidade ?? 0;
  }

  abrir(): void {
    this.aberta.set(true);
  }

  fechar(): void {
    this.aberta.set(false);
  }

  alternar(): void {
    this.aberta.update((aberta) => !aberta);
  }

  private limitar(quantidade: number, produto: Produto): number {
    const inteira = Number.isFinite(quantidade) ? Math.round(quantidade) : produto.quantidadeMinima;
    return Math.min(QUANTIDADE_MAXIMA, Math.max(produto.quantidadeMinima, inteira));
  }

  private atualizar(alteracao: (itens: ItemSacola[]) => ItemSacola[]): void {
    this._itens.update(alteracao);
    this.salvar(this._itens());
  }

  private carregar(): ItemSacola[] {
    try {
      const lista: unknown = JSON.parse(localStorage.getItem(CHAVE_SACOLA) ?? '[]');
      return Array.isArray(lista) ? lista.filter(itemValido) : [];
    } catch {
      return [];
    }
  }

  private salvar(itens: ItemSacola[]): void {
    try {
      localStorage.setItem(CHAVE_SACOLA, JSON.stringify(itens));
    } catch {
      // Armazenamento indisponível (aba anônima, cota cheia): a sacola segue só em memória.
    }
  }
}

function itemValido(item: unknown): item is ItemSacola {
  const candidato = item as Partial<ItemSacola> | null;
  return (
    typeof candidato?.quantidade === 'number' &&
    typeof candidato.produto?.id === 'number' &&
    typeof candidato.produto.preco === 'number'
  );
}
