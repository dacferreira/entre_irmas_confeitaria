import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '@environments/environment';
import { Observable } from 'rxjs';

export type ParametrosApi = Record<string, string | number | boolean | null | undefined>;

/** Base de todas as chamadas HTTP: monta a URL a partir de environment.apiUrl. */
@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly urlBase = environment.apiUrl.replace(/\/+$/, '');

  get<T>(caminho: string, parametros: ParametrosApi = {}): Observable<T> {
    return this.http.get<T>(this.url(caminho), { params: this.montarParametros(parametros) });
  }

  post<T>(caminho: string, corpo: unknown): Observable<T> {
    return this.http.post<T>(this.url(caminho), corpo);
  }

  private url(caminho: string): string {
    return `${this.urlBase}/${caminho.replace(/^\/+/, '')}`;
  }

  private montarParametros(parametros: ParametrosApi): HttpParams {
    let params = new HttpParams();
    for (const [chave, valor] of Object.entries(parametros)) {
      if (valor !== null && valor !== undefined && valor !== '') {
        params = params.set(chave, String(valor));
      }
    }
    return params;
  }
}
