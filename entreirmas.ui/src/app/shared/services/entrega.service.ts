import { Injectable, inject } from '@angular/core';
import { environment } from '@environments/environment';
import { EnderecoPedido } from '@models/endereco.model';
import { CotacaoEntrega } from '@models/pedido.model';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

/** Entrega por Uber/99: cálculo da taxa no checkout e o mapa do trajeto da loja até o cliente. */
@Injectable({ providedIn: 'root' })
export class EntregaService {
  private readonly api = inject(ApiService);

  /** A API cota nos parceiros e devolve a mais barata (ou o aviso de taxa combinada). */
  cotar(endereco: EnderecoPedido): Observable<CotacaoEntrega> {
    return this.api.post<CotacaoEntrega>('entregas/cotacao', endereco);
  }

  /** Mapa incorporado (sem chave de API) com o trajeto de carro da loja até o endereço. */
  urlDoTrajeto(endereco: EnderecoPedido): string {
    const destino = `${endereco.logradouro}, ${endereco.numero} - ${endereco.bairro}, ${endereco.cidade} - ${endereco.uf}, ${endereco.cep}`;
    return `https://maps.google.com/maps?saddr=${environment.coordenadasLoja}&daddr=${encodeURIComponent(destino)}&output=embed`;
  }
}
