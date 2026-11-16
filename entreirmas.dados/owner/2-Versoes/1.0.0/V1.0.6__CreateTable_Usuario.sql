-- -------------------------------------------------------
-- 1. DDL - CRIACAO DA TABELA USUARIO
-- -------------------------------------------------------
-- MySQL nao tem SEQUENCE: a chave usa AUTO_INCREMENT e a API mapeia com ValueGeneratedOnAdd().
-- Os usuarios iniciais NAO sao criados aqui (o repositorio e publico): use o script
-- local/criar-usuarios-iniciais.sql, que fica fora do git.
CREATE TABLE USUARIO (
  USUA_SQ_USUARIO            BIGINT         NOT NULL AUTO_INCREMENT COMMENT 'Chave primaria sequencial da tabela.',
  USUA_NM_LOGIN              VARCHAR(30)    NOT NULL                COMMENT 'Login de acesso a area administrativa (minusculas).',
  USUA_NM_USUARIO            VARCHAR(100)   NOT NULL                COMMENT 'Nome exibido na area administrativa.',
  USUA_DS_HASH_SENHA         VARCHAR(200)   NOT NULL                COMMENT 'Hash PBKDF2-SHA256 da senha (formato PBKDF2$iteracoes$sal$hash). Nunca a senha.',
  USUA_DT_ULTIMO_ACESSO      DATETIME(6)    NULL                    COMMENT 'Data e hora do ultimo login com sucesso.',
  USUA_NM_CADASTRADO_POR     VARCHAR(30)    NOT NULL                COMMENT 'Usuario ou rotina que cadastrou o registro.',
  USUA_IN_ATIVO              TINYINT        NOT NULL DEFAULT 1      COMMENT 'Situacao do registro: 0 inativo, 1 ativo, 2 arquivado.',
  USUA_DT_CADASTRO           DATETIME(6)    NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'Data e hora do cadastro.',
  CONSTRAINT PK_USUA PRIMARY KEY (USUA_SQ_USUARIO)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- -------------------------------------------------------
-- 2. DDL for Index
-- -------------------------------------------------------

CREATE UNIQUE INDEX UK_USUA_LOGIN ON USUARIO (USUA_NM_LOGIN);

-- -------------------------------------------------------
-- 3. DESCRICAO DOS CAMPOS (COMMENTS)
-- -------------------------------------------------------
ALTER TABLE USUARIO COMMENT = 'Usuarios com acesso a area administrativa do site.';

-- -------------------------------------------------------
-- 4. Constraints for Table USUARIO
-- -------------------------------------------------------
-- A chave primaria e criada junto com a tabela: no MySQL a coluna AUTO_INCREMENT precisa ser chave.

-- -------------------------------------------------------
-- 5. SEQUENCES
-- -------------------------------------------------------
-- Nao se aplica ao MySQL (ver AUTO_INCREMENT na secao 1).
