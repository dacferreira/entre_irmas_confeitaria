import { TestBed } from '@angular/core/testing';
import { environment } from '@environments/environment';
import { ContatoService } from './contato.service';

describe('ContatoService', () => {
  it('monta o link do WhatsApp com a mensagem codificada', () => {
    const servico = TestBed.inject(ContatoService);

    expect(servico.linkWhatsApp('Olá, tudo bem?')).toBe(
      `https://wa.me/${environment.whatsappNumero}?text=Ol%C3%A1%2C%20tudo%20bem%3F`,
    );
    expect(servico.linkWhatsApp()).toContain('?text=');
    expect(servico.instagramUrl).toBe(environment.instagramUrl);
  });

  it('expõe o endereço da loja e os links do mapa pelas coordenadas da loja', () => {
    const servico = TestBed.inject(ContatoService);
    const coordenadas = '-22.9161966,-43.6447006';

    expect(servico.enderecoLoja).toBe('Rua A, 41 - Paciência, Rio de Janeiro - RJ, 23580-250');
    expect(environment.coordenadasLoja).toBe(coordenadas);
    expect(servico.linkMapa).toBe(`https://www.google.com/maps/search/?api=1&query=${coordenadas}`);
    expect(servico.linkRota).toBe(`https://www.google.com/maps/dir/?api=1&destination=${coordenadas}`);
    expect(servico.urlMapaIncorporado).toBe(`https://maps.google.com/maps?q=${coordenadas}&z=17&output=embed`);
  });
});
