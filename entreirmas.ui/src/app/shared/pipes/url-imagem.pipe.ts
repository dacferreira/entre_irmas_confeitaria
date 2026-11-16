import { Pipe, PipeTransform } from '@angular/core';
import { environment } from '@environments/environment';

const URL_COMPLETA = /^(https?:|data:|blob:)/i;

/**
 * A API guarda a foto como caminho relativo a ela (produtos/10/imagem?v=...), porque no local a API
 * fica em outro endereço (localhost:5083) e nos ambientes no mesmo (/api). Aqui vira a URL completa.
 */
@Pipe({ name: 'urlImagem' })
export class UrlImagemPipe implements PipeTransform {
  transform(caminho: string | null | undefined): string | null {
    if (!caminho) {
      return null;
    }
    return URL_COMPLETA.test(caminho)
      ? caminho
      : `${environment.apiUrl.replace(/\/+$/, '')}/${caminho.replace(/^\/+/, '')}`;
  }
}
