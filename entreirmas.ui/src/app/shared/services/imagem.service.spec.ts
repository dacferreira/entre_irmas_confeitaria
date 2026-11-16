import { TestBed } from '@angular/core/testing';
import { ImagemService, LADO_MAXIMO, TAMANHO_MAXIMO_BYTES, dimensoesReduzidas } from './imagem.service';

describe('dimensoesReduzidas', () => {
  it('reduz o maior lado para o limite sem distorcer', () => {
    expect(dimensoesReduzidas(4000, 3000)).toEqual({ largura: LADO_MAXIMO, altura: 900 });
    expect(dimensoesReduzidas(3000, 4000)).toEqual({ largura: 900, altura: LADO_MAXIMO });
  });

  it('não amplia foto pequena', () => {
    expect(dimensoesReduzidas(800, 600)).toEqual({ largura: 800, altura: 600 });
  });
});

describe('ImagemService', () => {
  let servico: ImagemService;
  let tiposGerados: string[];
  let tamanhoGerado: number;
  const desenhar = jest.fn();

  beforeEach(() => {
    servico = TestBed.inject(ImagemService);
    tiposGerados = [];
    tamanhoGerado = 1000;
    (globalThis as { createImageBitmap?: unknown }).createImageBitmap = jest.fn(async () => ({
      width: 2400,
      height: 1800,
      close: jest.fn(),
    }));
    jest.spyOn(HTMLCanvasElement.prototype, 'getContext').mockReturnValue({ drawImage: desenhar } as never);
  });

  // suportaWebp: false simula o Safari antigo, que devolve PNG quando pedimos WEBP.
  function canvasGera(suportaWebp: boolean): void {
    jest.spyOn(HTMLCanvasElement.prototype, 'toBlob').mockImplementation(function (this: HTMLCanvasElement, cb, tipo) {
      tiposGerados.push(tipo ?? '');
      const gerado = tipo === 'image/webp' && !suportaWebp ? 'image/png' : (tipo ?? 'image/png');
      cb(new Blob([new Uint8Array(tamanhoGerado)], { type: gerado }));
    });
  }

  const foto = (tipo = 'image/jpeg'): File => new File(['x'], 'foto', { type: tipo });

  it('reduz para até 1200px e gera WEBP', async () => {
    canvasGera(true);

    const reduzida = await servico.reduzir(foto());

    expect(reduzida.type).toBe('image/webp');
    expect(desenhar).toHaveBeenCalledWith(expect.anything(), 0, 0, 1200, 900);
    expect(tiposGerados).toEqual(['image/webp']);
  });

  it('gera JPG quando o navegador não sabe gerar WEBP', async () => {
    canvasGera(false);

    const reduzida = await servico.reduzir(foto('image/png'));

    expect(reduzida.type).toBe('image/jpeg');
    expect(tiposGerados).toEqual(['image/webp', 'image/jpeg']);
  });

  it('recusa arquivo que não é foto aceita, foto ilegível e foto grande demais', async () => {
    canvasGera(true);
    await expect(servico.reduzir(foto('image/gif'))).rejects.toThrow('JPG, PNG ou WEBP');

    (globalThis as { createImageBitmap?: unknown }).createImageBitmap = jest.fn(async () => {
      throw new Error('corrompida');
    });
    await expect(servico.reduzir(foto())).rejects.toThrow('Não foi possível abrir');
  });

  it('recusa quando, mesmo reduzida, passa de 2 MB', async () => {
    tamanhoGerado = TAMANHO_MAXIMO_BYTES + 1;
    canvasGera(true);

    await expect(servico.reduzir(foto())).rejects.toThrow('2 MB');
  });

  it('avisa quando o navegador não consegue gerar a foto', async () => {
    jest.spyOn(HTMLCanvasElement.prototype, 'toBlob').mockImplementation((cb) => cb(null));

    await expect(servico.reduzir(foto())).rejects.toThrow('Não foi possível preparar');
  });
});
