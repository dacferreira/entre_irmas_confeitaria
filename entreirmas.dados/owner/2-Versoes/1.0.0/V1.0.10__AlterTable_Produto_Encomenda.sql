-- -------------------------------------------------------
-- 1. DDL - PRODUTO SOB ENCOMENDA
-- -------------------------------------------------------
-- Separa os produtos feitos sob encomenda (bolos inteiros, tortas, docinhos de festa...) dos que a
-- confeitaria tem para pronta entrega. No cardapio, os de encomenda aparecem por ultimo.
-- A antecedencia minima do pedido (PEDIDO_ANTECEDENCIA_MINIMA_DIAS na API) so vale quando o pedido
-- tem algum produto de encomenda; sem eles, o pedido pode ser para o mesmo dia.
-- Os produtos ja cadastrados comecam sem encomenda (DEFAULT 0); marque na area administrativa.
ALTER TABLE PRODUTO
  ADD COLUMN PROD_IN_ENCOMENDA TINYINT(1) NOT NULL DEFAULT 0
  COMMENT 'Indica se o produto e feito sob encomenda (exige antecedencia): 0 nao, 1 sim.'
  AFTER PROD_IN_DESTAQUE;

-- -------------------------------------------------------
-- 2. DDL for Index
-- -------------------------------------------------------
-- Sem indices novos (o cardapio e lido inteiro e ordenado na API).

-- -------------------------------------------------------
-- 3. DESCRICAO DOS CAMPOS (COMMENTS)
-- -------------------------------------------------------
-- Comentario na propria coluna (secao 1).

-- -------------------------------------------------------
-- 4. Constraints
-- -------------------------------------------------------

ALTER TABLE PRODUTO ADD CONSTRAINT CK_PROD_ENCOMENDA CHECK (PROD_IN_ENCOMENDA IN (0, 1));

-- -------------------------------------------------------
-- 5. SEQUENCES
-- -------------------------------------------------------
-- Nao se aplica ao MySQL.
