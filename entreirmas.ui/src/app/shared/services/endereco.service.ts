import { Injectable, inject } from '@angular/core';
import { somenteDigitos } from '@directives/mascara-cep.directive';
import { EnderecoCep } from '@models/endereco.model';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

/**
 * Busca o endereço pelo CEP. A API consulta o ViaCEP (o navegador não fala direto com o serviço externo).
 * Silencioso: quem chama mostra o erro ao lado do campo, sem o aviso geral do rodapé.
 */
@Injectable({ providedIn: 'root' })
export class EnderecoService {
  private readonly api = inject(ApiService);

  consultarCep(cep: string): Observable<EnderecoCep> {
    return this.api.get<EnderecoCep>(`enderecos/cep/${somenteDigitos(cep)}`, {}, { silencioso: true });
  }
}
