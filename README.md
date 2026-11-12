# Entre Irmãs Confeitaria — site de vendas

Site de encomendas de doces: cardápio, sacola, checkout e confirmação do pedido pelo WhatsApp.
Segue o [CONFIGURACAO-INICIAL-PROJETOS.md](CONFIGURACAO-INICIAL-PROJETOS.md), com Oracle trocado por MySQL.

| Pasta | Conteúdo |
|---|---|
| [entreirmas.api](entreirmas.api) | .NET 10, EF Core 9 + Pomelo (MySQL), camadas `Api → Application → Domain ← Infrastructure` |
| [entreirmas.ui](entreirmas.ui) | Angular 21 (standalone, signals, zone.js), Angular Material, Jest, ESLint — prefixo `eirm` |
| [entreirmas.dados](entreirmas.dados) | Scripts Flyway (o schema é versionado aqui; não há migrations do EF) |
| [entreirmas.docs](entreirmas.docs) | Cópia idêntica dos scripts em `Recursos/sandbox/sql/project/` |

Cada pasta é um repositório git próprio (branch `develop`), ainda sem commits.

## Como funciona o pedido

1. A pessoa monta a sacola (guardada no `localStorage`) respeitando o pedido mínimo de cada doce.
2. No checkout informa nome, WhatsApp, retirada/entrega e a data (com `PEDIDO_ANTECEDENCIA_MINIMA_DIAS` de antecedência).
3. A API recalcula os preços pelo cadastro (o valor do navegador é ignorado), grava `PEDIDO` + `ITEM_PEDIDO`
   e devolve o código (`EI-XXXXXX`) com o link `wa.me` e a mensagem pronta.
4. A confeitaria confirma pagamento e entrega pelo WhatsApp.

`POST /api/pedidos` tem limite de `PEDIDO_LIMITE_POR_MINUTO` por IP.

## Rodando localmente

### 1. Banco (MySQL local)

Edite `entreirmas.dados/local/criar-banco-local.sql`, trocando `<SENHA_DO_APP>` por uma senha sua, e rode como root:

```bash
mysql -u root -p < entreirmas.dados/local/criar-banco-local.sql
```

Aplique os scripts (sem Flyway instalado, o script abaixo usa o cliente `mysql`; rode só em banco vazio):

```powershell
.\entreirmas.dados\local\aplicar-scripts-local.ps1 -Usuario root
```

### 2. API

```bash
cd entreirmas.api/EntreIrmas.Api
dotnet user-secrets set MYSQL_SENHA "<SENHA_DO_APP>"
dotnet user-secrets set JWT_CHAVE "<uma chave aleatória com 32+ caracteres>"
dotnet run --launch-profile Local
```

Swagger: <http://localhost:5083/api/swagger> · Health: <http://localhost:5083/api/health>

### 3. Front

```bash
cd entreirmas.ui
npm install
npm start
```

Site: <http://localhost:4200>

## Testes e build

```bash
dotnet test entreirmas.api/EntreIrmas.Test/EntreIrmas.Test.csproj
```

```bash
cd entreirmas.ui && npx jest && npm run lint && npx ng build --configuration production
```

## Configuração da API

| Chave | Muda por ambiente / segredo? | Observação |
|---|---|---|
| `MYSQL_SERVIDOR`, `MYSQL_PORTA`, `MYSQL_BANCO`, `MYSQL_USUARIO` | muda por ambiente | |
| `MYSQL_SENHA` | **segredo** | user-secrets no local; ConfigMap/secret nos ambientes |
| `JWT_CHAVE` | **segredo** | mínimo de 32 caracteres; a API não sobe sem ela |
| `CORS_ORIGENS` | muda por ambiente | origens separadas por vírgula |
| `USE_SWAGGER` | muda por ambiente | |
| `MYSQL_VERSAO`, `MYSQL_TIMEOUT_SEGUNDOS`, `HEALTHCHECK_TIMEOUT_SEGUNDOS`, `JWT_EMISSOR`, `JWT_AUDIENCIA` | comum | só no `appsettings.json` |
| `WHATSAPP_NUMERO`, `PEDIDO_ANTECEDENCIA_MINIMA_DIAS`, `PEDIDO_LIMITE_POR_MINUTO` | comum | só no `appsettings.json` |

No front, `whatsappNumero` e `antecedenciaMinimaDias` estão em todos os `src/environments/*` e precisam
acompanhar os valores da API.

## Adaptações em relação ao guia

- **MySQL no lugar de Oracle.** O MySQL não tem `SEQUENCE`: as chaves usam `AUTO_INCREMENT` e a API
  usa `ValueGeneratedOnAdd()` no lugar de `UseHiLo`. Os scripts mantêm as 5 seções; a seção 5 fica como nota.
  Comentários de coluna vão na própria definição (sintaxe do MySQL).
- **EF Core 9 com .NET 10.** O Pomelo (MIT) ainda não tem versão para o EF Core 10, e o provedor da Oracle
  para MySQL é GPL. Atualizar para EF Core 10 quando sair o Pomelo 10.
- **MySqlConnector fixado em 2.6.2**: a 2.4.0 trazida pelo Pomelo tem vulnerabilidade conhecida (GHSA-473q-m89c-ghf8).
- **FluentAssertions 7.2** (Apache 2.0): a 8.x tem licença comercial.
- **FluentValidation 11.11**: o `FluentValidation.AspNetCore` 11.3 não aceita a 12.
- **Sem PrimeNG**: as telas são próprias; o Material entra nos campos do checkout e nas notificações.
- **Autenticação**: o JWT e o `AuthorizeFilter` global estão configurados, mas todas as rotas atuais são
  públicas (`[AllowAnonymous]`). `AuthService`/`AuthGuard` no front entram junto com o painel administrativo.
- **Ainda não criado**: painel administrativo (cardápio e pedidos), Cypress, manifestos k8s e pipelines do GitLab.

## Conteúdo a revisar com a confeitaria

- Produtos, descrições e **preços da carga inicial** (`V1.0.5__Insert_CardapioInicial.sql`) são exemplos.
- Os produtos usam ilustrações próprias em SVG; para usar fotos, preencha `PROD_DS_URL_IMAGEM`.
- Textos da página inicial ("Nossa cozinha") e o número do WhatsApp (tirado da bio do Instagram).
