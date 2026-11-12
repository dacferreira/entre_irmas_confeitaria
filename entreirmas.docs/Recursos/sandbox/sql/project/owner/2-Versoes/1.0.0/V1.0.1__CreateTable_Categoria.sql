----------------------------------------------------------
-- 1. DDL - CRIACAO DA TABELA CATEGORIA
----------------------------------------------------------
-- MySQL nao tem SEQUENCE: a chave usa AUTO_INCREMENT e a API mapeia com ValueGeneratedOnAdd().
-- Os comentarios das colunas ficam na propria definicao (sintaxe do MySQL).
CREATE TABLE CATEGORIA (
  CATE_SQ_CATEGORIA          BIGINT         NOT NULL AUTO_INCREMENT COMMENT 'Chave primaria sequencial da tabela.',
  CATE_NM_NOME               VARCHAR(100)   NOT NULL                COMMENT 'Nome da categoria exibido no cardapio.',
  CATE_DS_SLUG               VARCHAR(100)   NOT NULL                COMMENT 'Identificador da categoria na URL (minusculas, sem acentos).',
  CATE_DS_DESCRICAO          VARCHAR(300)   NULL                    COMMENT 'Descricao curta da categoria.',
  CATE_NR_ORDEM              INT            NOT NULL DEFAULT 0      COMMENT 'Ordem de exibicao no cardapio.',
  CATE_NM_CADASTRADO_POR     VARCHAR(30)    NOT NULL                COMMENT 'Usuario ou rotina que cadastrou o registro.',
  CATE_IN_ATIVO              TINYINT        NOT NULL DEFAULT 1      COMMENT 'Situacao do registro: 0 inativo, 1 ativo, 2 arquivado.',
  CATE_DT_CADASTRO           DATETIME(6)    NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'Data e hora do cadastro.',
  CONSTRAINT PK_CATE PRIMARY KEY (CATE_SQ_CATEGORIA)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--------------------------------------------------------
-- 2. DDL for Index
--------------------------------------------------------

CREATE UNIQUE INDEX UK_CATE_SLUG ON CATEGORIA (CATE_DS_SLUG);

----------------------------------------------------------
-- 3. DESCRICAO DOS CAMPOS (COMMENTS)
----------------------------------------------------------
ALTER TABLE CATEGORIA COMMENT = 'Categorias do cardapio da confeitaria (bolos, docinhos, salgados...).';

--------------------------------------------------------
-- 4. Constraints for Table CATEGORIA
--------------------------------------------------------
-- A chave primaria e criada junto com a tabela: no MySQL a coluna AUTO_INCREMENT precisa ser chave.

----------------------------------------------------------
-- 5. SEQUENCES
----------------------------------------------------------
-- Nao se aplica ao MySQL (ver AUTO_INCREMENT na secao 1).
