import { Pipe, PipeTransform } from '@angular/core';

/**
 * Formata datas "yyyy-MM-dd" como "dd/MM/yyyy" sem conversão de fuso
 * (o DatePipe trataria a data como UTC e poderia mostrar o dia anterior).
 */
@Pipe({ name: 'dataBr' })
export class DataBrPipe implements PipeTransform {
  transform(valor: string | null | undefined): string {
    const partes = /^(\d{4})-(\d{2})-(\d{2})/.exec(valor ?? '');
    return partes ? `${partes[3]}/${partes[2]}/${partes[1]}` : '';
  }
}
