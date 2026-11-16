import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { AppComponent } from './app.component';

@Component({ template: '<p class="tela">tela</p>' })
class TelaComponent {}

describe('AppComponent', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideRouter([{ path: '**', component: TelaComponent }])] });
  });

  it('monta cabeçalho, conteúdo, rodapé e sacola', () => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();
    const elemento: HTMLElement = fixture.nativeElement;

    expect(elemento.querySelector('eirm-cabecalho')).not.toBeNull();
    expect(elemento.querySelector('main#conteudo router-outlet')).not.toBeNull();
    expect(elemento.querySelector('eirm-rodape')).not.toBeNull();
    expect(elemento.querySelector('eirm-sacola')).not.toBeNull();
    expect(elemento.querySelector('.pular')?.getAttribute('href')).toBe('#conteudo');
  });

  it('na área administrativa mostra só a tela, sem cabeçalho, rodapé e sacola do site', async () => {
    const fixture = TestBed.createComponent(AppComponent);
    await TestBed.inject(Router).navigateByUrl('/admin/painel');
    fixture.detectChanges();
    const elemento: HTMLElement = fixture.nativeElement;

    expect(elemento.querySelector('eirm-cabecalho')).toBeNull();
    expect(elemento.querySelector('eirm-sacola')).toBeNull();
    expect(elemento.querySelector('.tela')).not.toBeNull();

    await TestBed.inject(Router).navigateByUrl('/administracao-de-festas');
    fixture.detectChanges();
    expect(elemento.querySelector('eirm-cabecalho')).not.toBeNull();
  });
});
