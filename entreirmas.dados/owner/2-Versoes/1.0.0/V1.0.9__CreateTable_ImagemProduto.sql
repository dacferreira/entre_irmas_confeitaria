-- -------------------------------------------------------
-- 1. DDL - CRIACAO DA TABELA IMAGEM_PRODUTO
-- -------------------------------------------------------
-- MySQL nao tem SEQUENCE: a chave usa AUTO_INCREMENT e a API mapeia com ValueGeneratedOnAdd().
-- Foto enviada pela area administrativa (uma por produto). Fica separada de PRODUTO para a listagem
-- do cardapio nao carregar os bytes. O navegador reduz a foto antes do envio (ate 1200px); a API aceita
-- ate 2 MB, so JPG, PNG ou WEBP.
-- O prefixo BL (binario) nao existe no padrao de nomes: e o tipo desta coluna, que guarda o arquivo.
CREATE TABLE IMAGEM_PRODUTO (
  IMPR_SQ_IMAGEM_PRODUTO     BIGINT         NOT NULL AUTO_INCREMENT COMMENT 'Chave primaria sequencial da tabela.',
  PROD_SQ_PRODUTO            BIGINT         NOT NULL                COMMENT 'Produto da foto (FK para PRODUTO). Uma foto por produto.',
  IMPR_DS_TIPO_CONTEUDO      VARCHAR(30)    NOT NULL                COMMENT 'Tipo do arquivo: image/jpeg, image/png ou image/webp.',
  IMPR_BL_CONTEUDO           MEDIUMBLOB     NOT NULL                COMMENT 'Bytes da foto (ate 2 MB).',
  IMPR_NM_CADASTRADO_POR     VARCHAR(30)    NOT NULL                COMMENT 'Usuario ou rotina que cadastrou o registro.',
  IMPR_IN_ATIVO              TINYINT        NOT NULL DEFAULT 1      COMMENT 'Situacao do registro: 0 inativo, 1 ativo, 2 arquivado.',
  IMPR_DT_CADASTRO           DATETIME(6)    NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'Data e hora do cadastro.',
  CONSTRAINT PK_IMPR PRIMARY KEY (IMPR_SQ_IMAGEM_PRODUTO)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- -------------------------------------------------------
-- 2. DDL for Index
-- -------------------------------------------------------

CREATE UNIQUE INDEX UK_IMPR_PRODUTO ON IMAGEM_PRODUTO (PROD_SQ_PRODUTO);

-- -------------------------------------------------------
-- 3. DESCRICAO DOS CAMPOS (COMMENTS)
-- -------------------------------------------------------
ALTER TABLE IMAGEM_PRODUTO COMMENT = 'Fotos dos produtos enviadas pela area administrativa.';

-- A coluna de PRODUTO passa a guardar o caminho da foto na API, com versao para o cache.
ALTER TABLE PRODUTO MODIFY COLUMN PROD_DS_URL_IMAGEM VARCHAR(500) NULL
  COMMENT 'Caminho da foto na API (produtos/{id}/imagem?v={versao}). Nulo: o site usa a ilustracao.';

-- -------------------------------------------------------
-- 4. Constraints for Table IMAGEM_PRODUTO
-- -------------------------------------------------------

ALTER TABLE IMAGEM_PRODUTO ADD CONSTRAINT FK_IMPR_PROD FOREIGN KEY (PROD_SQ_PRODUTO) REFERENCES PRODUTO (PROD_SQ_PRODUTO);
ALTER TABLE IMAGEM_PRODUTO ADD CONSTRAINT CK_IMPR_TIPO_CONTEUDO
  CHECK (IMPR_DS_TIPO_CONTEUDO IN ('image/jpeg', 'image/png', 'image/webp'));

-- -------------------------------------------------------
-- 5. SEQUENCES
-- -------------------------------------------------------
-- Nao se aplica ao MySQL (ver AUTO_INCREMENT na secao 1).
