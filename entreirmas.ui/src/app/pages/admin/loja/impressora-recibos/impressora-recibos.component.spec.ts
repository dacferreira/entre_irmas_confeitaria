import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ImpressoraService } from '@services/impressora.service';
import { NotificacaoService } from '@services/notificacao.service';
import { criarNotificacaoFalsa } from '@testing/dados-admin';
import { ImpressoraRecibosComponent } from './impressora-recibos.component';

describe('ImpressoraRecibosComponent', () => {
  let notificacao: ReturnType<typeof criarNotificacaoFalsa>;
  let conectada: ReturnType<typeof signal<boolean>>;
  let automatica: ReturnType<typeof signal<boolean>>;
  let impressora: Record<string, unknown> & {
    conectar: jest.Mock;
    imprimirTeste: jest.Mock;
    alternarAutomatica: jest.Mock;
  };
  let fixture: ComponentFixture<ImpressoraRecibosComponent>;
  let elemento: HTMLElement;

  function criar(suportada = true, habilitada = true): void {
    notificacao = criarNotificacaoFalsa();
    conectada = signal(false);
    automatica = signal(true);
    impressora = {
      habilitada,
      suportada,
      conectada,
      automatica,
      imprimindo: signal(false),
      conectar: jest.fn(async () => {
        conectada.set(true);
        return true;
      }),
      imprimirTeste: jest.fn(async () => undefined),
      alternarAutomatica: jest.fn(() => automatica.update((ligada) => !ligada)),
    };
    TestBed.configureTestingModule({
      providers: [
        { provide: ImpressoraService, useValue: impressora },
        { provide: NotificacaoService, useValue: notificacao },
      ],
    });
    fixture = TestBed.createComponent(ImpressoraRecibosComponent);
    fixture.detectChanges();
    elemento = fixture.nativeElement;
  }

  function botoes(): HTMLButtonElement[] {
    return Array.from(elemento.querySelectorAll<HTMLButtonElement>('.impressora__acoes button'));
  }

  it('sem suporte no navegador, explica que precisa do Chrome ou do Edge no computador', () => {
    criar(false);

    expect(elemento.textContent).toContain('Use o Chrome ou o Edge');
    expect(botoes()).toHaveLength(0);
  });

  it('desligada no environment, avisa que não imprime e não mostra os botões', () => {
    criar(true, false);

    expect(elemento.textContent).toContain('Impressora desligada neste ambiente');
    expect(elemento.querySelector('.impressora__automatica')).toBeNull();
    expect(botoes()).toHaveLength(0);
  });

  it('conecta a impressora, mostra a situação e imprime o teste', async () => {
    criar();
    expect(elemento.querySelector('.impressora__situacao')?.textContent).toContain('Nenhuma impressora conectada');
    expect(botoes().map((botao) => botao.textContent?.trim())).toEqual(['Conectar impressora']);

    await fixture.componentInstance.conectar();
    fixture.detectChanges();

    expect(notificacao.sucesso).toHaveBeenCalledWith('Impressora conectada. Imprima um teste para conferir.');
    expect(elemento.querySelector('.impressora__situacao')?.textContent).toContain('Impressora conectada');
    expect(botoes().map((botao) => botao.textContent?.trim())).toEqual(['Trocar impressora', 'Imprimir teste']);

    await fixture.componentInstance.imprimirTeste();
    expect(impressora.imprimirTeste).toHaveBeenCalled();
    expect(notificacao.sucesso).toHaveBeenLastCalledWith('Teste enviado para a impressora.');
  });

  it('avisa quando o teste falha e não avisa se a pessoa desistir de escolher a porta', async () => {
    criar();
    impressora.conectar.mockResolvedValueOnce(false);
    impressora.imprimirTeste.mockRejectedValueOnce(new Error('desligada'));

    await fixture.componentInstance.conectar();
    await fixture.componentInstance.imprimirTeste();

    expect(notificacao.sucesso).not.toHaveBeenCalled();
    expect(notificacao.erro).toHaveBeenCalledWith(expect.stringContaining('Não foi possível imprimir'));
  });

  it('liga e desliga a impressão automática', () => {
    criar();
    const caixa = elemento.querySelector<HTMLInputElement>('.impressora__automatica input')!;
    expect(caixa.checked).toBe(true);

    caixa.click();
    fixture.detectChanges();

    expect(impressora.alternarAutomatica).toHaveBeenCalled();
    expect(caixa.checked).toBe(false);
  });
});
