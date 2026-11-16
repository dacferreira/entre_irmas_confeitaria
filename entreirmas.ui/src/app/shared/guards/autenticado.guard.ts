import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { ROTAS_ADMIN } from '@constants/admin';
import { AuthService } from '@services/auth.service';

/** Área administrativa: sem sessão, vai para o login e volta para a tela pedida depois de entrar. */
export const autenticadoGuard: CanActivateFn = (_rota, estado) => {
  if (inject(AuthService).autenticado()) {
    return true;
  }
  return inject(Router).createUrlTree(['/', ROTAS_ADMIN.BASE, ROTAS_ADMIN.LOGIN], {
    queryParams: { retorno: estado.url },
  });
};

/** Tela de login: quem já está logado vai direto para o painel. */
export const visitanteGuard: CanActivateFn = () =>
  inject(AuthService).autenticado()
    ? inject(Router).createUrlTree(['/', ROTAS_ADMIN.BASE, ROTAS_ADMIN.PAINEL])
    : true;
