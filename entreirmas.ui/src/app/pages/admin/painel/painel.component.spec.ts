import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { periodoDoMes } from '@constants/admin';
import { PainelService } from '@services/admin.service';
import { criarPainel } from '@testing/dados-admin';
import { of, throwError } from 'rxjs';
import { PainelComponent } from './painel.component';

describe('PainelComponent', () => {
  const painelService = { obter: jest.fn() };
  let fixture: ComponentFixture<PainelComponent>;
  let componente: PainelComponent;
  let elemento: HTMLElement;

  function criar(): void {
    TestBed.configureTestingModule({
      providers: [provideRouter([]), { provide: PainelService, useValue: painelService }],
    });
    fixture = TestBed.createComponent(PainelComponent);
    componente = fixture.componentInstance;
    elemento = fixture.nativeElement;
    fixture.detectChanges();
  }

  it('carrega o mês corrente e mostra indicadores, gráfico e rankings', () => {
    painelService.obter.mockReturnValue(of(criarPainel()));
    criar();
    const { inicio, fim } = periodoDoMes(new Date());

    expect(painelService.obter).toHaveBeenCalledWith(inicio, fim);
    expect(elemento.querySelector('.indicador--destaque .indicador__valor')?.textContent).toContain('300');
    expect(elemento.querySelector('.indicador--positivo')?.textContent).toContain('margem de 50%');
    expect(elemento.querySelector('.aviso')?.textContent).toContain('aguardando confirmação');
    expect(elemento.querySelectorAll('.grafico__coluna').length).toBe(3);
    expect(elemento.querySelectorAll('.ranking__item').length).toBe(2);
    // Só as etapas com pedidos aparecem.
    expect(elemento.querySelectorAll('.etapas li').length).toBe(2);
  });

  it('calcula a altura das colunas e a divisão da receita', () => {
    painelService.obter.mockReturnValue(of(criarPainel()));
    criar();

    expect(componente.colunas()[1].alturaReceita).toBe(100);
    expect(componente.colunas()[1].alturaCustos).toBe(45);
    expect(componente.fatias().map((f) => Math.round(f.percentual))).toEqual([35, 10, 5, 50]);
    expect(componente.maiorVenda()).toBe(200);
  });

  it('mostra prejuízo e o estado vazio', () => {
    painelService.obter.mockReturnValue(
      of(criarPainel({ receita: 0, despesas: 0, perdas: 0, lucro: -10, margem: null, produtosMaisVendidos: [] })),
    );
    criar();

    expect(elemento.querySelector('.indicador--negativo')?.textContent).toContain('Prejuízo');
    expect(elemento.querySelector('.admin-vazio')?.textContent).toContain('Nada por aqui');
  });

  it('troca o período pelos atalhos e pelas datas, mantendo início antes do fim', () => {
    painelService.obter.mockReturnValue(of(criarPainel()));
    criar();

    componente.escolherAtalho('ano');
    expect(componente.periodo().inicio).toMatch(/-01-01$/);
    componente.escolherAtalho('mes-passado');
    componente.escolherAtalho('30-dias');
    componente.escolherAtalho('mes');
    expect(componente.periodo()).toEqual(periodoDoMes(new Date()));

    componente.alterarData('inicio', '2030-01-10');
    expect(componente.periodo()).toEqual({ inicio: '2030-01-10', fim: '2030-01-10' });
    expect(componente.atalho()).toBe('personalizado');
    componente.alterarData('fim', '');
    expect(componente.periodo().fim).toBe('2030-01-10');
  });

  it('mostra o erro e tenta de novo', fakeAsync(() => {
    painelService.obter.mockReturnValue(throwError(() => new Error('x')));
    criar();
    expect(elemento.querySelector('[role="alert"]')?.textContent).toContain('Não foi possível');

    painelService.obter.mockReturnValue(of(criarPainel()));
    componente.recarregar();
    tick();
    fixture.detectChanges();

    expect(componente.falhou()).toBe(false);
    expect(componente.painel()).not.toBeNull();
  }));

  it('formata o rótulo do eixo por dia ou por mês', () => {
    painelService.obter.mockReturnValue(of(criarPainel()));
    criar();

    expect(componente.rotuloDoPonto('2026-11-05', 'Dia')).toBe('5');
    expect(componente.rotuloDoPonto('2026-11-01', 'Mes')).toBe('nov');
  });
});
