import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { ROTAS } from '@constants/rotas';
import { SacolaService } from '@services/sacola.service';

/** Só deixa abrir o checkout com algo na sacola; senão, volta para o cardápio. */
export const sacolaPreenchidaGuard: CanActivateFn = () => {
  const sacola = inject(SacolaService);
  return sacola.vazia() ? inject(Router).createUrlTree(['/', ROTAS.CARDAPIO]) : true;
};
