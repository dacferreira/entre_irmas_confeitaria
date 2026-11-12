import { HttpErrorResponse, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { ErroApi, ValidationResultApi } from '@models/erro-api.model';
import { NotificacaoService } from '@services/notificacao.service';
import { TimeoutError, catchError, throwError } from 'rxjs';

const MENSAGEM_GENERICA = 'Não foi possível concluir a operação. Tente novamente em instantes.';

/** Mostra uma mensagem amigável para os erros de HTTP e repassa o erro para quem chamou. */
export const erroInterceptor: HttpInterceptorFn = (requisicao, next) => {
  const notificacao = inject(NotificacaoService);

  return next(requisicao).pipe(
    catchError((erro: unknown) => {
      const mensagem = mensagemDeErro(erro, requisicao);
      if (mensagem) {
        notificacao.erro(mensagem);
      }
      return throwError(() => erro);
    }),
  );
};

/** Retorna null quando a própria tela trata o erro (ex.: produto não encontrado). */
export function mensagemDeErro(erro: unknown, requisicao: HttpRequest<unknown>): string | null {
  if (erro instanceof TimeoutError) {
    return 'O servidor demorou para responder. Tente novamente.';
  }
  if (!(erro instanceof HttpErrorResponse)) {
    return MENSAGEM_GENERICA;
  }
  if (erro.status === 0) {
    return 'Sem conexão com o servidor. Confira sua internet e tente de novo.';
  }
  if (erro.status === 404 && requisicao.method === 'GET') {
    return null;
  }
  if (erro.status === 400) {
    const corpo = (erro.error ?? {}) as Partial<ValidationResultApi>;
    const mensagensDosCampos = (corpo.erros ?? []).map((e) => e.mensagem).slice(0, 3);
    return mensagensDosCampos.length > 0
      ? mensagensDosCampos.join(' ')
      : corpo.mensagem ?? 'Verifique os dados informados.';
  }
  if (erro.status < 500) {
    return ((erro.error ?? {}) as Partial<ErroApi>).error ?? MENSAGEM_GENERICA;
  }
  return MENSAGEM_GENERICA;
}
