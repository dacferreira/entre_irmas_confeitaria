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
});
