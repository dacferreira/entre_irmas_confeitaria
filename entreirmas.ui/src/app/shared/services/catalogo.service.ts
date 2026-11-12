import { Injectable, inject } from '@angular/core';
import { Categoria } from '@models/categoria.model';
import { FiltroProduto, Produto } from '@models/produto.model';
import { Observable, catchError, shareReplay, throwError } from 'rxjs';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class CatalogoService {
  private readonly api = inject(ApiService);

  // As categorias mudam pouco: uma chamada por visita.
  private categorias$?: Observable<Categoria[]>;

  listarCategorias(): Observable<Categoria[]> {
    if (!this.categorias$) {
      this.categorias$ = this.api.get<Categoria[]>('categorias').pipe(
        catchError((erro: unknown) => {
          this.categorias$ = undefined;
          return throwError(() => erro);
        }),
        shareReplay({ bufferSize: 1, refCount: false }),
      );
    }
    return this.categorias$;
  }

  listarProdutos(filtro: FiltroProduto = {}): Observable<Produto[]> {
    return this.api.get<Produto[]>('produtos', { ...filtro });
  }

  obterProduto(slug: string): Observable<Produto> {
    return this.api.get<Produto>(`produtos/${encodeURIComponent(slug)}`);
  }
}
