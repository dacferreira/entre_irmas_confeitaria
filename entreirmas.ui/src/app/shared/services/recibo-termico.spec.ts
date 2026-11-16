import { criarPedidoDetalhe } from '@testing/dados-admin';
import { LARGURA_RECIBO, montarRecibo, montarReciboDeTeste, paraTextoDaImpressora, quebrarEmLinhas } from './recibo-termico';

/** Só o texto impresso, uma linha por item (sem os comandos ESC/POS). */
function linhasDoTexto(bytes: Uint8Array): string[] {
  // Bytes de parâmetro depois de ESC/GS + letra do comando: ESC @ (0), ESC a/E/d (1), GS ! (1), GS V (2).
  const parametros: Record<string, number> = { '@': 0, a: 1, E: 1, d: 1, '!': 1, V: 2 };
  let texto = '';
  for (let i = 0; i < bytes.length; i++) {
    if (bytes[i] === 0x1b || bytes[i] === 0x1d) {
      i += 1 + parametros[String.fromCharCode(bytes[i + 1])];
      continue;
    }
    texto += String.fromCharCode(bytes[i]);
  }
  return texto.split('\n').filter((linha) => linha.trim().length > 0);
}

describe('recibo térmico', () => {
  it('troca acentos e símbolos por ASCII', () => {
    expect(paraTextoDaImpressora('Ação, pão de mel × 2 — R$ 10,00 “ok” ✓')).toBe('Acao, pao de mel x 2 - R$ 10,00 "ok" ?');
  });

  it('quebra o texto sem cortar palavras, exceto as maiores que a linha', () => {
    expect(quebrarEmLinhas('Bolo de pote de ninho com morango', 16)).toEqual(['Bolo de pote de', 'ninho com', 'morango']);
    expect(quebrarEmLinhas('ab ' + 'x'.repeat(20), 8)).toEqual(['ab', 'xxxxxxxx', 'xxxxxxxx', 'xxxx']);
    expect(quebrarEmLinhas('   ')).toEqual([]);
  });

  it('monta o recibo do pedido com cliente, itens e total, sem passar da largura do papel', () => {
    const pedido = criarPedidoDetalhe({
      codigo: 'EI-ABC234',
      nomeCliente: 'Maria José',
      telefone: '21999998888',
      tipoEntrega: 'Entrega',
      dataDesejada: '2026-11-20',
      endereco: 'Rua das Flores, 123, apto 401 - Centro, Niterói/RJ',
      observacao: 'Sem lactose',
    });

    const bytes = montarRecibo(pedido);
    const linhas = linhasDoTexto(bytes);

    expect(Array.from(bytes.slice(0, 2))).toEqual([0x1b, 0x40]);
    expect(linhas).toEqual(
      expect.arrayContaining([
        'ENTRE IRMAS',
        'EI-ABC234',
        'ENTREGA',
        'Para 20/11/2026',
        'Cliente: Maria Jose',
        'Telefone: (21) 99999-8888',
        '2x Bolo de Cenoura',
        'Sem lactose',
        'Pedido pelo site',
      ]),
    );
    expect(linhas.join('\n')).toContain('Endereco: Rua das Flores, 123,');
    expect(linhas.find((linha) => linha.startsWith('   R$ 45,00 cada'))).toMatch(/R\$ 90,00$/);
    // Total em letra alta: ocupa as 32 colunas.
    expect(linhas.find((linha) => linha.startsWith('TOTAL'))).toMatch(/^TOTAL +R\$ 90,00$/);
    expect(linhas.every((linha) => linha.length <= LARGURA_RECIBO)).toBe(true);
    // Termina avançando o papel e cortando.
    expect(Array.from(bytes.slice(-7))).toEqual([0x1b, 0x64, 4, 0x1d, 0x56, 0x42, 0]);
  });

  it('pedido de retirada lançado na loja, sem endereço nem observação', () => {
    const linhas = linhasDoTexto(
      montarRecibo(criarPedidoDetalhe({ tipoEntrega: 'Retirada', cadastradoPor: 'suellen', endereco: null, observacao: null })),
    );

    expect(linhas).toContain('RETIRADA');
    expect(linhas).toContain('Pedido lancado na loja');
    expect(linhas.some((linha) => linha.startsWith('Endereco'))).toBe(false);
    expect(linhas).not.toContain('Observacao:');
  });

  it('monta o recibo de teste', () => {
    const linhas = linhasDoTexto(montarReciboDeTeste());

    expect(linhas).toContain('Teste de impressao');
    expect(linhas).toContain('Esquerda' + ' '.repeat(LARGURA_RECIBO - 15) + 'Direita');
  });
});
