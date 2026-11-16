import { formatarTelefone } from '@directives/mascara-telefone.directive';
import { PedidoDetalhe } from '@models/admin.model';

/** Colunas da impressora térmica de 58mm na fonte normal. */
export const LARGURA_RECIBO = 32;

const ESC = 0x1b;
const GS = 0x1d;

/** Tamanho da letra (GS !): normal, dobro da altura ou dobro da altura e da largura. */
const TAMANHO = { normal: 0x00, alto: 0x01, grande: 0x11 } as const;

const moeda = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' });

/**
 * Recibo do pedido em comandos ESC/POS, o padrão das impressoras térmicas de recibo.
 * Sai sem acentos: cada impressora usa uma tabela de caracteres, e o texto sem acento sai igual em todas.
 */
export function montarRecibo(pedido: PedidoDetalhe): Uint8Array {
  const recibo = new Recibo();

  recibo.centralizar().tamanho('grande').negrito(true).linha('ENTRE IRMAS');
  recibo.tamanho('normal').negrito(false).linha('Confeitaria').separador();

  recibo.tamanho('grande').negrito(true).linha(pedido.codigo);
  recibo.tamanho('alto').linha(pedido.tipoEntrega === 'Entrega' ? 'ENTREGA' : 'RETIRADA');
  recibo.tamanho('normal').negrito(false).linha(`Para ${formatarData(pedido.dataDesejada)}`);
  recibo.alinharAEsquerda().separador();

  recibo.texto(`Feito em: ${formatarDataEHora(pedido.dataEHoraDeCadastro)}`);
  recibo.negrito(true).texto(`Cliente: ${pedido.nomeCliente}`).negrito(false);
  recibo.texto(`Telefone: ${formatarTelefone(pedido.telefone)}`);
  if (pedido.endereco) {
    recibo.texto(`Endereco: ${pedido.endereco}`);
  }
  recibo.separador();

  for (const item of pedido.itens) {
    recibo.texto(`${item.quantidade}x ${item.nomeProduto}`);
    recibo.colunas(`   ${moeda.format(item.precoUnitario)} cada`, moeda.format(item.valorTotal));
  }
  recibo.separador();
  recibo.tamanho('alto').negrito(true).colunas('TOTAL', moeda.format(pedido.valorTotal));
  recibo.tamanho('normal').negrito(false);

  if (pedido.observacao) {
    recibo.separador().negrito(true).linha('Observacao:').negrito(false).texto(pedido.observacao);
  }

  recibo.separador().centralizar().linha(pedido.cadastradoPor === 'SITE' ? 'Pedido pelo site' : 'Pedido lancado na loja');
  return recibo.finalizar();
}

/** Recibo curto para conferir a conexão e o corte do papel. */
export function montarReciboDeTeste(): Uint8Array {
  const recibo = new Recibo();
  recibo.centralizar().tamanho('grande').negrito(true).linha('ENTRE IRMAS');
  recibo.tamanho('normal').negrito(false).linha('Teste de impressao').separador();
  recibo.alinharAEsquerda().texto(`Impresso em ${formatarDataEHora(new Date().toISOString())}`);
  recibo.colunas('Esquerda', 'Direita');
  return recibo.finalizar();
}

/** Troca acentos e símbolos por equivalentes ASCII; o que sobrar vira "?". */
export function paraTextoDaImpressora(texto: string): string {
  return texto
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/[\u00a0\u202f\t]/g, ' ')
    .replace(/[\r\n]+/g, ' ')
    .replace(/\u00d7/g, 'x')
    .replace(/[\u201c\u201d]/g, '"')
    .replace(/[\u2018\u2019]/g, "'")
    .replace(/[\u2013\u2014]/g, '-')
    .replace(/[^\x20-\x7e]/g, '?');
}

