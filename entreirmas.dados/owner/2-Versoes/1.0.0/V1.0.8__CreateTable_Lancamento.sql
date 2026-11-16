-- -------------------------------------------------------
-- 1. DDL - CRIACAO DA TABELA LANCAMENTO
-- -------------------------------------------------------
-- MySQL nao tem SEQUENCE: a chave usa AUTO_INCREMENT e a API mapeia com ValueGeneratedOnAdd().
-- Despesas (ingredientes, embalagem, gas, entrega...) e perdas (doce descartado, encomenda desperdicada)
-- que entram no calculo do lucro do painel.
CREATE TABLE LANCAMENTO (
  LANC_SQ_LANCAMENTO         BIGINT         NOT NULL AUTO_INCREMENT COMMENT 'Chave primaria sequencial da tabela.',
  LANC_CD_TIPO               VARCHAR(20)    NOT NULL                COMMENT 'Tipo do lancamento: Despesa ou Perda.',
  LANC_DS_LANCAMENTO         VARCHAR(200)   NOT NULL                COMMENT 'Descricao do lancamento (ex.: Compra de chocolate).',
  LANC_NR_VALOR              DECIMAL(10,2)  NOT NULL                COMMENT 'Valor do lancamento, em reais.',
  LANC_DT_LANCAMENTO         DATE           NOT NULL                COMMENT 'Data em que a despesa ou a perda aconteceu.',
  LANC_NM_CADASTRADO_POR     VARCHAR(30)    NOT NULL                COMMENT 'Usuario ou rotina que cadastrou o registro.',
  LANC_IN_ATIVO              TINYINT        NOT NULL DEFAULT 1      COMMENT 'Situacao do registro: 0 inativo, 1 ativo, 2 arquivado.',
  LANC_DT_CADASTRO           DATETIME(6)    NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'Data e hora do cadastro.',
  CONSTRAINT PK_LANC PRIMARY KEY (LANC_SQ_LANCAMENTO)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- -------------------------------------------------------
-- 2. DDL for Index
-- -------------------------------------------------------

CREATE INDEX IX_LANC_DATA_TIPO ON LANCAMENTO (LANC_DT_LANCAMENTO, LANC_CD_TIPO);

-- -------------------------------------------------------
-- 3. DESCRICAO DOS CAMPOS (COMMENTS)
-- -------------------------------------------------------
ALTER TABLE LANCAMENTO COMMENT = 'Despesas e perdas da confeitaria, usadas no controle de lucro.';

-- -------------------------------------------------------
-- 4. Constraints for Table LANCAMENTO
-- -------------------------------------------------------

ALTER TABLE LANCAMENTO ADD CONSTRAINT CK_LANC_TIPO CHECK (LANC_CD_TIPO IN ('Despesa', 'Perda'));
ALTER TABLE LANCAMENTO ADD CONSTRAINT CK_LANC_VALOR CHECK (LANC_NR_VALOR > 0);

-- -------------------------------------------------------
-- 5. SEQUENCES
-- -------------------------------------------------------
-- Nao se aplica ao MySQL (ver AUTO_INCREMENT na secao 1).
