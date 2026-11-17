import { TestBed } from '@angular/core/testing';
import { ContatoService } from '@services/contato.service';
import { MapaLojaComponent } from './mapa-loja.component';

describe('MapaLojaComponent', () => {
  it('mostra o endereço da loja, o link "Como chegar" e o mapa incorporado', () => {
    const fixture = TestBed.createComponent(MapaLojaComponent);
    fixture.detectChanges();
    const elemento: HTMLElement = fixture.nativeElement;
    const contato = TestBed.inject(ContatoService);

    expect(elemento.querySelector('section')?.id).toBe('onde-estamos');
    expect(elemento.querySelector('address')?.textContent).toContain('Rua A, 41 - Paciência, Rio de Janeiro - RJ, 23580-250');

    const comoChegar = elemento.querySelector<HTMLAnchorElement>('a.botao')!;
    expect(comoChegar.href).toBe(contato.linkRota);
    expect(comoChegar.target).toBe('_blank');

    const mapa = elemento.querySelector<HTMLIFrameElement>('iframe')!;
    expect(mapa.src).toBe(contato.urlMapaIncorporado);
    expect(mapa.title).toContain('Mapa');
    expect(mapa.getAttribute('loading')).toBe('lazy');
  });
});
