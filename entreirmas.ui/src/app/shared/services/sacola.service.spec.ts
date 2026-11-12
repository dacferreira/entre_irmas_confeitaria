import { TestBed } from '@angular/core/testing';
import { BRIGADEIRO, criarProduto } from '@testing/dados-de-teste';
import { CHAVE_SACOLA, QUANTIDADE_MAXIMA, SacolaService } from './sacola.service';

describe('SacolaService', () => {
  let servico: SacolaService;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({});
    servico = TestBed.inject(SacolaService);
  });

  it('começa vazia', () => {
    expect(servico.vazia()).toBe(true);
    expect(servico.total()).toBe(0);
  });

  it('adiciona com a quantidade mínima do produto e soma ao repetir', () => {
    servico.adicionar(BRIGADEIRO);
    servico.adicionar(BRIGADEIRO, 5);

    expect(servico.quantidadeDe(BRIGADEIRO.id)).toBe(30);
    expect(servico.quantidadeDeItens()).toBe(1);
    expect(servico.quantidadeDeUnidades()).toBe(30);
    expect(servico.total()).toBe(75);
  });

  it('respeita o mínimo e o máximo ao alterar a quantidade', () => {
    servico.adicionar(BRIGADEIRO);

    servico.alterarQuantidade(BRIGADEIRO.id, 3);
    expect(servico.quantidadeDe(BRIGADEIRO.id)).toBe(25);

    servico.alterarQuantidade(BRIGADEIRO.id, 99999);
    expect(servico.quantidadeDe(BRIGADEIRO.id)).toBe(QUANTIDADE_MAXIMA);

    servico.alterarQuantidade(BRIGADEIRO.id, Number.NaN);
    expect(servico.quantidadeDe(BRIGADEIRO.id)).toBe(25);
  });

  it('remove e limpa itens', () => {
    servico.adicionar(criarProduto());
    servico.adicionar(BRIGADEIRO);

    servico.remover(BRIGADEIRO.id);
    expect(servico.itens().map((i) => i.produto.id)).toEqual([1]);

    servico.limpar();
    expect(servico.vazia()).toBe(true);
  });

  it('grava no localStorage e recarrega descartando itens inválidos', () => {
    servico.adicionar(criarProduto(), 2);
    const salvo = JSON.parse(localStorage.getItem(CHAVE_SACOLA) ?? '[]');
    salvo.push({ produto: { id: 'x' }, quantidade: 1 });
    localStorage.setItem(CHAVE_SACOLA, JSON.stringify(salvo));

    TestBed.resetTestingModule();
    const recarregado = TestBed.inject(SacolaService);

    expect(recarregado.quantidadeDe(1)).toBe(2);
    expect(recarregado.quantidadeDeItens()).toBe(1);
  });

  it('ignora conteúdo corrompido no localStorage', () => {
    localStorage.setItem(CHAVE_SACOLA, '{não é json');
    TestBed.resetTestingModule();

    expect(TestBed.inject(SacolaService).vazia()).toBe(true);
  });

  it('continua funcionando quando o localStorage falha', () => {
    jest.spyOn(Storage.prototype, 'setItem').mockImplementation(() => {
      throw new Error('cota cheia');
    });

    servico.adicionar(criarProduto());

    expect(servico.quantidadeDeItens()).toBe(1);
  });

  it('abre, fecha e alterna o painel', () => {
    servico.abrir();
    expect(servico.aberta()).toBe(true);
    servico.alternar();
    expect(servico.aberta()).toBe(false);
    servico.alternar();
    servico.fechar();
    expect(servico.aberta()).toBe(false);
  });
});
