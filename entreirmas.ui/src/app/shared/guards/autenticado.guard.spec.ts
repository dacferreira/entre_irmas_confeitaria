import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree, provideRouter } from '@angular/router';
import { AuthService } from '@services/auth.service';
import { autenticadoGuard, visitanteGuard } from './autenticado.guard';

describe('guards da área administrativa', () => {
  const auth = { autenticado: jest.fn() };
  const rota = {} as ActivatedRouteSnapshot;
  const estado = { url: '/admin/pedidos?status=Recebido' } as RouterStateSnapshot;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideRouter([]), { provide: AuthService, useValue: auth }] });
  });

  it('autenticadoGuard libera com sessão e manda para o login sem ela', () => {
    auth.autenticado.mockReturnValue(true);
    expect(TestBed.runInInjectionContext(() => autenticadoGuard(rota, estado))).toBe(true);

    auth.autenticado.mockReturnValue(false);
    const destino = TestBed.runInInjectionContext(() => autenticadoGuard(rota, estado)) as UrlTree;
    expect(destino.toString()).toBe('/admin/entrar?retorno=%2Fadmin%2Fpedidos%3Fstatus%3DRecebido');
  });

  it('visitanteGuard leva quem já entrou para o painel', () => {
    auth.autenticado.mockReturnValue(true);
    expect((TestBed.runInInjectionContext(() => visitanteGuard(rota, estado)) as UrlTree).toString()).toBe(
      '/admin/painel',
    );

    auth.autenticado.mockReturnValue(false);
    expect(TestBed.runInInjectionContext(() => visitanteGuard(rota, estado))).toBe(true);
  });
});
