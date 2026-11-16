import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService } from '@services/api.service';
import { AuthService } from '@services/auth.service';
import { catchError, throwError } from 'rxjs';

const ROTA_LOGIN = 'autenticacao/login';

/**
 * Envia o token nas chamadas à API. Se a API responder 401 (token vencido ou revogado),
 * encerra a sessão e leva para o login, voltando depois para a tela em que a pessoa estava.
 */
export const authInterceptor: HttpInterceptorFn = (requisicao, next) => {
  const auth = inject(AuthService);
  const api = inject(ApiService);
  const router = inject(Router);

  const token = auth.token();
  const daApi = api.ehDaApi(requisicao.url);
  const enviada = token && daApi ? requisicao.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : requisicao;

  return next(enviada).pipe(
    catchError((erro: unknown) => {
      const ehLogin = requisicao.url.endsWith(ROTA_LOGIN);
      if (daApi && !ehLogin && erro instanceof HttpErrorResponse && erro.status === 401) {
        auth.sair(router.url);
      }
      return throwError(() => erro);
    }),
  );
};
