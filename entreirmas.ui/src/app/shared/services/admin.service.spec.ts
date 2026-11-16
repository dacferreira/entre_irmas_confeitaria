import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '@environments/environment';
import {
  CategoriaAdminService,
  LancamentoService,
  PainelService,
  PedidoAdminService,
  ProdutoAdminService,
  UsuarioService,
} from './admin.service';

describe('serviços da área administrativa', () => {
  const api = environment.apiUrl;
  let controlador: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    controlador = TestBed.inject(HttpTestingController);
  });

  afterEach(() => controlador.verify());

  it('cadastros usam GET, POST, PUT, PATCH e DELETE no caminho de cada recurso', () => {
    const produtos = TestBed.inject(ProdutoAdminService);

    produtos.listar({ ativo: true, busca: '' }).subscribe();
    produtos.criar({} as never).subscribe();
    produtos.atualizar(7, {} as never).subscribe();
    produtos.alterarSituacao(7, false).subscribe();
    produtos.excluir(7).subscribe();

    expect(controlador.expectOne(`${api}/admin/produtos?ativo=true`).request.method).toBe('GET');
    expect(controlador.expectOne({ method: 'POST', url: `${api}/admin/produtos` })).toBeTruthy();
    expect(controlador.expectOne({ method: 'PUT', url: `${api}/admin/produtos/7` })).toBeTruthy();
    const situacao = controlador.expectOne({ method: 'PATCH', url: `${api}/admin/produtos/7/situacao` });
    expect(situacao.request.body).toEqual({ ativo: false });
    expect(controlador.expectOne({ method: 'DELETE', url: `${api}/admin/produtos/7` })).toBeTruthy();
    controlador.match(() => true).forEach((r) => r.flush(null));
  });

  it('envia a foto em multipart no campo "arquivo" e remove com DELETE', () => {
    const produtos = TestBed.inject(ProdutoAdminService);

    produtos.enviarImagem(7, new Blob(['x'], { type: 'image/webp' })).subscribe();
    produtos.enviarImagem(8, new Blob(['x'])).subscribe();
    produtos.removerImagem(7).subscribe();

    const envio = controlador.expectOne({ method: 'PUT', url: `${api}/admin/produtos/7/imagem` });
    const corpo = envio.request.body as FormData;
    expect((corpo.get('arquivo') as File).name).toBe('foto.webp');
    const semTipo = controlador.expectOne({ method: 'PUT', url: `${api}/admin/produtos/8/imagem` });
    expect(((semTipo.request.body as FormData).get('arquivo') as File).name).toBe('foto.jpg');
    expect(controlador.expectOne({ method: 'DELETE', url: `${api}/admin/produtos/7/imagem` })).toBeTruthy();
  });

  it('cada cadastro aponta para o seu recurso', () => {
    TestBed.inject(CategoriaAdminService).listar().subscribe();
    TestBed.inject(UsuarioService).listar().subscribe();
    TestBed.inject(LancamentoService).listar({ tipo: 'Perda', inicio: '2026-11-01' }).subscribe();

    controlador.expectOne(`${api}/admin/categorias`).flush([]);
    controlador.expectOne(`${api}/admin/usuarios`).flush([]);
    controlador.expectOne(`${api}/admin/lancamentos?tipo=Perda&inicio=2026-11-01`).flush([]);
  });

  it('pedidos e painel', () => {
    const pedidos = TestBed.inject(PedidoAdminService);

    pedidos.listar({ status: 'Recebido', pagina: 2 }).subscribe();
    pedidos.obter(5).subscribe();
    pedidos.criar({} as never).subscribe();
    pedidos.alterarStatus(5, 'Entregue').subscribe();
    pedidos.excluir(5).subscribe();
    TestBed.inject(PainelService).obter('2026-11-01', '2026-11-30').subscribe();

    controlador.expectOne(`${api}/admin/pedidos?status=Recebido&pagina=2`).flush({});
    controlador.expectOne({ method: 'GET', url: `${api}/admin/pedidos/5` }).flush({});
    controlador.expectOne({ method: 'POST', url: `${api}/admin/pedidos` }).flush({});
    expect(controlador.expectOne(`${api}/admin/pedidos/5/status`).request.body).toEqual({ status: 'Entregue' });
    controlador.expectOne({ method: 'DELETE', url: `${api}/admin/pedidos/5` }).flush(null);
    controlador.expectOne(`${api}/admin/painel?inicio=2026-11-01&fim=2026-11-30`).flush({});
    controlador.match(() => true).forEach((r) => r.flush(null));
  });
});
