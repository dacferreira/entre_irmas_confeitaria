import { TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { CategoriaAdminService } from '@services/admin.service';
import { NotificacaoService } from '@services/notificacao.service';
import { criarCategoriaAdmin, criarNotificacaoFalsa } from '@testing/dados-admin';
import { of } from 'rxjs';
import { CategoriaFormComponent, DadosCategoriaForm } from './categoria-form.component';

describe('CategoriaFormComponent', () => {
  const categoriaService = { criar: jest.fn(), atualizar: jest.fn() };
  const referencia = { close: jest.fn() };

  function criar(dados: DadosCategoriaForm): CategoriaFormComponent {
    TestBed.configureTestingModule({
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: dados },
        { provide: MatDialogRef, useValue: referencia },
        { provide: CategoriaAdminService, useValue: categoriaService },
        { provide: NotificacaoService, useValue: criarNotificacaoFalsa() },
      ],
    });
    const fixture = TestBed.createComponent(CategoriaFormComponent);
    fixture.detectChanges();
    return fixture.componentInstance;
  }

  it('cria com a ordem sugerida e descrição vazia como nula', () => {
    categoriaService.criar.mockReturnValue(of(criarCategoriaAdmin({ nome: 'Pães' })));
    const componente = criar({ categoria: null, proximaOrdem: 6 });

    componente.salvar();
    expect(categoriaService.criar).not.toHaveBeenCalled();

    componente.formulario.patchValue({ nome: ' Pães ', descricao: '  ' });
    componente.salvar();

    expect(categoriaService.criar).toHaveBeenCalledWith({ nome: 'Pães', descricao: null, ordem: 6 });
    expect(referencia.close).toHaveBeenCalled();
  });

  it('edita a categoria existente', () => {
    categoriaService.atualizar.mockReturnValue(of(criarCategoriaAdmin()));
    const componente = criar({ categoria: criarCategoriaAdmin(), proximaOrdem: 9 });

    expect(componente.formulario.controls.ordem.value).toBe(1);
    componente.salvar();
    componente.cancelar();

    expect(categoriaService.atualizar).toHaveBeenCalledWith(1, { nome: 'Bolos', descricao: 'Bolos caseiros', ordem: 1 });
    expect(referencia.close).toHaveBeenLastCalledWith();
  });
});
