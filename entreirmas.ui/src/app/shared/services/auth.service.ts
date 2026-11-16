import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { ROTAS_ADMIN } from '@constants/admin';
import { Sessao } from '@models/admin.model';
import { Observable, tap } from 'rxjs';
import { ApiService } from './api.service';

const CHAVE = 'eirm.sessao';

/** Login da área administrativa. A sessão (token JWT) fica no localStorage até expirar ou a pessoa sair. */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = inject(ApiService);
  private readonly router = inject(Router);
  private readonly sessao = signal<Sessao | null>(this.lerSessao());

  /** Verdadeiro enquanto há sessão e o token não venceu. */
  readonly autenticado = computed(() => {
    const sessao = this.sessao();
    return !!sessao && Date.parse(sessao.expiraEm) > Date.now();
  });

  readonly nome = computed(() => this.sessao()?.nome ?? '');
  readonly login = computed(() => this.sessao()?.login ?? '');

  entrar(login: string, senha: string): Observable<Sessao> {
    return this.api.post<Sessao>('autenticacao/login', { login, senha }).pipe(tap((sessao) => this.gravar(sessao)));
  }

  /** Token para o cabeçalho Authorization; nulo quando não há sessão válida. */
  token(): string | null {
    return this.autenticado() ? (this.sessao()?.token ?? null) : null;
  }

  /** Encerra a sessão e volta para o login (com o endereço atual para retornar depois, se informado). */
  sair(retorno?: string): void {
    this.gravar(null);
    void this.router.navigate(['/', ROTAS_ADMIN.BASE, ROTAS_ADMIN.LOGIN], {
      queryParams: retorno ? { retorno } : {},
    });
  }

  private gravar(sessao: Sessao | null): void {
    this.sessao.set(sessao);
    try {
      if (sessao) {
        localStorage.setItem(CHAVE, JSON.stringify(sessao));
      } else {
        localStorage.removeItem(CHAVE);
      }
    } catch {
      // Navegação anônima ou armazenamento bloqueado: a sessão vale só nesta aba.
    }
  }

  private lerSessao(): Sessao | null {
    try {
      const salva = JSON.parse(localStorage.getItem(CHAVE) ?? 'null') as Sessao | null;
      return salva?.token && salva.expiraEm ? salva : null;
    } catch {
      return null;
    }
  }
}
