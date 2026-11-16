import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '@environments/environment';
import { SEM_AVISO_DE_ERRO } from '@interceptors/erro.interceptor';
import { Observable } from 'rxjs';

export type ParametrosApi = Record<string, string | number | boolean | null | undefined>;

/** Base de todas as chamadas HTTP: monta a URL a partir de environment.apiUrl. */
@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly urlBase = environment.apiUrl.replace(/\/+$/, '');

  /** silencioso: a tela trata o erro e o aviso geral (rodapé) não aparece. */
  get<T>(caminho: string, parametros: ParametrosApi = {}, opcoes: { silencioso?: boolean } = {}): Observable<T> {
    return this.http.get<T>(this.url(caminho), {
      params: this.montarParametros(parametros),
      context: new HttpContext().set(SEM_AVISO_DE_ERRO, !!opcoes.silencioso),
    });
  }

  post<T>(caminho: string, corpo: unknown): Observable<T> {
    return this.http.post<T>(this.url(caminho), corpo);
  }

  put<T>(caminho: string, corpo: unknown): Observable<T> {
    return this.http.put<T>(this.url(caminho), corpo);
  }

  patch<T>(caminho: string, corpo: unknown): Observable<T> {
    return this.http.patch<T>(this.url(caminho), corpo);
  }

  delete<T = void>(caminho: string): Observable<T> {
    return this.http.delete<T>(this.url(caminho));
  }

  /** Envia um arquivo (multipart/form-data) no campo informado. */
  enviarArquivo<T>(caminho: string, campo: string, arquivo: Blob, nomeDoArquivo: string): Observable<T> {
    const formulario = new FormData();
    formulario.append(campo, arquivo, nomeDoArquivo);
    return this.http.put<T>(this.url(caminho), formulario);
  }

  /** Indica se a URL é da API (o token só vai para ela). */
  ehDaApi(url: string): boolean {
    return url.startsWith(`${this.urlBase}/`);
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
