import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { ContatoService } from '@services/contato.service';

/** Onde fica a loja: endereço de retirada, link "Como chegar" e o mapa do Google incorporado. */
@Component({
  selector: 'eirm-mapa-loja',
  templateUrl: './mapa-loja.component.html',
  styleUrl: './mapa-loja.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MapaLojaComponent {
  readonly contato = inject(ContatoService);

  // O endereço vem do environment (não do usuário): a URL do mapa é fixa e pode ir no iframe.
  readonly urlMapa = inject(DomSanitizer).bypassSecurityTrustResourceUrl(this.contato.urlMapaIncorporado);
}
