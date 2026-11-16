import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, DestroyRef, ElementRef, OnInit, inject, input, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MascaraCepDirective, somenteDigitos } from '@directives/mascara-cep.directive';
import { EnderecoCep, EnderecoPedido } from '@models/endereco.model';
import { EnderecoService } from '@services/endereco.service';
import { catchError, debounceTime, distinctUntilChanged, filter, map, of, switchMap, tap } from 'rxjs';

export const UFS = [
  'AC', 'AL', 'AM', 'AP', 'BA', 'CE', 'DF', 'ES', 'GO', 'MA', 'MG', 'MS', 'MT', 'PA',
  'PB', 'PE', 'PI', 'PR', 'RJ', 'RN', 'RO', 'RR', 'RS', 'SC', 'SE', 'SP', 'TO',
] as const;

export type FormularioEndereco = FormGroup<{
  cep: FormControl<string>;
  logradouro: FormControl<string>;
  numero: FormControl<string>;
  complemento: FormControl<string>;
  bairro: FormControl<string>;
  cidade: FormControl<string>;
  uf: FormControl<string>;
}>;

export const MENSAGEM_CEP_NAO_ENCONTRADO = 'CEP não encontrado. Confira o número ou preencha o endereço.';
export const MENSAGEM_CEP_INDISPONIVEL = 'Não foi possível buscar o CEP agora. Preencha o endereço manualmente.';

const cepValidator = (controle: AbstractControl<string>): ValidationErrors | null =>
  controle.value && somenteDigitos(controle.value).length !== 8 ? { cep: true } : null;

/** Cria o grupo de campos do endereço (os mesmos limites da API). */
export function criarFormularioEndereco(fb: NonNullableFormBuilder): FormularioEndereco {
  return fb.group({
    cep: ['', [Validators.required, cepValidator]],
    logradouro: ['', [Validators.required, Validators.maxLength(150)]],
    numero: ['', [Validators.required, Validators.maxLength(20)]],
    complemento: ['', [Validators.maxLength(100)]],
    bairro: ['', [Validators.required, Validators.maxLength(100)]],
    cidade: ['', [Validators.required, Validators.maxLength(100)]],
    uf: ['', [Validators.required]],
  });
}

/** Valores do grupo no formato que a API espera (CEP só com dígitos, textos aparados). */
export function enderecoDoFormulario(grupo: FormularioEndereco): EnderecoPedido {
  const valores = grupo.getRawValue();
  return {
    cep: somenteDigitos(valores.cep),
    logradouro: valores.logradouro.trim(),
    numero: valores.numero.trim(),
    complemento: valores.complemento.trim() || null,
    bairro: valores.bairro.trim(),
    cidade: valores.cidade.trim(),
    uf: valores.uf,
  };
}

/**
 * Campos do endereço de entrega. Ao completar o CEP, busca o endereço (ViaCEP, pela API) e preenche
 * rua, bairro, cidade e UF; depois leva o cursor para o número. Se o CEP não existir ou o serviço falhar,
 * avisa ao lado do campo e deixa tudo para preencher à mão.
 */
@Component({
  selector: 'eirm-campos-endereco',
  imports: [ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatSelectModule, MascaraCepDirective],
  templateUrl: './campos-endereco.component.html',
  styleUrl: './campos-endereco.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CamposEnderecoComponent implements OnInit {
  private readonly enderecoService = inject(EnderecoService);
  private readonly elemento = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly destroyRef = inject(DestroyRef);

  readonly grupo = input.required<FormularioEndereco>();

  readonly ufs = UFS;
  readonly buscando = signal(false);
  readonly erroCep = signal('');
  /** Texto lido por leitores de tela quando a busca termina. */
  readonly situacao = signal('');

  ngOnInit(): void {
    const cep = this.grupo().controls.cep;

    cep.valueChanges
      .pipe(
        map((valor) => somenteDigitos(valor)),
        // Repete o mesmo valor (ex.: máscara) sem nova busca; apagar e redigitar o CEP busca de novo.
        distinctUntilChanged(),
        tap(() => this.erroCep.set('')),
        debounceTime(300),
        filter((digitos) => digitos.length === 8),
        tap(() => {
          this.buscando.set(true);
          this.situacao.set('Buscando o endereço…');
        }),
        // switchMap: se a pessoa trocar o CEP no meio da busca, vale só o último.
        switchMap((digitos) =>
          this.enderecoService.consultarCep(digitos).pipe(
            map((endereco): EnderecoCep | string => endereco),
            catchError((erro: unknown) =>
              of(erro instanceof HttpErrorResponse && erro.status === 404 ? MENSAGEM_CEP_NAO_ENCONTRADO : MENSAGEM_CEP_INDISPONIVEL),
            ),
          ),
        ),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((resultado) => {
        this.buscando.set(false);
        if (typeof resultado === 'string') {
          this.erroCep.set(resultado);
          this.situacao.set(resultado);
          return;
        }
        this.preencher(resultado);
      });
  }

  private preencher(endereco: EnderecoCep): void {
    const controles = this.grupo().controls;
    controles.logradouro.setValue(endereco.logradouro);
    controles.bairro.setValue(endereco.bairro);
    controles.cidade.setValue(endereco.cidade);
    controles.uf.setValue(endereco.uf);
    // O complemento do ViaCEP fala do trecho da rua ("lado ímpar"); só entra se a pessoa não escreveu nada.
    if (!controles.complemento.value && endereco.complemento) {
      controles.complemento.setValue(endereco.complemento);
    }
    this.situacao.set(`Endereço encontrado: ${endereco.logradouro}, ${endereco.bairro}. Informe o número.`);

    // CEP de cidade pequena vem sem rua: o cursor vai para onde ainda falta preencher.
    const proximo = endereco.logradouro ? 'numero' : 'logradouro';
    queueMicrotask(() =>
      this.elemento.nativeElement.querySelector<HTMLInputElement>(`input[formControlName="${proximo}"]`)?.focus(),
    );
  }
}
