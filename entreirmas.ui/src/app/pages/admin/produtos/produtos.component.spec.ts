import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { CategoriaAdminService, ProdutoAdminService } from '@services/admin.service';
import { NotificacaoService } from '@services/notificacao.service';
import { criarCategoriaAdmin, criarDialogoFalso, criarNotificacaoFalsa, criarProdutoAdmin } from '@testing/dados-admin';
import { of, throwError } from 'rxjs';
import { ProdutosComponent } from './produtos.component';

describe('ProdutosComponent', () => {
  const produtoService = { listar: jest.fn(), alterarSituacao: jest.fn(), excluir: jest.fn() };
  const categoriaService = { listar: jest.fn(() => of([criarCategoriaAdmin(), criarCategoriaAdmin({ id: 2, nome: 'Docinhos' })])) };
  let notificacao: ReturnType<typeof criarNotificacaoFalsa>;
  let dialogo: ReturnType<typeof criarDialogoFalso>;
  let fixture: ComponentFixture<ProdutosComponent>;
  let componente: ProdutosComponent;

  const cenoura = criarProdutoAdmin();
  const brigadeiro = criarProdutoAdmin({ id: 11, nome: 'Brigadeiro Gourmet', categoriaId: 2, preco: 2.5, custo: 0, ativo: false });

  function criar(): void {
    notificacao = criarNotificacaoFalsa();
    dialogo = criarDialogoFalso(true);
    TestBed.configureTestingModule({
      providers: [
        { provide: ProdutoAdminService, useValue: produtoService },
        { provide: CategoriaAdminService, useValue: categoriaService },
        { provide: NotificacaoService, useValue: notificacao },
        { provide: MatDialog, useValue: dialogo },
      ],
    });
    fixture = TestBed.createComponent(ProdutosComponent);
    componente = fixture.componentInstance;
    fixture.detectChanges();
  }

  beforeEach(() => produtoService.listar.mockReturnValue(of([cenoura, brigadeiro])));

  it('lista com margem, aviso de custo faltando e produto inativo esmaecido', () => {
    criar();
    const elemento: HTMLElement = fixture.nativeElement;

    expect(elemento.querySelectorAll('tbody tr').length).toBe(2);
    expect(componente.margem(cenoura)).toBe(60);
    expect(elemento.querySelector('.sem-custo')).not.toBeNull();
    expect(elemento.querySelector('tr.inativo')?.textContent).toContain('Brigadeiro');
  });

  it('filtra por nome sem acento, categoria e situação', () => {
    criar();

    componente.busca.set('cenóura');
    expect(componente.filtrados().map((p) => p.id)).toEqual([10]);
    componente.busca.set('');
    componente.categoriaId.set(2);
    expect(componente.filtrados().map((p) => p.id)).toEqual([11]);
    componente.categoriaId.set(null);
    componente.situacao.set('ativos');
    expect(componente.filtrados().map((p) => p.id)).toEqual([10]);
    componente.situacao.set('inativos');
    expect(componente.filtrados().map((p) => p.id)).toEqual([11]);
  });

  it('liga e desliga o produto no cardápio', () => {
    produtoService.alterarSituacao.mockReturnValue(of(undefined));
    criar();

    componente.alternarSituacao(cenoura);

    expect(produtoService.alterarSituacao).toHaveBeenCalledWith(10, false);
    expect(componente.produtos()?.find((p) => p.id === 10)?.ativo).toBe(false);
    expect(notificacao.sucesso).toHaveBeenCalledWith('Bolo de Cenoura saiu do cardápio.');
    expect(componente.alterando()).toBeNull();
  });

  it('libera o interruptor quando a troca falha', () => {
    produtoService.alterarSituacao.mockReturnValue(throwError(() => new Error('x')));
    criar();

    componente.alternarSituacao(brigadeiro);

    expect(componente.alterando()).toBeNull();
    expect(componente.produtos()?.find((p) => p.id === 11)?.ativo).toBe(false);
  });

  it('exclui depois de confirmar e abre o formulário com as categorias', () => {
    produtoService.excluir.mockReturnValue(of(undefined));
    criar();

    componente.excluir(cenoura);
    expect(produtoService.excluir).toHaveBeenCalledWith(10);
    expect(componente.produtos()?.map((p) => p.id)).toEqual([11]);

    componente.abrirFormulario(brigadeiro);
    const dados = dialogo.open.mock.calls[1][1].data;
    expect(dados.produto.id).toBe(11);
    expect(dados.categorias.length).toBe(2);
    // Salvou no formulário: recarrega a lista.
    expect(produtoService.listar).toHaveBeenCalledTimes(2);
  });

  it('mostra o erro e o estado vazio', () => {
    produtoService.listar.mockReturnValue(throwError(() => new Error('x')));
    criar();
    expect(fixture.nativeElement.querySelector('[role="alert"]')).not.toBeNull();

    produtoService.listar.mockReturnValue(of([]));
    componente.carregar();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.admin-vazio')?.textContent).toContain('Nenhum produto cadastrado');
  });
});
