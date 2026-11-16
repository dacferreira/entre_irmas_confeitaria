import { environment } from '@environments/environment';
import { UrlImagemPipe } from './url-imagem.pipe';

describe('UrlImagemPipe', () => {
  const pipe = new UrlImagemPipe();

  it('junta o caminho da API com a base configurada', () => {
    expect(pipe.transform('produtos/10/imagem?v=1')).toBe(`${environment.apiUrl}/produtos/10/imagem?v=1`);
    expect(pipe.transform('/produtos/10/imagem')).toBe(`${environment.apiUrl}/produtos/10/imagem`);
  });

  it('mantém URLs completas e prévias locais, e devolve nulo sem foto', () => {
    expect(pipe.transform('https://exemplo.com/a.jpg')).toBe('https://exemplo.com/a.jpg');
    expect(pipe.transform('blob:http://localhost/abc')).toBe('blob:http://localhost/abc');
    expect(pipe.transform(null)).toBeNull();
    expect(pipe.transform('')).toBeNull();
  });
});
