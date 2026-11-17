import { Injectable, inject } from '@angular/core';
import {
  CategoriaAdmin,
  FiltroLancamento,
  FiltroPedido,
  FiltroProdutoAdmin,
  Lancamento,
  Pagina,
  Painel,
  PedidoDetalhe,
  PedidoResumo,
  ProdutoAdmin,
  SalvarCategoria,
  SalvarLancamento,
  SalvarProduto,
  SalvarUsuario,
  StatusPedido,
  Usuario,
} from '@models/admin.model';
import { NovoPedido } from '@models/pedido.model';
import { Observable } from 'rxjs';
import { ParametrosApi, ApiService } from './api.service';

/** Operações comuns aos cadastros da área administrativa (listar, criar, editar, excluir). */
abstract class CadastroAdminService<T, TSalvar, TFiltro extends object = object> {
  protected readonly api = inject(ApiService);
  protected abstract readonly caminho: string;

  listar(filtro?: TFiltro): Observable<T[]> {
    return this.api.get<T[]>(this.caminho, { ...(filtro ?? {}) } as ParametrosApi);
  }

  criar(dados: TSalvar): Observable<T> {
    return this.api.post<T>(this.caminho, dados);
  }

  atualizar(id: number, dados: TSalvar): Observable<T> {
    return this.api.put<T>(`${this.caminho}/${id}`, dados);
  }

  excluir(id: number): Observable<void> {
    return this.api.delete(`${this.caminho}/${id}`);
  }
}

/** Cadastros que podem ser ligados e desligados sem excluir (aparecer ou não no site). */
abstract class CadastroComSituacaoService<T, TSalvar, TFiltro extends object = object> extends CadastroAdminService<
  T,
  TSalvar,
  TFiltro
> {
  alterarSituacao(id: number, ativo: boolean): Observable<void> {
    return this.api.patch<void>(`${this.caminho}/${id}/situacao`, { ativo });
  }
}

@Injectable({ providedIn: 'root' })
export class CategoriaAdminService extends CadastroComSituacaoService<CategoriaAdmin, SalvarCategoria> {
  protected readonly caminho = 'admin/categorias';
}

@Injectable({ providedIn: 'root' })
export class ProdutoAdminService extends CadastroComSituacaoService<ProdutoAdmin, SalvarProduto, FiltroProdutoAdmin> {
  protected readonly caminho = 'admin/produtos';

  /** Envia (ou troca) a foto; devolve o produto com o novo urlImagem. */
  enviarImagem(id: number, foto: Blob): Observable<ProdutoAdmin> {
    const extensao = foto.type.split('/')[1] ?? 'jpg';
    return this.api.enviarArquivo<ProdutoAdmin>(`${this.caminho}/${id}/imagem`, 'arquivo', foto, `foto.${extensao}`);
  }

  /** Apaga a foto: o site volta a usar a ilustração. */
  removerImagem(id: number): Observable<ProdutoAdmin> {
    return this.api.delete<ProdutoAdmin>(`${this.caminho}/${id}/imagem`);
  }
}

@Injectable({ providedIn: 'root' })
export class UsuarioService extends CadastroComSituacaoService<Usuario, SalvarUsuario> {
  protected readonly caminho = 'admin/usuarios';
}

@Injectable({ providedIn: 'root' })
export class LancamentoService extends CadastroAdminService<Lancamento, SalvarLancamento, FiltroLancamento> {
  protected readonly caminho = 'admin/lancamentos';
}

@Injectable({ providedIn: 'root' })
export class PedidoAdminService {
  private readonly api = inject(ApiService);
  private readonly caminho = 'admin/pedidos';

  listar(filtro: FiltroPedido): Observable<Pagina<PedidoResumo>> {
    return this.api.get<Pagina<PedidoResumo>>(this.caminho, { ...filtro });
  }

  obter(id: number): Observable<PedidoDetalhe> {
    return this.api.get<PedidoDetalhe>(`${this.caminho}/${id}`);
  }

  /** Pedido recebido fora do site: entra como confirmado. */
  criar(pedido: NovoPedido): Observable<PedidoDetalhe> {
    return this.api.post<PedidoDetalhe>(this.caminho, pedido);
  }

  alterarStatus(id: number, status: StatusPedido): Observable<PedidoDetalhe> {
    return this.api.patch<PedidoDetalhe>(`${this.caminho}/${id}/status`, { status });
  }

  /** Chama de novo o entregador da Uber/99 (quando a chamada automática do "Pronto" falhou). */
  chamarEntregador(id: number): Observable<PedidoDetalhe> {
    return this.api.post<PedidoDetalhe>(`${this.caminho}/${id}/entregador`, {});
  }

  excluir(id: number): Observable<void> {
    return this.api.delete(`${this.caminho}/${id}`);
  }
}

@Injectable({ providedIn: 'root' })
export class PainelService {
  private readonly api = inject(ApiService);

  obter(inicio: string, fim: string): Observable<Painel> {
    return this.api.get<Painel>('admin/painel', { inicio, fim });
  }
}