/** Quebra o texto em linhas de até `largura` caracteres, sem cortar palavras (exceto as maiores que a linha). */
export function quebrarEmLinhas(texto: string, largura = LARGURA_RECIBO): string[] {
  const linhas: string[] = [];
  let atual = '';
  for (const palavra of texto.split(' ').filter(Boolean)) {
    let resto = palavra;
    while (resto.length > largura) {
      if (atual) {
        linhas.push(atual);
        atual = '';
      }
      linhas.push(resto.slice(0, largura));
      resto = resto.slice(largura);
    }
    if (!resto) {
      continue;
    }
    if (!atual) {
      atual = resto;
    } else if (atual.length + 1 + resto.length <= largura) {
      atual += ` ${resto}`;
    } else {
      linhas.push(atual);
      atual = resto;
    }
  }
  if (atual) {
    linhas.push(atual);
  }
  return linhas;
}

class Recibo {
  private readonly bytes: number[] = [ESC, 0x40]; // ESC @: reinicia a impressora
  private largura: number = LARGURA_RECIBO;

  centralizar(): this {
    return this.comando(ESC, 0x61, 1);
  }

  alinharAEsquerda(): this {
    return this.comando(ESC, 0x61, 0);
  }

  negrito(ligado: boolean): this {
    return this.comando(ESC, 0x45, ligado ? 1 : 0);
  }

  /** A letra grande ocupa o dobro da largura: cabem metade das colunas. */
  tamanho(tamanho: keyof typeof TAMANHO): this {
    this.largura = tamanho === 'grande' ? LARGURA_RECIBO / 2 : LARGURA_RECIBO;
    return this.comando(GS, 0x21, TAMANHO[tamanho]);
  }

  /** Uma linha só, cortada se passar da largura. */
  linha(texto: string): this {
    return this.escrever(paraTextoDaImpressora(texto).slice(0, this.largura));
  }

  /** Texto livre, quebrado em quantas linhas precisar. */
  texto(texto: string): this {
    quebrarEmLinhas(paraTextoDaImpressora(texto), this.largura).forEach((linha) => this.escrever(linha));
    return this;
  }

  /** Texto à esquerda e valor à direita na mesma linha (o valor vai para a linha de baixo se não couber). */
  colunas(esquerda: string, direita: string): this {
    const textoEsquerda = paraTextoDaImpressora(esquerda);
    const textoDireita = paraTextoDaImpressora(direita);
    const espaco = this.largura - textoEsquerda.length - textoDireita.length;
    if (espaco >= 1) {
      return this.escrever(textoEsquerda + ' '.repeat(espaco) + textoDireita);
    }
    this.texto(textoEsquerda);
    return this.escrever(textoDireita.padStart(this.largura));
  }

  separador(): this {
    return this.escrever('-'.repeat(this.largura));
  }

  /** Avança o papel para o recibo passar da lâmina de corte e corta (impressoras sem guilhotina ignoram). */
  finalizar(): Uint8Array {
    this.comando(ESC, 0x64, 4);
    this.comando(GS, 0x56, 0x42, 0);
    return Uint8Array.from(this.bytes);
  }

  private escrever(linha: string): this {
    for (let i = 0; i < linha.length; i++) {
      this.bytes.push(linha.charCodeAt(i));
    }
    this.bytes.push(0x0a);
    return this;
  }

  private comando(...bytes: number[]): this {
    this.bytes.push(...bytes);
    return this;
  }
}

function formatarData(data: string): string {
  const [ano, mes, dia] = data.split('-');
  return `${dia}/${mes}/${ano}`;
}

function formatarDataEHora(dataEHora: string): string {
  const data = new Date(dataEHora);
  if (Number.isNaN(data.getTime())) {
    return dataEHora;
  }
  const doisDigitos = (valor: number): string => String(valor).padStart(2, '0');
  return (
    `${doisDigitos(data.getDate())}/${doisDigitos(data.getMonth() + 1)}/${data.getFullYear()} ` +
    `${doisDigitos(data.getHours())}:${doisDigitos(data.getMinutes())}`
  );
}
