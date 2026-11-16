import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { ComponentFixture, TestBed, fakeAsync, flushMicrotasks, tick } from '@angular/core/testing';
import { NonNullableFormBuilder } from '@angular/forms';
import { EnderecoCep } from '@models/endereco.model';
import { EnderecoService } from '@services/endereco.service';
import { Subject, of, throwError } from 'rxjs';
import {
  CamposEnderecoComponent,
  MENSAGEM_CEP_INDISPONIVEL,
  MENSAGEM_CEP_NAO_ENCONTRADO,
  criarFormularioEndereco,
  enderecoDoFormulario,
} from './campos-endereco.component';

@Component({
  imports: [CamposEnderecoComponent],
  template: `<eirm-campos-endereco [grupo]="grupo" />`,
})
class HospedeiroComponent {
  readonly grupo = criarFormularioEndereco(inject(NonNullableFormBuilder));
}

const PRACA_DA_SE: EnderecoCep = {
  cep: '01001000',
  logradouro: 'Praça da Sé',
  complemento: 'lado ímpar',
  bairro: 'Sé',
  cidade: 'São Paulo',
  uf: 'SP',
};

describe('CamposEnderecoComponent', () => {
  const enderecoService = { consultarCep: jest.fn() };
  let fixture: ComponentFixture<HospedeiroComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [{ provide: EnderecoService, useValue: enderecoService }] });
    fixture = TestBed.createComponent(HospedeiroComponent);
    fixture.detectChanges();
  });

  const grupo = () => fixture.componentInstance.grupo;
  const elemento = (): HTMLElement => fixture.nativeElement;

  function digitarCep(valor: string): void {
    const campo = elemento().querySelector<HTMLInputElement>('input[formControlName="cep"]')!;
    campo.value = valor;
    campo.dispatchEvent(new Event('input'));
  }

  it('ao completar o CEP preenche o endereço e leva o cursor para o número', fakeAsync(() => {
    enderecoService.consultarCep.mockReturnValue(of(PRACA_DA_SE));

    digitarCep('01001000');
    tick(300);
    flushMicrotasks();
    fixture.detectChanges();

    expect(enderecoService.consultarCep).toHaveBeenCalledWith('01001000');
    expect(grupo().getRawValue()).toEqual({
      cep: '01001-000',
      logradouro: 'Praça da Sé',
      numero: '',
      complemento: 'lado ímpar',
      bairro: 'Sé',
      cidade: 'São Paulo',
      uf: 'SP',
    });
    expect(document.activeElement?.getAttribute('formControlName')).toBe('numero');
    expect(elemento().querySelector('#cep-situacao')?.textContent).toContain('Endereço encontrado');
  }));

  it('não troca o complemento que a pessoa já escreveu', fakeAsync(() => {
    enderecoService.consultarCep.mockReturnValue(of(PRACA_DA_SE));
    grupo().controls.complemento.setValue('Apto 2');

    digitarCep('01001000');
    tick(300);
    flushMicrotasks();

    expect(grupo().controls.complemento.value).toBe('Apto 2');
  }));

  it('sem rua no retorno (CEP de cidade), o cursor vai para a rua', fakeAsync(() => {
    enderecoService.consultarCep.mockReturnValue(of({ ...PRACA_DA_SE, logradouro: '', bairro: '' }));

    digitarCep('69900000');
    tick(300);
    flushMicrotasks();

    expect(document.activeElement?.getAttribute('formControlName')).toBe('logradouro');
  }));

  it('só busca com 8 dígitos, espera a pessoa parar de digitar e não repete o mesmo CEP', fakeAsync(() => {
    enderecoService.consultarCep.mockReturnValue(of(PRACA_DA_SE));

    digitarCep('0100100');
    tick(300);
    expect(enderecoService.consultarCep).not.toHaveBeenCalled();

    digitarCep('01001000');
    tick(100);
    expect(enderecoService.consultarCep).not.toHaveBeenCalled();
    tick(200);
    grupo().controls.cep.setValue('01001-000');
    tick(300);
    flushMicrotasks();

    expect(enderecoService.consultarCep).toHaveBeenCalledTimes(1);
  }));

  it('mostra o carregamento enquanto busca', fakeAsync(() => {
    const resposta = new Subject<EnderecoCep>();
    enderecoService.consultarCep.mockReturnValue(resposta);

    digitarCep('01001000');
    tick(300);
    fixture.detectChanges();

    expect(elemento().querySelector('.girando')).not.toBeNull();
    expect(elemento().querySelector('input[formControlName="cep"]')?.getAttribute('aria-busy')).toBe('true');

    resposta.next(PRACA_DA_SE);
    resposta.complete();
    flushMicrotasks();
    fixture.detectChanges();

    expect(elemento().querySelector('.girando')).toBeNull();
  }));

  it.each([
    [404, MENSAGEM_CEP_NAO_ENCONTRADO],
    [503, MENSAGEM_CEP_INDISPONIVEL],
    [0, MENSAGEM_CEP_INDISPONIVEL],
  ])('com erro %s avisa ao lado do campo e libera o preenchimento manual', fakeAsync((status: number, mensagem: string) => {
    enderecoService.consultarCep.mockReturnValue(throwError(() => new HttpErrorResponse({ status })));

    digitarCep('99999999');
    tick(300);
    fixture.detectChanges();

    expect(elemento().querySelector('[role="alert"]')?.textContent?.trim()).toBe(mensagem);
    expect(elemento().querySelector('.girando')).toBeNull();
    expect(grupo().controls.logradouro.enabled).toBe(true);

    // Corrigir o CEP limpa o aviso e a busca continua funcionando depois do erro.
    enderecoService.consultarCep.mockReturnValue(of(PRACA_DA_SE));
    digitarCep('01001000');
    fixture.detectChanges();
    expect(elemento().querySelector('[role="alert"]')).toBeNull();
    tick(300);
    flushMicrotasks();
    expect(grupo().controls.logradouro.value).toBe('Praça da Sé');
  }));

  it('valida os campos obrigatórios e o tamanho do CEP', () => {
    expect(grupo().controls.cep.hasError('required')).toBe(true);
    grupo().controls.cep.setValue('0100');
    expect(grupo().controls.cep.hasError('cep')).toBe(true);
    grupo().controls.cep.setValue('01001-000');
    expect(grupo().controls.cep.valid).toBe(true);
    expect(grupo().controls.complemento.valid).toBe(true);
    expect(grupo().controls.numero.hasError('required')).toBe(true);
  });

  it('monta o endereço para a API com o CEP só em dígitos e textos aparados', () => {
    grupo().setValue({
      cep: '01001-000',
      logradouro: ' Praça da Sé ',
      numero: ' 10 ',
      complemento: '  ',
      bairro: 'Sé',
      cidade: 'São Paulo',
      uf: 'SP',
    });

    expect(enderecoDoFormulario(grupo())).toEqual({
      cep: '01001000',
      logradouro: 'Praça da Sé',
      numero: '10',
      complemento: null,
      bairro: 'Sé',
      cidade: 'São Paulo',
      uf: 'SP',
    });
  });
});
