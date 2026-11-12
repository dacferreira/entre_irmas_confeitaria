----------------------------------------------------------
-- 1. DDL - CRIACAO DA TABELA ITEM_PEDIDO
----------------------------------------------------------
-- MySQL nao tem SEQUENCE: a chave usa AUTO_INCREMENT e a API mapeia com ValueGeneratedOnAdd().
-- Nome, unidade e preco do produto sao copiados para o historico nao mudar quando o cardapio mudar.
CREATE TABLE ITEM_PEDIDO (
  ITPE_SQ_ITEM_PEDIDO        BIGINT         NOT NULL AUTO_INCREMENT COMMENT 'Chave primaria sequencial da tabela.',
  PEDI_SQ_PEDIDO             BIGINT         NOT NULL                COMMENT 'Pedido ao qual o item pertence (FK para PEDIDO).',
  PROD_SQ_PRODUTO            BIGINT         NOT NULL                COMMENT 'Produto pedido (FK para PRODUTO).',
  ITPE_NM_PRODUTO            VARCHAR(100)   NOT NULL                COMMENT 'Nome do produto no momento do pedido.',
  ITPE_DS_UNIDADE            VARCHAR(40)    NOT NULL                COMMENT 'Unidade de venda no momento do pedido.',
  ITPE_NR_QUANTIDADE         INT            NOT NULL                COMMENT 'Quantidade pedida.',
  ITPE_NR_PRECO_UNITARIO     DECIMAL(10,2)  NOT NULL                COMMENT 'Preco unitario no momento do pedido, em reais.',
  ITPE_NR_VALOR_TOTAL        DECIMAL(10,2)  NOT NULL                COMMENT 'Quantidade x preco unitario, em reais.',
  ITPE_NM_CADASTRADO_POR     VARCHAR(30)    NOT NULL                COMMENT 'Usuario ou rotina que cadastrou o registro.',
  ITPE_IN_ATIVO              TINYINT        NOT NULL DEFAULT 1      COMMENT 'Situacao do registro: 0 inativo, 1 ativo, 2 arquivado.',
  ITPE_DT_CADASTRO           DATETIME(6)    NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'Data e hora do cadastro.',
  CONSTRAINT PK_ITPE PRIMARY KEY (ITPE_SQ_ITEM_PEDIDO)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--------------------------------------------------------
-- 2. DDL for Index
--------------------------------------------------------

CREATE INDEX IX_ITPE_PEDIDO ON ITEM_PEDIDO (PEDI_SQ_PEDIDO);
CREATE INDEX IX_ITPE_PRODUTO ON ITEM_PEDIDO (PROD_SQ_PRODUTO);

----------------------------------------------------------
-- 3. DESCRICAO DOS CAMPOS (COMMENTS)
----------------------------------------------------------
ALTER TABLE ITEM_PEDIDO COMMENT = 'Itens de cada pedido feito pelo site.';

--------------------------------------------------------
-- 4. Constraints for Table ITEM_PEDIDO
--------------------------------------------------------

ALTER TABLE ITEM_PEDIDO ADD CONSTRAINT FK_ITPE_PEDI FOREIGN KEY (PEDI_SQ_PEDIDO) REFERENCES PEDIDO (PEDI_SQ_PEDIDO);
ALTER TABLE ITEM_PEDIDO ADD CONSTRAINT FK_ITPE_PROD FOREIGN KEY (PROD_SQ_PRODUTO) REFERENCES PRODUTO (PROD_SQ_PRODUTO);
ALTER TABLE ITEM_PEDIDO ADD CONSTRAINT CK_ITPE_QUANTIDADE CHECK (ITPE_NR_QUANTIDADE >= 1);

----------------------------------------------------------
-- 5. SEQUENCES
----------------------------------------------------------
-- Nao se aplica ao MySQL (ver AUTO_INCREMENT na secao 1).
