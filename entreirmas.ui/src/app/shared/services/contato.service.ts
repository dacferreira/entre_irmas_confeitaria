import { Injectable } from '@angular/core';
import { environment } from '@environments/environment';

/** Canais de contato da confeitaria. */
@Injectable({ providedIn: 'root' })
export class ContatoService {
  readonly instagramUrl = environment.instagramUrl;
  readonly instagramUsuario = environment.instagramUsuario;

  linkWhatsApp(mensagem = 'Olá! Vim pelo site e queria tirar uma dúvida 🧁'): string {
    const numero = environment.whatsappNumero.replace(/\D/g, '');
    return `https://wa.me/${numero}?text=${encodeURIComponent(mensagem)}`;
  }
}
