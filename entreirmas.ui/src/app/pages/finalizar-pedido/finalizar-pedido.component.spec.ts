import { computed, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { environment } from '@environments/environment';
import { CotacaoEntrega, NovoPedido } from '@models/pedido.model';
import { SituacaoLoja } from '@models/loja.model';
import { EnderecoService } from '@services/endereco.service';
import { EntregaService } from '@services/entrega.service';
import { LojaService } from '@services/loja.service';
import { PedidoService } from '@services/pedido.service';
import { SacolaService } from '@services/sacola.service';
import { BRIGADEIRO, ENDERECO_DIGITADO, ENDERECO_ENVIADO, criarPedido, criarProduto } from '@testing/dados-de-teste';
import { dataDaquiA } from '@validators/pedido.validators';
import { Subject, of, throwError } from 'rxjs';
import { FinalizarPedidoComponent } from './finalizar-pedido.component';

describe('FinalizarPedidoComponent', () => {
  const pedidoService = { criar: jest.fn() };
  const situacao = signal<SituacaoLoja>({ aberta: true, mensagem: null });
  const loja = { situacao, aberta: computed(() => situacao().aberta), atualizar: jest.fn() };
  const COTACAO: CotacaoEntrega = { disponivel: true, parceiro: 'NoventaENove', valor: 12.5, prazoMinutos: 30, mensagem: null };
  const entregaService = {
    cotar: jest.fn(() => of(COTACAO)),
    urlDoTrajeto: jest.fn(() => 'https://maps.google.com/maps?saddr=1,2&daddr=destino&output=embed'),
  };
  let fixture: ComponentFixture<FinalizarPedidoComponent>;
  let componente: FinalizarPedidoComponent;
  let sacola: SacolaService;
  let navegar: jest.SpyInstance;

  beforeEach(() => {
    localStorage.clear();
    situacao.set({ aberta: true, mensagem: null });
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        { provide: PedidoService, useValue: pedidoService },
        { provide: EnderecoService, useValue: { consultarCep: jest.fn() } },
        { provide: LojaService, useValue: loja },
        { provide: EntregaService, useValue: entregaService },
      ],
    });
    sacola = TestBed.inject(SacolaService);
    sacola.adicionar(criarProduto(), 2);
    sacola.adicionar(BRIGADEIRO);
    navegar = jest.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    fixture = TestBed.createComponent(FinalizarPedidoComponent);
    componente = fixture.componentInstance;
    fixture.detectChanges();
  });

  function preencher(): void {
    componente.formulario.patchValue({ nomeCliente: '  Maria ', telefone: '(21) 99999-8888', observacao: ' ' });
  }

  it('pedido normal é para hoje e sem campo de data; com encomenda aparece a data com antecedência', () => {
    const elemento: HTMLElement = fixture.nativeElement;
    const hoje = dataDaquiA(0);
    expect(loja.atualizar).toHaveBeenCalled();
    expect(componente.dataMinima()).toBe(hoje);
    expect(componente.formulario.controls.dataDesejada.disabled).toBe(true);
    expect(elemento.querySelector('input[type="date"]')).toBeNull();
    expect(elemento.querySelector('.para-hoje')?.textContent).toContain('Pedido para hoje');

    sacola.adicionar(criarProduto({ id: 9, nome: 'Bolo', encomenda: true }));
    fixture.detectChanges();

    const minima = dataDaquiA(environment.antecedenciaMinimaDias);
    expect(componente.dataMinima()).toBe(minima);
    expect(componente.formulario.controls.dataDesejada.enabled).toBe(true);
    expect(componente.formulario.controls.dataDesejada.value).toBe(minima);
    expect(elemento.querySelector('.para-hoje')).toBeNull();
    componente.formulario.controls.dataDesejada.setValue(hoje);
    expect(componente.formulario.controls.dataDesejada.hasError('dataMinima')).toBe(environment.antecedenciaMinimaDias > 0);
    expect(elemento.querySelector('mat-hint')?.textContent).toContain('sob encomenda');
  });

  it('com a loja fechada bloqueia o pedido normal e libera quando vira encomenda', () => {
    const elemento: HTMLElement = fixture.nativeElement;
    situacao.set({ aberta: false, mensagem: 'Voltamos amanhã' });
    fixture.detectChanges();

    expect(componente.bloqueado()).toBe(true);
    expect(elemento.querySelector('.fechado')?.textContent).toContain('Voltamos amanhã');
    expect(elemento.querySelector<HTMLButtonElement>('button[type="submit"]')?.disabled).toBe(true);
    componente.formulario.patchValue({ nomeCliente: 'Maria', telefone: '(21) 99999-8888' });
    componente.enviar();
    expect(pedidoService.criar).not.toHaveBeenCalled();

    sacola.adicionar(criarProduto({ id: 9, nome: 'Bolo', encomenda: true }));
    fixture.detectChanges();

    expect(componente.bloqueado()).toBe(false);
    expect(elemento.querySelector('.fechado')).toBeNull();
    expect(elemento.querySelector<HTMLButtonElement>('button[type="submit"]')?.disabled).toBe(false);
  });

  it('mostra o resumo da sacola e começa com a data mínima', () => {
    const elemento: HTMLElement = fixture.nativeElement;

    expect(elemento.querySelectorAll('.resumo__item').length).toBe(2);
    expect(componente.formulario.controls.dataDesejada.value).toBe(componente.dataMinima());
  });

  it('não envia com dados inválidos e marca os campos', () => {
    componente.enviar();
    fixture.detectChanges();

    expect(pedidoService.criar).not.toHaveBeenCalled();
    expect(componente.formulario.controls.nomeCliente.touched).toBe(true);
    expect(fixture.nativeElement.querySelectorAll('mat-error').length).toBeGreaterThan(0);
  });

  it('mostra e exige os campos do endereço só na entrega; na retirada mostra o endereço da loja', () => {
    preencher();
    expect(componente.formulario.controls.endereco.disabled).toBe(true);
    expect(componente.formulario.valid).toBe(true);
    const retirada = (): HTMLElement | null => fixture.nativeElement.querySelector('.retirada');
    expect(retirada()?.textContent).toContain('Rua A, 41 - Paciência, Rio de Janeiro - RJ, 23580-250');
    expect(retirada()?.querySelector('a')?.getAttribute('href')).toContain('google.com/maps/search');

    componente.formulario.controls.tipoEntrega.setValue('Entrega');
    fixture.detectChanges();

    expect(retirada()).toBeNull();
    expect(fixture.nativeElement.querySelector('eirm-campos-endereco input[formControlName="cep"]')).not.toBeNull();
    expect(componente.formulario.controls.endereco.controls.cep.hasError('required')).toBe(true);
    expect(componente.formulario.valid).toBe(false);

    componente.formulario.controls.tipoEntrega.setValue('Retirada');
    fixture.detectChanges();
    expect(componente.formulario.valid).toBe(true);
    expect(fixture.nativeElement.querySelector('eirm-campos-endereco')).toBeNull();
  });

  it('envia o pedido montado, limpa a sacola e vai para a confirmação', () => {
    pedidoService.criar.mockReturnValue(of(criarPedido()));
    preencher();
    componente.formulario.controls.tipoEntrega.setValue('Entrega');
    componente.formulario.controls.endereco.setValue(ENDERECO_DIGITADO);
    componente.calcularEntrega();

    componente.enviar();

    const enviado: NovoPedido = pedidoService.criar.mock.calls[0][0];
    expect(enviado).toEqual({
      nomeCliente: 'Maria',
      telefone: '21999998888',
      tipoEntrega: 'Entrega',
      endereco: ENDERECO_ENVIADO,
      dataDesejada: componente.dataMinima(),
      observacao: null,
      itens: [
        { produtoId: 1, quantidade: 2 },
        { produtoId: 2, quantidade: 25 },
      ],
    });
    expect(sacola.vazia()).toBe(true);
    expect(navegar).toHaveBeenCalledWith(['/', 'pedido-confirmado']);
    expect(componente.enviando()).toBe(false);
  });

  it('calcula a entrega pelo botão, mostra o parceiro, o trajeto e soma a taxa no total', () => {
    const elemento: HTMLElement = fixture.nativeElement;
    preencher();
    componente.formulario.controls.tipoEntrega.setValue('Entrega');
    fixture.detectChanges();
    const totalDaSacola = sacola.total();
    expect(elemento.querySelector('.resumo__entrega')?.textContent).toContain('calcule');

    componente.formulario.controls.endereco.setValue(ENDERECO_DIGITADO);
    elemento.querySelector<HTMLButtonElement>('.entrega__calcular')!.click();
    fixture.detectChanges();

    expect(entregaService.cotar).toHaveBeenCalledWith(ENDERECO_ENVIADO);
    expect(elemento.querySelector('.entrega__resultado')?.textContent).toContain('Entrega pela 99');
    expect(elemento.querySelector('.entrega__resultado')?.textContent).toContain('30 min');
    expect(elemento.querySelector<HTMLIFrameElement>('.entrega__mapa')?.src).toContain('maps.google.com');
    expect(elemento.querySelector('.resumo__entrega')?.textContent).toContain('Entrega (99)');
    expect(componente.taxaEntrega()).toBe(12.5);
    expect(componente.total()).toBe(totalDaSacola + 12.5);
    expect(elemento.querySelector('.entrega__calcular')?.textContent).toContain('Calcular de novo');

    // Mudou o endereço: o cálculo e o mapa somem, e o total volta a ser só a sacola.
    componente.formulario.controls.endereco.controls.numero.setValue('99');
    fixture.detectChanges();
    expect(componente.cotacao()).toBeNull();
    expect(elemento.querySelector('.entrega__mapa')).toBeNull();
    expect(componente.total()).toBe(totalDaSacola);
  });

  it('na entrega, só envia depois de calcular; com endereço incompleto não consulta', () => {
    const elemento: HTMLElement = fixture.nativeElement;
    preencher();
    componente.formulario.controls.tipoEntrega.setValue('Entrega');
    fixture.detectChanges();

    componente.calcularEntrega();
    expect(entregaService.cotar).not.toHaveBeenCalled();
    expect(componente.formulario.controls.endereco.controls.cep.touched).toBe(true);

    componente.formulario.controls.endereco.setValue(ENDERECO_DIGITADO);
    componente.enviar();
    fixture.detectChanges();

    expect(pedidoService.criar).not.toHaveBeenCalled();
    expect(elemento.querySelector('.entrega__erro')?.textContent).toContain('Calcule a entrega');

    pedidoService.criar.mockReturnValue(of(criarPedido()));
    componente.calcularEntrega();
    fixture.detectChanges();
    expect(elemento.querySelector('.entrega__erro')).toBeNull();
    componente.enviar();
    expect(pedidoService.criar).toHaveBeenCalled();
  });

  it('sem parceiro para o endereço mostra o aviso, não cobra taxa e deixa enviar', () => {
    const elemento: HTMLElement = fixture.nativeElement;
    entregaService.cotar.mockReturnValueOnce(
      of({ disponivel: false, parceiro: null, valor: null, prazoMinutos: null, mensagem: 'A taxa será combinada pelo WhatsApp.' }),
    );
    pedidoService.criar.mockReturnValue(of(criarPedido()));
    preencher();
    componente.formulario.controls.tipoEntrega.setValue('Entrega');
    componente.formulario.controls.endereco.setValue(ENDERECO_DIGITADO);

    componente.calcularEntrega();
    fixture.detectChanges();

    expect(elemento.querySelector('.entrega__aviso')?.textContent).toContain('combinada pelo WhatsApp');
    expect(componente.taxaEntrega()).toBeNull();
    expect(elemento.querySelector('.resumo__nota')?.textContent).toContain('combinada à parte');
    componente.enviar();
    expect(pedidoService.criar).toHaveBeenCalled();
  });

  it('mantém a sacola quando o envio falha e bloqueia envio duplo', () => {
    const resposta$ = new Subject<never>();
    pedidoService.criar.mockReturnValue(resposta$);
    preencher();

    componente.enviar();
    componente.enviar();
    fixture.detectChanges();
    expect(pedidoService.criar).toHaveBeenCalledTimes(1);
    expect(fixture.nativeElement.querySelector('.enviar')?.textContent).toContain('Enviando');

    resposta$.error(new Error('falhou'));
    expect(componente.enviando()).toBe(false);
    expect(sacola.vazia()).toBe(false);
  });

  it('abre a sacola para editar', () => {
    fixture.nativeElement.querySelector('.resumo__editar').click();

    expect(sacola.aberta()).toBe(true);
  });

  it('não envia se a sacola esvaziou', () => {
    pedidoService.criar.mockReturnValue(throwError(() => new Error('x')));
    preencher();
    sacola.limpar();

    componente.enviar();

    expect(pedidoService.criar).not.toHaveBeenCalled();
  });
});
