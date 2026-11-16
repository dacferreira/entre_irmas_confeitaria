-- -------------------------------------------------------
-- 1. DDL - CRIACAO DA TABELA PEDIDO
-- -------------------------------------------------------
-- MySQL nao tem SEQUENCE: a chave usa AUTO_INCREMENT e a API mapeia com ValueGeneratedOnAdd().
CREATE TABLE PEDIDO (
  PEDI_SQ_PEDIDO             BIGINT         NOT NULL AUTO_INCREMENT COMMENT 'Chave primaria sequencial da tabela.',
  PEDI_CD_PEDIDO             VARCHAR(12)    NOT NULL                COMMENT 'Codigo do pedido informado ao cliente (ex.: EI-7K2Q9M).',
  PEDI_NM_CLIENTE            VARCHAR(100)   NOT NULL                COMMENT 'Nome do cliente.',
  PEDI_NR_TELEFONE           VARCHAR(11)    NOT NULL                COMMENT 'Telefone do cliente com DDD, somente digitos.',
  PEDI_CD_TIPO_ENTREGA       VARCHAR(20)    NOT NULL                COMMENT 'Forma de receber o pedido: Retirada ou Entrega.',
  PEDI_DS_ENDERECO           VARCHAR(300)   NULL                    COMMENT 'Endereco de entrega (somente quando Entrega).',
  PEDI_DT_DESEJADA           DATE           NOT NULL                COMMENT 'Data em que o cliente deseja receber o pedido.',
  PEDI_TX_OBSERVACAO         VARCHAR(500)   NULL                    COMMENT 'Observacoes do cliente (sabor, decoracao, restricoes).',
  PEDI_NR_VALOR_TOTAL        DECIMAL(10,2)  NOT NULL                COMMENT 'Valor total do pedido, em reais, calculado pela API.',
  PEDI_CD_STATUS             VARCHAR(20)    NOT NULL                COMMENT 'Etapa: Recebido, Confirmado, EmPreparo, Pronto, Entregue, Cancelado.',
  PEDI_NM_CADASTRADO_POR     VARCHAR(30)    NOT NULL                COMMENT 'Usuario ou rotina que cadastrou o registro.',
  PEDI_IN_ATIVO              TINYINT        NOT NULL DEFAULT 1      COMMENT 'Situacao do registro: 0 inativo, 1 ativo, 2 arquivado.',
  PEDI_DT_CADASTRO           DATETIME(6)    NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'Data e hora do cadastro.',
  CONSTRAINT PK_PEDI PRIMARY KEY (PEDI_SQ_PEDIDO)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- -----------------------------------------------------
-- 2. DDL for Index
-- -----------------------------------------------------

CREATE UNIQUE INDEX UK_PEDI_CODIGO ON PEDIDO (PEDI_CD_PEDIDO);
CREATE INDEX IX_PEDI_DATA_STATUS ON PEDIDO (PEDI_DT_DESEJADA, PEDI_CD_STATUS);

-- -------------------------------------------------------
-- 3. DESCRICAO DOS CAMPOS (COMMENTS)
-- -------------------------------------------------------
ALTER TABLE PEDIDO COMMENT = 'Pedidos feitos pelo site da confeitaria.';

-- -----------------------------------------------------
-- 4. Constraints for Table PEDIDO
-- -----------------------------------------------------

ALTER TABLE PEDIDO ADD CONSTRAINT CK_PEDI_TIPO_ENTREGA CHECK (PEDI_CD_TIPO_ENTREGA IN ('Retirada', 'Entrega'));

-- -------------------------------------------------------
-- 5. SEQUENCES
-- -------------------------------------------------------
-- Nao se aplica ao MySQL (ver AUTO_INCREMENT na secao 1).
