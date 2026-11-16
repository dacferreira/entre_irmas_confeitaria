-- -------------------------------------------------------
-- 1. DDL - CUSTO DOS PRODUTOS (CONTROLE DE LUCRO)
-- -------------------------------------------------------
-- O custo do produto e copiado para o item do pedido, como ja acontece com o preco:
-- mudar o custo depois nao altera o lucro dos pedidos antigos.
-- Pedidos gravados antes deste script ficam com custo 0.
ALTER TABLE PRODUTO
  ADD COLUMN PROD_NR_CUSTO DECIMAL(10,2) NOT NULL DEFAULT 0
  COMMENT 'Custo de producao por unidade de venda, em reais (ingredientes e embalagem).'
  AFTER PROD_NR_PRECO;

ALTER TABLE ITEM_PEDIDO
  ADD COLUMN ITPE_NR_CUSTO_UNITARIO DECIMAL(10,2) NOT NULL DEFAULT 0
  COMMENT 'Custo unitario do produto no momento do pedido, em reais.'
  AFTER ITPE_NR_PRECO_UNITARIO;

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

ALTER TABLE PRODUTO ADD CONSTRAINT CK_PROD_CUSTO CHECK (PROD_NR_CUSTO >= 0);
ALTER TABLE ITEM_PEDIDO ADD CONSTRAINT CK_ITPE_CUSTO_UNITARIO CHECK (ITPE_NR_CUSTO_UNITARIO >= 0);

-- -------------------------------------------------------
-- 5. SEQUENCES
-- -------------------------------------------------------
-- Nao se aplica ao MySQL.
