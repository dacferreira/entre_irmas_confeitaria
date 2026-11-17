import { Injectable } from '@angular/core';
import { environment } from '@environments/environment';

/** Canais de contato da confeitaria e o endereço da loja (onde o cliente retira o pedido). */
@Injectable({ providedIn: 'root' })
export class ContatoService {
  readonly instagramUrl = environment.instagramUrl;
  readonly instagramUsuario = environment.instagramUsuario;
  readonly enderecoLoja = environment.enderecoLoja;

  // Os mapas usam as coordenadas: pelo texto ("Rua A") o Google põe o pino em outra rua.
  /** Abre o ponto da loja no Google Maps (no celular, no app de mapas). */
  readonly linkMapa = `https://www.google.com/maps/search/?api=1&query=${environment.coordenadasLoja}`;

  /** Abre a rota até a loja a partir de onde a pessoa está. */
  readonly linkRota = `https://www.google.com/maps/dir/?api=1&destination=${environment.coordenadasLoja}`;

  /** Mapa para incorporar na página (iframe), sem chave de API. */
  readonly urlMapaIncorporado = `https://maps.google.com/maps?q=${environment.coordenadasLoja}&z=17&output=embed`;

  linkWhatsApp(mensagem = 'Olá! Vim pelo site e queria tirar uma dúvida 🧁'): string {
    const numero = environment.whatsappNumero.replace(/\D/g, '');
    return `https://wa.me/${numero}?text=${encodeURIComponent(mensagem)}`;
  }
}
