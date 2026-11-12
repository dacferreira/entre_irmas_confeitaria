import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { NaoEncontradoComponent } from './nao-encontrado.component';

describe('NaoEncontradoComponent', () => {
  it('mostra a mensagem e os caminhos de volta', () => {
    TestBed.configureTestingModule({ providers: [provideRouter([])] });
    const fixture = TestBed.createComponent(NaoEncontradoComponent);
    fixture.detectChanges();
    const elemento: HTMLElement = fixture.nativeElement;

    expect(elemento.querySelector('h1')?.textContent).toContain('essa página acabou');
    expect(Array.from(elemento.querySelectorAll('.vazio__acoes a')).map((a) => a.getAttribute('href'))).toEqual([
      '/cardapio',
      '/',
    ]);
  });
});
