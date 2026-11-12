----------------------------------------------------------
-- 1. DDL - CRIACAO DA TABELA PRODUTO
----------------------------------------------------------
-- MySQL nao tem SEQUENCE: a chave usa AUTO_INCREMENT e a API mapeia com ValueGeneratedOnAdd().
CREATE TABLE PRODUTO (
  PROD_SQ_PRODUTO            BIGINT         NOT NULL AUTO_INCREMENT COMMENT 'Chave primaria sequencial da tabela.',
  CATE_SQ_CATEGORIA          BIGINT         NOT NULL                COMMENT 'Categoria do produto (FK para CATEGORIA).',
  PROD_NM_NOME               VARCHAR(100)   NOT NULL                COMMENT 'Nome do produto exibido no site.',
  PROD_DS_SLUG               VARCHAR(120)   NOT NULL                COMMENT 'Identificador do produto na URL (minusculas, sem acentos).',
  PROD_TX_DESCRICAO          VARCHAR(1000)  NOT NULL                COMMENT 'Descricao do produto: sabor, recheio, tamanho.',
  PROD_NR_PRECO              DECIMAL(10,2)  NOT NULL                COMMENT 'Preco de venda por unidade de venda, em reais.',
  PROD_DS_UNIDADE            VARCHAR(40)    NOT NULL                COMMENT 'Unidade de venda (unidade, fatia, cento, caixa com 4...).',
  PROD_NR_QUANTIDADE_MINIMA  INT            NOT NULL DEFAULT 1      COMMENT 'Quantidade minima aceita por pedido.',
  PROD_CD_ILUSTRACAO         VARCHAR(30)    NOT NULL                COMMENT 'Codigo da ilustracao usada pelo site quando nao ha foto.',
  PROD_DS_URL_IMAGEM         VARCHAR(500)   NULL                    COMMENT 'URL da foto do produto (opcional).',
  PROD_IN_DESTAQUE           TINYINT(1)     NOT NULL DEFAULT 0      COMMENT 'Indica se o produto aparece nos destaques: 0 nao, 1 sim.',
  PROD_NM_CADASTRADO_POR     VARCHAR(30)    NOT NULL                COMMENT 'Usuario ou rotina que cadastrou o registro.',
  PROD_IN_ATIVO              TINYINT        NOT NULL DEFAULT 1      COMMENT 'Situacao do registro: 0 inativo, 1 ativo, 2 arquivado.',
  PROD_DT_CADASTRO           DATETIME(6)    NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'Data e hora do cadastro.',
  CONSTRAINT PK_PROD PRIMARY KEY (PROD_SQ_PRODUTO)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--------------------------------------------------------
-- 2. DDL for Index
--------------------------------------------------------

CREATE UNIQUE INDEX UK_PROD_SLUG ON PRODUTO (PROD_DS_SLUG);
CREATE INDEX IX_PROD_CATEGORIA ON PRODUTO (CATE_SQ_CATEGORIA, PROD_IN_ATIVO);

----------------------------------------------------------
-- 3. DESCRICAO DOS CAMPOS (COMMENTS)
----------------------------------------------------------
ALTER TABLE PRODUTO COMMENT = 'Produtos do cardapio da confeitaria.';

--------------------------------------------------------
-- 4. Constraints for Table PRODUTO
--------------------------------------------------------

ALTER TABLE PRODUTO ADD CONSTRAINT FK_PROD_CATE FOREIGN KEY (CATE_SQ_CATEGORIA) REFERENCES CATEGORIA (CATE_SQ_CATEGORIA);
ALTER TABLE PRODUTO ADD CONSTRAINT CK_PROD_PRECO CHECK (PROD_NR_PRECO > 0);
ALTER TABLE PRODUTO ADD CONSTRAINT CK_PROD_QUANTIDADE_MINIMA CHECK (PROD_NR_QUANTIDADE_MINIMA >= 1);

----------------------------------------------------------
-- 5. SEQUENCES
----------------------------------------------------------
-- Nao se aplica ao MySQL (ver AUTO_INCREMENT na secao 1).
