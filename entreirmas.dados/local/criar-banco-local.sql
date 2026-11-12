-- Cria o banco e o usuario da aplicacao no MySQL LOCAL.
-- Rode como root, trocando <SENHA_DO_APP> por uma senha sua (nao versione a senha):
--   mysql -u root -p < local/criar-banco-local.sql
-- Depois grave a mesma senha nos user-secrets da API:
--   dotnet user-secrets set MYSQL_SENHA "<SENHA_DO_APP>" --project ../entreirmas.api/EntreIrmas.Api

CREATE DATABASE IF NOT EXISTS entreirmas CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;

CREATE USER IF NOT EXISTS 'entreirmas_app'@'localhost' IDENTIFIED BY '<SENHA_DO_APP>';

-- A aplicacao so le e grava dados; o schema e mantido pelos scripts Flyway.
GRANT SELECT, INSERT, UPDATE, DELETE ON entreirmas.* TO 'entreirmas_app'@'localhost';
FLUSH PRIVILEGES;
