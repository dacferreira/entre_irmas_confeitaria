import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { environment } from '@environments/environment';
import { RodapeComponent } from './rodape.component';

describe('RodapeComponent', () => {
  it('mostra os contatos, a antecedência e o ano atual', () => {
    TestBed.configureTestingModule({ providers: [provideRouter([])] });
    const fixture = TestBed.createComponent(RodapeComponent);
    fixture.detectChanges();
    const elemento: HTMLElement = fixture.nativeElement;

    expect(elemento.querySelector('.botao--whatsapp')?.getAttribute('href')).toContain(
      `wa.me/${environment.whatsappNumero}`,
    );
    expect(elemento.querySelector(`a[href="${environment.instagramUrl}"]`)).not.toBeNull();
    expect(elemento.textContent).toContain(`${environment.antecedenciaMinimaDias} dia(s)`);
    expect(elemento.textContent).toContain(String(new Date().getFullYear()));
  });
});
