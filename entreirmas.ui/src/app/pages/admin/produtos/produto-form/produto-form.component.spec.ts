import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { ProdutoAdminService } from '@services/admin.service';
import { ImagemService } from '@services/imagem.service';
import { NotificacaoService } from '@services/notificacao.service';
import { criarCategoriaAdmin, criarNotificacaoFalsa, criarProdutoAdmin } from '@testing/dados-admin';
import { of, throwError } from 'rxjs';
import { DadosProdutoForm, ProdutoFormComponent } from './produto-form.component';

describe('ProdutoFormComponent', () => {
  const produtoService = { criar: jest.fn(), atualizar: jest.fn(), enviarImagem: jest.fn(), removerImagem: jest.fn() };
  const imagemService = { reduzir: jest.fn() };
  const referencia = { close: jest.fn() };
  const fotoReduzida = new Blob(['webp'], { type: 'image/webp' });
  let notificacao: ReturnType<typeof criarNotificacaoFalsa>;
  let fixture: ComponentFixture<ProdutoFormComponent>;
  let componente: ProdutoFormComponent;

  beforeAll(() => {
    // O jsdom não implementa URL.createObjectURL.
    URL.createObjectURL = jest.fn(() => 'blob:http://localhost/previa');
    URL.revokeObjectURL = jest.fn();
  });

  function criar(dados: DadosProdutoForm): void {
    notificacao = criarNotificacaoFalsa();
    TestBed.configureTestingModule({
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: dados },
        { provide: MatDialogRef, useValue: referencia },
        { provide: ProdutoAdminService, useValue: produtoService },
        { provide: ImagemService, useValue: imagemService },
        { provide: NotificacaoService, useValue: notificacao },
      ],
    });
    fixture = TestBed.createComponent(ProdutoFormComponent);
    componente = fixture.componentInstance;
    fixture.detectChanges();
  }

  function eventoDeArquivo(arquivo?: File): Event {
    const campo = document.createElement('input');
    Object.defineProperty(campo, 'files', { value: arquivo ? [arquivo] : [] });
    return { target: campo } as unknown as Event;
  }

  function preencher(): void {
    componente.formulario.patchValue({ nome: ' Pudim ', descricao: ' De leite ', preco: 30, custo: 9 });
  }

  it('novo produto: começa na primeira categoria e não salva sem nome, descrição e preço', () => {
    criar({ produto: null, categorias: [criarCategoriaAdmin({ id: 3 })] });

    componente.salvar();

    expect(componente.formulario.controls.categoriaId.value).toBe(3);
    expect(produtoService.criar).not.toHaveBeenCalled();
    expect(fixture.nativeElement.querySelector('h2')?.textContent).toContain('Novo produto');
  });

  it('calcula lucro e margem enquanto digita', () => {
    criar({ produto: null, categorias: [criarCategoriaAdmin()] });

    componente.formulario.patchValue({ preco: 50, custo: 20 });
    fixture.detectChanges();

    expect(componente.lucroUnitario()).toBe(30);
    expect(componente.margem()).toBe(60);
    expect(fixture.nativeElement.querySelector('.margem')?.textContent).toContain('60%');
  });

  it('cria com os textos aparados e, sem foto, mostra a ilustração e não envia imagem', () => {
    produtoService.criar.mockReturnValue(of(criarProdutoAdmin({ nome: 'Pudim' })));
    criar({ produto: null, categorias: [criarCategoriaAdmin()] });

    expect(fixture.nativeElement.querySelector('.foto__previa eirm-ilustracao')).not.toBeNull();
    preencher();
    componente.salvar();

    expect(produtoService.criar).toHaveBeenCalledWith(
      expect.objectContaining({ nome: 'Pudim', descricao: 'De leite', categoriaId: 1 }),
    );
    expect(produtoService.criar.mock.calls[0][0]).not.toHaveProperty('urlImagem');
    expect(produtoService.criar.mock.calls[0][0].encomenda).toBe(false);
    expect(produtoService.enviarImagem).not.toHaveBeenCalled();
    expect(notificacao.sucesso).toHaveBeenCalledWith('Pudim cadastrado.');
    expect(referencia.close).toHaveBeenCalled();
  });

  it('escolhe uma foto, mostra a prévia e envia depois de salvar o produto novo', async () => {
    imagemService.reduzir.mockResolvedValue(fotoReduzida);
    produtoService.criar.mockReturnValue(of(criarProdutoAdmin({ id: 33, nome: 'Pudim' })));
    produtoService.enviarImagem.mockReturnValue(of(criarProdutoAdmin({ id: 33, nome: 'Pudim', urlImagem: 'produtos/33/imagem?v=1' })));
    criar({ produto: null, categorias: [criarCategoriaAdmin()] });

    await componente.escolherFoto(eventoDeArquivo(new File(['x'], 'pudim.jpg', { type: 'image/jpeg' })));
    fixture.detectChanges();
    preencher();
    componente.salvar();

    expect(fixture.nativeElement.querySelector('.foto__previa img')?.getAttribute('src')).toBe('blob:http://localhost/previa');
    expect(produtoService.enviarImagem).toHaveBeenCalledWith(33, fotoReduzida);
    expect(referencia.close).toHaveBeenCalledWith(expect.objectContaining({ urlImagem: 'produtos/33/imagem?v=1' }));
  });

  it('mostra o erro quando a foto não serve e ignora quando nada foi escolhido', async () => {
    imagemService.reduzir.mockRejectedValue(new Error('Escolha uma foto em JPG, PNG ou WEBP.'));
    criar({ produto: null, categorias: [criarCategoriaAdmin()] });

    await componente.escolherFoto(eventoDeArquivo());
    expect(imagemService.reduzir).not.toHaveBeenCalled();

    await componente.escolherFoto(eventoDeArquivo(new File(['x'], 'doc.pdf', { type: 'application/pdf' })));
    fixture.detectChanges();

    expect(componente.fotoNova()).toBeNull();
    expect(componente.preparandoFoto()).toBe(false);
    expect(fixture.nativeElement.querySelector('.foto__erro')?.textContent).toContain('JPG, PNG ou WEBP');
  });

  it('edita mantendo a foto salva e, ao remover, apaga a foto depois de salvar', () => {
    const comFoto = criarProdutoAdmin({ urlImagem: 'produtos/10/imagem?v=5' });
    produtoService.atualizar.mockReturnValue(of(comFoto));
    produtoService.removerImagem.mockReturnValue(of(criarProdutoAdmin()));
    criar({ produto: comFoto, categorias: [criarCategoriaAdmin()] });

    expect(componente.previa()).toBe('produtos/10/imagem?v=5');
    componente.formulario.controls.encomenda.setValue(true);
    componente.tirarFoto();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.foto__previa img')).toBeNull();

    componente.salvar();

    expect(produtoService.removerImagem).toHaveBeenCalledWith(10);
    expect(produtoService.atualizar).toHaveBeenCalledWith(10, expect.objectContaining({ encomenda: true }));
    expect(notificacao.sucesso).toHaveBeenCalledWith('Bolo de Cenoura atualizado.');
  });

  it('desfaz a foto recém-escolhida sem marcar a salva para remoção', async () => {
    imagemService.reduzir.mockResolvedValue(fotoReduzida);
    produtoService.atualizar.mockReturnValue(of(criarProdutoAdmin({ urlImagem: 'produtos/10/imagem?v=5' })));
    criar({ produto: criarProdutoAdmin({ urlImagem: 'produtos/10/imagem?v=5' }), categorias: [criarCategoriaAdmin()] });

    await componente.escolherFoto(eventoDeArquivo(new File(['x'], 'a.png', { type: 'image/png' })));
    componente.tirarFoto();
    componente.salvar();

    expect(URL.revokeObjectURL).toHaveBeenCalled();
    expect(componente.previa()).toBe('produtos/10/imagem?v=5');
    expect(produtoService.enviarImagem).not.toHaveBeenCalled();
    expect(produtoService.removerImagem).not.toHaveBeenCalled();
  });

  it('se a foto falhar, o produto continua salvo e a pessoa é avisada', async () => {
    imagemService.reduzir.mockResolvedValue(fotoReduzida);
    produtoService.atualizar.mockReturnValue(of(criarProdutoAdmin()));
    produtoService.enviarImagem.mockReturnValue(throwError(() => new Error('500')));
    criar({ produto: criarProdutoAdmin(), categorias: [criarCategoriaAdmin()] });

    await componente.escolherFoto(eventoDeArquivo(new File(['x'], 'a.png', { type: 'image/png' })));
    componente.salvar();

    expect(notificacao.erro).toHaveBeenCalledWith(expect.stringContaining('foi salvo, mas a foto não'));
    expect(referencia.close).toHaveBeenCalledWith(expect.objectContaining({ id: 10 }));
    expect(componente.salvando()).toBe(false);
  });

  it('cancela sem salvar', () => {
    criar({ produto: null, categorias: [] });

    componente.cancelar();

    expect(componente.formulario.controls.categoriaId.value).toBeNull();
    expect(referencia.close).toHaveBeenCalledWith();
  });
});
