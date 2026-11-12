import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AppComponent } from './app.component';

describe('AppComponent', () => {
  it('monta cabeçalho, conteúdo, rodapé e sacola', () => {
    TestBed.configureTestingModule({ providers: [provideRouter([])] });
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();
    const elemento: HTMLElement = fixture.nativeElement;

    expect(elemento.querySelector('eirm-cabecalho')).not.toBeNull();
    expect(elemento.querySelector('main#conteudo router-outlet')).not.toBeNull();
    expect(elemento.querySelector('eirm-rodape')).not.toBeNull();
    expect(elemento.querySelector('eirm-sacola')).not.toBeNull();
    expect(elemento.querySelector('.pular')?.getAttribute('href')).toBe('#conteudo');
  });
});
