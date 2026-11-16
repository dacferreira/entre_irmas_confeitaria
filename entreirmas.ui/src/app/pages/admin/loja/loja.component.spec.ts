import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LojaAdmin } from '@models/loja.model';
import { LojaService } from '@services/loja.service';
import { NotificacaoService } from '@services/notificacao.service';
import { criarNotificacaoFalsa } from '@testing/dados-admin';
import { of, throwError } from 'rxjs';
import { LojaComponent } from './loja.component';

function criarLoja(parcial: Partial<LojaAdmin> = {}): LojaAdmin {
  return { aberta: true, mensagem: null, alteradoPor: 'suellen', dataDaAlteracao: '2026-11-15T09:30:00', ...parcial };
}

describe('LojaComponent', () => {
  const lojaService = { obterAdmin: jest.fn(), alterar: jest.fn() };
  let notificacao: ReturnType<typeof criarNotificacaoFalsa>;
  let fixture: ComponentFixture<LojaComponent>;
  let componente: LojaComponent;

  function renderizar(loja: LojaAdmin): HTMLElement {
    lojaService.obterAdmin.mockReturnValue(of(loja));
    notificacao = criarNotificacaoFalsa();
    TestBed.configureTestingModule({
      providers: [
        { provide: LojaService, useValue: lojaService },
        { provide: NotificacaoService, useValue: notificacao },
      ],
    });
    fixture = TestBed.createComponent(LojaComponent);
    componente = fixture.componentInstance;
    fixture.detectChanges();
    return fixture.nativeElement;
  }

  it('mostra a loja aberta com quem alterou e fecha com o aviso digitado', () => {
    const elemento = renderizar(criarLoja());
    expect(elemento.querySelector('.situacao__titulo')?.textContent).toContain('Loja aberta');
    expect(elemento.querySelector('.situacao__alteracao')?.textContent).toContain('suellen em 15/11/2026 09:30');

    lojaService.alterar.mockReturnValue(of(criarLoja({ aberta: false, mensagem: 'Voltamos amanhã' })));
    componente.mensagem.setValue('  Voltamos amanhã  ');
    elemento.querySelector<HTMLButtonElement>('.botao--perigo')!.click();
    fixture.detectChanges();

    expect(lojaService.alterar).toHaveBeenCalledWith({ aberta: false, mensagem: 'Voltamos amanhã' });
    expect(elemento.querySelector('.situacao--fechada')).not.toBeNull();
    expect(elemento.querySelector('.situacao__titulo')?.textContent).toContain('Loja fechada');
    expect(notificacao.sucesso).toHaveBeenCalledWith('Loja fechada: o site só aceita encomendas.');
  });

  it('fechada, abre (sem aviso) ou só troca o aviso', () => {
    const elemento = renderizar(criarLoja({ aberta: false, mensagem: 'Feriado' }));
    expect(componente.mensagem.value).toBe('Feriado');

    lojaService.alterar.mockReturnValue(of(criarLoja({ aberta: false, mensagem: 'Voltamos dia 20' })));
    componente.mensagem.setValue('Voltamos dia 20');
    componente.fechar();
    expect(notificacao.sucesso).toHaveBeenCalledWith('Aviso atualizado.');

    lojaService.alterar.mockReturnValue(of(criarLoja({ aberta: true })));
    elemento.querySelector<HTMLButtonElement>('.situacao__acoes .botao:not(.botao--contorno)')!.click();
    fixture.detectChanges();

    expect(lojaService.alterar).toHaveBeenLastCalledWith({ aberta: true, mensagem: null });
    expect(componente.mensagem.value).toBe('');
    expect(notificacao.sucesso).toHaveBeenLastCalledWith('Loja aberta: o site voltou a aceitar pedidos.');
  });

  it('não fecha com aviso longo demais e mantém a tela se a alteração falhar', () => {
    renderizar(criarLoja());

    componente.mensagem.setValue('a'.repeat(201));
    componente.fechar();
    expect(lojaService.alterar).not.toHaveBeenCalled();

    componente.mensagem.setValue('Feriado');
    lojaService.alterar.mockReturnValue(throwError(() => new Error('falhou')));
    componente.fechar();

    expect(componente.loja()?.aberta).toBe(true);
    expect(componente.salvando()).toBe(false);
  });

  it('avisa quando não consegue carregar', () => {
    lojaService.obterAdmin.mockReturnValue(throwError(() => new Error('falhou')));
    TestBed.configureTestingModule({
      providers: [
        { provide: LojaService, useValue: lojaService },
        { provide: NotificacaoService, useValue: criarNotificacaoFalsa() },
      ],
    });
    const fixtureComErro = TestBed.createComponent(LojaComponent);
    fixtureComErro.detectChanges();

    expect(fixtureComErro.nativeElement.querySelector('[role="alert"]')?.textContent).toContain('Não foi possível');
  });
});
