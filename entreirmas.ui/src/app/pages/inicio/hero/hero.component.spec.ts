import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { environment } from '@environments/environment';
import { HeroComponent } from './hero.component';

describe('HeroComponent', () => {
  it('mostra a chamada, os botões e a composição ilustrada', () => {
    TestBed.configureTestingModule({ providers: [provideRouter([])] });
    const fixture = TestBed.createComponent(HeroComponent);
    fixture.detectChanges();
    const elemento: HTMLElement = fixture.nativeElement;

    expect(elemento.querySelector('h1')?.textContent).toContain('entre irmãs');
    expect(elemento.querySelector('.hero__acoes a')?.getAttribute('href')).toBe('/cardapio');
    expect(elemento.querySelector('.botao--contorno')?.getAttribute('href')).toContain(environment.whatsappNumero);
    expect(elemento.querySelectorAll('.hero__granulado').length).toBe(fixture.componentInstance.granulados.length);
    expect(elemento.querySelectorAll('eirm-ilustracao').length).toBe(4);
  });
});
