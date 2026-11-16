import { Injectable } from '@angular/core';

/** Maior lado da foto depois de reduzida, em pixels. Sobra para a página do produto (640px em tela 2x). */
export const LADO_MAXIMO = 1200;

/** Mesmo limite da API. */
export const TAMANHO_MAXIMO_BYTES = 2 * 1024 * 1024;

const QUALIDADE = 0.85;
const TIPOS_ACEITOS = ['image/jpeg', 'image/png', 'image/webp'];

/** Largura e altura reduzidas para caber em LADO_MAXIMO, sem distorcer e sem ampliar foto pequena. */
export function dimensoesReduzidas(largura: number, altura: number, ladoMaximo = LADO_MAXIMO): { largura: number; altura: number } {
  const escala = Math.min(1, ladoMaximo / Math.max(largura, altura));
  return { largura: Math.round(largura * escala), altura: Math.round(altura * escala) };
}

/**
 * Prepara a foto no próprio navegador antes do envio: reduz para até 1200px e converte para WEBP
 * (ou JPG, onde o navegador não gera WEBP). Uma foto de celular de 5 MB vira ~200 KB, sem biblioteca extra.
 */
@Injectable({ providedIn: 'root' })
export class ImagemService {
  /** Rejeita com uma mensagem amigável quando o arquivo não é uma foto aceita. */
  async reduzir(arquivo: File): Promise<Blob> {
    if (!TIPOS_ACEITOS.includes(arquivo.type)) {
      throw new Error('Escolha uma foto em JPG, PNG ou WEBP.');
    }

    let bitmap: ImageBitmap;
    try {
      bitmap = await createImageBitmap(arquivo);
    } catch {
      throw new Error('Não foi possível abrir esta foto. Tente outra.');
    }

    const { largura, altura } = dimensoesReduzidas(bitmap.width, bitmap.height);
    const canvas = document.createElement('canvas');
    canvas.width = largura;
    canvas.height = altura;
    canvas.getContext('2d')?.drawImage(bitmap, 0, 0, largura, altura);
    bitmap.close();

    // Navegador sem WEBP no canvas devolve PNG (pesado): nesse caso gera JPG.
    let foto = await this.gerar(canvas, 'image/webp');
    if (foto.type !== 'image/webp') {
      foto = await this.gerar(canvas, 'image/jpeg');
    }

    if (foto.size > TAMANHO_MAXIMO_BYTES) {
      throw new Error('A foto ficou maior que 2 MB mesmo reduzida. Tente outra.');
    }
    return foto;
  }

  private gerar(canvas: HTMLCanvasElement, tipo: string): Promise<Blob> {
    return new Promise((resolver, rejeitar) =>
      canvas.toBlob((blob) => (blob ? resolver(blob) : rejeitar(new Error('Não foi possível preparar a foto.'))), tipo, QUALIDADE),
    );
  }
}
