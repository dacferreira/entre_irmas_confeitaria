-- -------------------------------------------------------
-- 1. DDL - ENTREGA POR PARCEIRO (UBER / 99)
-- -------------------------------------------------------
-- O checkout calcula a entrega na Uber e na 99 e cobra do cliente a mais barata, somada ao total.
-- Quando o pedido fica Pronto, a API chama o entregador do parceiro escolhido e guarda o id e o
-- link de rastreio. PEDI_NR_VALOR_TOTAL continua sendo so a soma dos itens (o painel e o lucro
-- nao mudam); o cliente paga PEDI_NR_VALOR_TOTAL + PEDI_NR_TAXA_ENTREGA.
-- Todas as colunas novas sao nulas: retirada, pedidos antigos e entregas com taxa combinada nao tem parceiro.
ALTER TABLE PEDIDO
  ADD COLUMN PEDI_NR_TAXA_ENTREGA      DECIMAL(10,2) NULL
    COMMENT 'Taxa de entrega cobrada do cliente (cotacao mais barata no envio do pedido).' AFTER PEDI_NR_VALOR_TOTAL,
  ADD COLUMN PEDI_CD_PARCEIRO_ENTREGA  VARCHAR(20)   NULL
    COMMENT 'Parceiro da entrega: Uber ou NoventaENove (99).' AFTER PEDI_NR_TAXA_ENTREGA,
  ADD COLUMN PEDI_CD_SITUACAO_ENTREGA  VARCHAR(20)   NULL
    COMMENT 'Chamada do entregador: Solicitada ou Falhou (nula enquanto nao foi chamado).' AFTER PEDI_CD_PARCEIRO_ENTREGA,
  ADD COLUMN PEDI_CD_ENTREGA_PARCEIRO  VARCHAR(100)  NULL
    COMMENT 'Id da entrega no sistema do parceiro.' AFTER PEDI_CD_SITUACAO_ENTREGA,
  ADD COLUMN PEDI_DS_RASTREIO_ENTREGA  VARCHAR(500)  NULL
    COMMENT 'Link para acompanhar o entregador.' AFTER PEDI_CD_ENTREGA_PARCEIRO,
  ADD COLUMN PEDI_TX_ERRO_ENTREGA      VARCHAR(300)  NULL
    COMMENT 'Motivo da ultima falha ao chamar o entregador.' AFTER PEDI_DS_RASTREIO_ENTREGA;

-- -------------------------------------------------------
-- 2. DDL for Index
-- -------------------------------------------------------
-- Sem indices novos.

-- -------------------------------------------------------
-- 3. DESCRICAO DOS CAMPOS (COMMENTS)
-- -------------------------------------------------------
-- Comentarios nas proprias colunas (secao 1).

-- -------------------------------------------------------
-- 4. Constraints
-- -------------------------------------------------------
ALTER TABLE PEDIDO ADD CONSTRAINT CK_PEDI_TAXA_ENTREGA CHECK (PEDI_NR_TAXA_ENTREGA IS NULL OR PEDI_NR_TAXA_ENTREGA >= 0);
ALTER TABLE PEDIDO ADD CONSTRAINT CK_PEDI_PARCEIRO_ENTREGA
  CHECK (PEDI_CD_PARCEIRO_ENTREGA IS NULL OR PEDI_CD_PARCEIRO_ENTREGA IN ('Uber', 'NoventaENove'));
ALTER TABLE PEDIDO ADD CONSTRAINT CK_PEDI_SITUACAO_ENTREGA
  CHECK (PEDI_CD_SITUACAO_ENTREGA IS NULL OR PEDI_CD_SITUACAO_ENTREGA IN ('Solicitada', 'Falhou'));

-- -------------------------------------------------------
-- 5. SEQUENCES
-- -------------------------------------------------------
-- Nao se aplica ao MySQL.
