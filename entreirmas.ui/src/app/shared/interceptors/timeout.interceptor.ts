import { HttpInterceptorFn } from '@angular/common/http';
import { environment } from '@environments/environment';
import { timeout } from 'rxjs';

/** Cancela requisições que passam do tempo máximo configurado. */
export const timeoutInterceptor: HttpInterceptorFn = (requisicao, next) =>
  next(requisicao).pipe(timeout(environment.timeoutMs));
