import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree, provideRouter } from '@angular/router';
import { SacolaService } from '@services/sacola.service';
import { criarProduto } from '@testing/dados-de-teste';
import { sacolaPreenchidaGuard } from './sacola-preenchida.guard';

describe('sacolaPreenchidaGuard', () => {
  function executar(): unknown {
    return TestBed.runInInjectionContext(() =>
      sacolaPreenchidaGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot),
    );
  }

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({ providers: [provideRouter([])] });
  });

  it('redireciona para o cardápio com a sacola vazia', () => {
    const resultado = executar() as UrlTree;

    expect(resultado.toString()).toBe('/cardapio');
  });

  it('libera com itens na sacola', () => {
    TestBed.inject(SacolaService).adicionar(criarProduto());

    expect(executar()).toBe(true);
  });
});
