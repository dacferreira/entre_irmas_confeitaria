-- -------------------------------------------------------
-- 1. DDL - CRIACAO DA TABELA LOJA
-- -------------------------------------------------------
-- Registro unico (LOJA_SQ_LOJA = 1) com a situacao da loja no site. A chave NAO e AUTO_INCREMENT:
-- a API sempre le e altera o id 1, que e inserido no fim deste script.
-- Fechada, o site so aceita encomendas (pedido normal, de pronta entrega, fica bloqueado).
CREATE TABLE LOJA (
  LOJA_SQ_LOJA               BIGINT         NOT NULL                COMMENT 'Chave primaria da tabela (registro unico, id 1).',
  LOJA_IN_ABERTA             TINYINT(1)     NOT NULL DEFAULT 1      COMMENT 'Loja aberta (1) ou fechada (0). Fechada, o site so aceita encomendas.',
  LOJA_TX_MENSAGEM_FECHADA   VARCHAR(200)   NULL                    COMMENT 'Aviso exibido no site com a loja fechada (ex.: Voltamos segunda as 9h).',
  LOJA_NM_ALTERADO_POR       VARCHAR(30)    NOT NULL                COMMENT 'Usuario que abriu ou fechou a loja por ultimo.',
  LOJA_DT_ALTERACAO          DATETIME(6)    NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'Data e hora da ultima abertura ou fechamento.',
  LOJA_NM_CADASTRADO_POR     VARCHAR(30)    NOT NULL                COMMENT 'Usuario ou rotina que cadastrou o registro.',
  LOJA_IN_ATIVO              TINYINT        NOT NULL DEFAULT 1      COMMENT 'Situacao do registro: 0 inativo, 1 ativo, 2 arquivado.',
  LOJA_DT_CADASTRO           DATETIME(6)    NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'Data e hora do cadastro.',
  CONSTRAINT PK_LOJA PRIMARY KEY (LOJA_SQ_LOJA)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- -------------------------------------------------------
-- 2. DDL for Index
-- -------------------------------------------------------
-- Sem indices alem da chave primaria (tabela de um registro).

-- -------------------------------------------------------
-- 3. DESCRICAO DOS CAMPOS (COMMENTS)
-- -------------------------------------------------------
ALTER TABLE LOJA COMMENT = 'Situacao da loja no site (aberta ou fechada), registro unico.';

-- -------------------------------------------------------
-- 4. Constraints for Table LOJA
-- -------------------------------------------------------

ALTER TABLE LOJA ADD CONSTRAINT CK_LOJA_REGISTRO_UNICO CHECK (LOJA_SQ_LOJA = 1);

-- -------------------------------------------------------
-- 5. SEQUENCES
-- -------------------------------------------------------
-- Nao se aplica (registro unico com id fixo).

-- -------------------------------------------------------
-- 6. CARGA INICIAL
-- -------------------------------------------------------
INSERT INTO LOJA (LOJA_SQ_LOJA, LOJA_IN_ABERTA, LOJA_NM_ALTERADO_POR, LOJA_NM_CADASTRADO_POR)
VALUES (1, 1, 'SISTEMA', 'SISTEMA');
