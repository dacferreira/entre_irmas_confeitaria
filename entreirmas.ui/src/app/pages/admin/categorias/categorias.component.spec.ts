import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { CategoriaAdminService } from '@services/admin.service';
import { NotificacaoService } from '@services/notificacao.service';
import { criarCategoriaAdmin, criarDialogoFalso, criarNotificacaoFalsa } from '@testing/dados-admin';
import { of, throwError } from 'rxjs';
import { CategoriasComponent } from './categorias.component';

describe('CategoriasComponent', () => {
  const categoriaService = { listar: jest.fn(), alterarSituacao: jest.fn(), excluir: jest.fn() };
  const bolos = criarCategoriaAdmin();
  const tortas = criarCategoriaAdmin({ id: 2, nome: 'Tortas', ordem: 4, quantidadeProdutos: 0, descricao: null });
  let notificacao: ReturnType<typeof criarNotificacaoFalsa>;
  let dialogo: ReturnType<typeof criarDialogoFalso>;
  let fixture: ComponentFixture<CategoriasComponent>;
  let componente: CategoriasComponent;

  function criar(): void {
    notificacao = criarNotificacaoFalsa();
    dialogo = criarDialogoFalso(true);
    TestBed.configureTestingModule({
      providers: [
        { provide: CategoriaAdminService, useValue: categoriaService },
        { provide: NotificacaoService, useValue: notificacao },
        { provide: MatDialog, useValue: dialogo },
      ],
    });
    fixture = TestBed.createComponent(CategoriasComponent);
    componente = fixture.componentInstance;
    fixture.detectChanges();
  }

  beforeEach(() => categoriaService.listar.mockReturnValue(of([bolos, tortas])));

  it('lista e sugere a próxima ordem ao criar', () => {
    criar();

    componente.abrirFormulario();

    expect(fixture.nativeElement.querySelectorAll('tbody tr').length).toBe(2);
    expect(dialogo.open.mock.calls[0][1].data).toEqual({ categoria: null, proximaOrdem: 5 });
    expect(categoriaService.listar).toHaveBeenCalledTimes(2);
  });

  it('esconde e mostra no site', () => {
    categoriaService.alterarSituacao.mockReturnValue(of(undefined));
    criar();

    componente.alternarSituacao(bolos);
    componente.alternarSituacao({ ...tortas, ativo: false });

    expect(categoriaService.alterarSituacao).toHaveBeenCalledWith(1, false);
    expect(notificacao.sucesso).toHaveBeenCalledWith('Bolos e os produtos dela saíram do site.');
    expect(notificacao.sucesso).toHaveBeenCalledWith('Tortas voltou ao site.');
  });

  it('não deixa excluir com produtos e exclui a vazia', () => {
    categoriaService.excluir.mockReturnValue(of(undefined));
    criar();

    componente.excluir(bolos);
    expect(notificacao.erro).toHaveBeenCalledWith(expect.stringContaining('tem 2 produto(s)'));
    expect(dialogo.open).not.toHaveBeenCalled();

    componente.excluir(tortas);
    expect(categoriaService.excluir).toHaveBeenCalledWith(2);
    expect(componente.categorias()?.map((c) => c.id)).toEqual([1]);
  });

  it('trata a falha ao trocar a situação e ao carregar', () => {
    categoriaService.alterarSituacao.mockReturnValue(throwError(() => new Error('x')));
    categoriaService.listar.mockReturnValue(throwError(() => new Error('x')));
    criar();

    componente.alternarSituacao(bolos);

    expect(componente.alterando()).toBeNull();
    expect(fixture.nativeElement.querySelector('[role="alert"]')).not.toBeNull();
  });
});
