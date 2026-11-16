-- -------------------------------------------------------
-- 1. DDL - ENDERECO DE ENTREGA EM PARTES
-- -------------------------------------------------------
-- O checkout passa a pedir CEP, rua, numero, complemento, bairro, cidade e UF separados (o CEP preenche
-- o resto pelo ViaCEP). PEDI_DS_ENDERECO continua com o endereco em uma linha, montado pela API,
-- para a mensagem do WhatsApp e o painel; cresce para caber todas as partes.
-- Todas as colunas novas sao nulas: na retirada nao ha endereco, e os pedidos antigos so tem a linha.
ALTER TABLE PEDIDO
  MODIFY COLUMN PEDI_DS_ENDERECO VARCHAR(500) NULL
    COMMENT 'Endereco de entrega em uma linha (montado pela API a partir das partes abaixo).',
  ADD COLUMN PEDI_NR_CEP          VARCHAR(8)   NULL COMMENT 'CEP da entrega, somente digitos.' AFTER PEDI_DS_ENDERECO,
  ADD COLUMN PEDI_DS_LOGRADOURO   VARCHAR(150) NULL COMMENT 'Rua, avenida ou praca da entrega.' AFTER PEDI_NR_CEP,
  ADD COLUMN PEDI_DS_NUMERO       VARCHAR(20)  NULL COMMENT 'Numero da entrega (ou s/n).' AFTER PEDI_DS_LOGRADOURO,
  ADD COLUMN PEDI_DS_COMPLEMENTO  VARCHAR(100) NULL COMMENT 'Apartamento, bloco, referencia.' AFTER PEDI_DS_NUMERO,
  ADD COLUMN PEDI_NM_BAIRRO       VARCHAR(100) NULL COMMENT 'Bairro da entrega.' AFTER PEDI_DS_COMPLEMENTO,
  ADD COLUMN PEDI_NM_CIDADE       VARCHAR(100) NULL COMMENT 'Cidade da entrega.' AFTER PEDI_NM_BAIRRO,
  ADD COLUMN PEDI_SG_UF           CHAR(2)      NULL COMMENT 'UF da entrega.' AFTER PEDI_NM_CIDADE;

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

-- Entrega gravada pelo site tem o endereco completo; retirada nao tem CEP.
ALTER TABLE PEDIDO ADD CONSTRAINT CK_PEDI_CEP CHECK (PEDI_NR_CEP IS NULL OR PEDI_NR_CEP REGEXP '^[0-9]{8}$');

-- -------------------------------------------------------
-- 5. SEQUENCES
-- -------------------------------------------------------
-- Nao se aplica ao MySQL.
