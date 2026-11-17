# Entre Irmãs Confeitaria — site de vendas

Site de encomendas de doces: cardápio, sacola, checkout e confirmação do pedido pelo WhatsApp.
Segue o [CONFIGURACAO-INICIAL-PROJETOS.md](CONFIGURACAO-INICIAL-PROJETOS.md), com Oracle trocado por MySQL.

| Pasta | Conteúdo |
|---|---|
| [entreirmas.api](entreirmas.api) | .NET 10, EF Core 9 + Pomelo (MySQL), camadas `Api → Application → Domain ← Infrastructure`; integrações externas em `Gateways/` |
| [entreirmas.ui](entreirmas.ui) | Angular 21 (standalone, signals, zone.js), Angular Material, Jest, ESLint — prefixo `eirm` |
| [entreirmas.dados](entreirmas.dados) | Scripts Flyway (o schema é versionado aqui; não há migrations do EF) |

As três pastas ficam neste mesmo repositório (branch `develop`).

## Como funciona o pedido

Há dois fluxos, decididos pelo cadastro dos produtos (a API confere, o navegador não decide):

| | Pedido normal | Encomenda |
|---|---|---|
| Quando | Só doces de pronta entrega na sacola | Algum doce marcado "sob encomenda" (pode juntar pronta entrega) |
| Data | Para hoje: o checkout não pede data e a API ignora a enviada | Escolhida pelo cliente, com `PEDIDO_ANTECEDENCIA_MINIMA_DIAS` de antecedência |
| Loja fechada | Bloqueado (aviso no checkout e 400 na API) | Aceita |
| Depois | Paga pelo WhatsApp e retira ou recebe | Paga pelo WhatsApp e retira ou recebe na data |

1. A pessoa monta a sacola (guardada no `localStorage`) respeitando o pedido mínimo de cada doce.
2. No checkout informa nome, WhatsApp e retirada/entrega (e a data, se for encomenda). Na entrega, o endereço vai
   em partes (CEP, rua, número, complemento, bairro, cidade e UF): ao completar o CEP, a tela busca o endereço no
   ViaCEP (pela API) e preenche o resto; se o CEP não existir ou o serviço falhar, avisa ao lado do campo.
3. A API recalcula os preços pelo cadastro (o valor do navegador é ignorado), grava `PEDIDO` + `ITEM_PEDIDO`
   e devolve o código (`EI-XXXXXX`) com o link `wa.me` e a mensagem pronta.
4. A confeitaria confirma pagamento e entrega pelo WhatsApp.
   Na entrega, o cliente clica em **Calcular entrega** (ver abaixo) e a taxa entra no total.
5. O cliente acompanha a etapa (recebido → confirmado → em preparo → pronto → entregue, ou cancelado) em
   **Meu pedido** (`/acompanhar-pedido`), digitando o código ou pelo botão da tela de pedido recebido. Os pedidos
   feitos ou consultados no aparelho ficam no `localStorage` para um clique. A etapa muda quando a confeitaria
   altera o status na tela Pedidos.

### Entrega pela Uber ou 99

No checkout, na entrega, o botão **Calcular entrega** manda o endereço para `POST /api/entregas/cotacao` (público,
limite de `ENTREGA_LIMITE_POR_MINUTO` por IP). A API cota na Uber e na 99 ao mesmo tempo e devolve só a **mais
barata** (parceiro, valor e minutos até a entrega); a tela mostra o valor, soma no total e desenha o trajeto da loja
até o endereço no mapa do Google (sem chave de API). Mudar o endereço apaga o cálculo; o pedido de entrega só é
enviado depois de calcular.

- No envio, a API **cota de novo** (o valor do navegador não vale) e grava a taxa e o parceiro no pedido
  (`V1.0.13`). `PEDI_NR_VALOR_TOTAL` continua sendo só os itens (painel e lucro não mudam); o cliente paga
  itens + taxa (`totalAPagar`), que aparecem na confirmação, no Meu pedido, no admin, no recibo e no WhatsApp.
- Sem parceiro configurado, endereço fora da área ou parceiro fora do ar: o checkout avisa e a taxa é combinada
  pelo WhatsApp, como antes. Pedidos lançados no admin não cotam.
- **Ao marcar o pedido como Pronto** no admin, a API chama o entregador do parceiro escolhido e guarda o id e o
  link de rastreio (o cliente vê "Acompanhar o entregador" no Meu pedido). Se a chamada falhar, a etapa muda mesmo
  assim, o motivo aparece no detalhe do pedido e o botão **Chamar entregador de novo** tenta outra vez
  (`POST /api/admin/pedidos/{id}/entregador`).

Situação de cada parceiro:

| Parceiro | Integração | Para ligar |
|---|---|---|
| Uber (Uber Direct) | Pronta: token OAuth, cotação (`delivery_quotes`) e entrega (`deliveries`) | Criar a conta no Uber Direct, pegar no painel Developer o Client ID, o Client Secret e o Customer ID e configurar `UBER_CLIENT_ID`, `UBER_CLIENT_SECRET` e `UBER_CUSTOMER_ID` (segredos). Comece com `UBER_MODO_TESTE = true` (entregador-robô) |
| 99 (99Entrega) | Só no simulador | A documentação da API da 99Entrega não é pública: é liberada pelo time comercial junto com a conta empresarial. Com ela em mãos, entra um gateway como o da Uber (`IEntregaParceiroService`) |

**Simulador** (`ENTREGA_SIMULADOR = true`, ligado no ambiente `Local` e nos testes): Uber e 99 de mentira, sem
chamar ninguém. O valor sai da diferença entre o CEP da loja e o do cliente (a Uber fica mais barata perto da loja
e a 99 mais longe) e fora da UF da loja ninguém atende. **Nunca ligue em produção**: os clientes veriam taxas
inventadas.

O endereço de coleta (a loja) fica em `LOJA_*` no `appsettings.json`.

### Loja aberta ou fechada

- Tela **Loja** no admin (`/admin/loja`): botão Abrir/Fechar e um aviso opcional (ex.: "Voltamos amanhã às 9h").
  Fica na tabela `LOJA` (registro único, criado pelo `V1.0.12`).
- Fechada, o site mostra uma faixa no topo e o checkout bloqueia o pedido normal; encomendas continuam.
  O site consulta `GET /api/loja` ao abrir e quando a aba volta ao foco; a API confere de novo no envio.
- Pedido lançado no admin (`Lançar pedido`) não segue essas regras.

### Aviso de pedido novo (admin)

Com a área administrativa aberta (qualquer tela), ela marca o ponto de partida em
`GET /api/admin/pedidos/ultimo-do-site` e, a cada 20 s, busca `GET /api/admin/pedidos/novos-do-site?depoisDe={id}`
(todos os pedidos do site depois daquele id, com os itens, até 20). Quando chega pedido, toca um som (gerado pelo
navegador, sem arquivo), mostra a notificação com o botão Ver, a lista de Pedidos se atualiza e o recibo é impresso.
O som liga/desliga no menu lateral (a escolha fica no aparelho).
O navegador só libera áudio depois de um clique na página: depois de entrar, clique em qualquer lugar.

### Recibo na impressora térmica (admin)

Cada pedido novo do site sai impresso numa impressora térmica de recibos de 58mm por bluetooth, sem janela de
impressão. Funciona no **Chrome ou Edge no computador** (Web Serial); Firefox, Safari e celulares não têm esse acesso.

1. Pareie a impressora no Bluetooth do Windows. Ela vira uma porta COM ("Porta serial padrão por link Bluetooth").
2. No admin, em **Loja → Impressora de recibos**, clique em **Conectar impressora**, escolha a porta e use
   **Imprimir teste**. O navegador lembra a escolha: nas próximas vezes a área administrativa reconecta sozinha.
3. A impressão automática liga/desliga na mesma tela (a escolha fica no aparelho). No detalhe de cada pedido há o
   botão **Imprimir recibo** para reimprimir.

O recibo sai em ESC/POS, 32 colunas e sem acentos (cada impressora usa uma tabela de caracteres; sem acento sai igual
em todas). Ele traz o código, retirada ou entrega, a data, o cliente, o telefone, o endereço, os itens, o total e a
observação. Impressoras só BLE (sem o perfil serial SPP) não aparecem na lista. Se a impressão falhar, o admin
avisa; a fila tenta reabrir a conexão uma vez antes de desistir.

A impressora só funciona onde `impressoraHabilitada` é `true` no `src/environments/*`: hoje só no
`environment.prod.ts` (produção). No `ng serve`, no `prod-local`, no `dsv` e no `tst` ela fica desligada, para
os testes não imprimirem: a tela da Loja avisa, o botão **Imprimir recibo** some e nada conecta. Para testar a
impressora no computador, troque para `true` no `environment.ts` (sem commitar).

`POST /api/pedidos` tem limite de `PEDIDO_LIMITE_POR_MINUTO` por IP. `GET /api/pedidos/{codigo}` (público, aceita
o código com ou sem `EI-`) devolve só etapa, primeiro nome, data, forma, itens e total, sem telefone, endereço ou
observação, com limite de `ACOMPANHAMENTO_LIMITE_POR_MINUTO` por IP para dificultar a tentativa de códigos.

### Busca de CEP (ViaCEP)

- `GET /api/enderecos/cep/{cep}` (público, limite de `CEP_LIMITE_POR_MINUTO` por IP): 200 com o endereço,
  400 para CEP sem 8 números, 404 para CEP inexistente e 503 quando o ViaCEP não responde.
- O acesso ao ViaCEP fica no projeto `Gateways/EntreIrmas.ViaCepService` (HttpClient nomeado com timeout,
  cache em memória de `VIACEP_CACHE_HORAS` para CEPs encontrados e health check `viacep` como *Degraded*:
  sem o ViaCEP a API continua de pé e o endereço é digitado à mão).
- O navegador não chama o ViaCEP direto: passa pela API, que normaliza o retorno e aplica o limite.

## Área da confeitaria (administrativa)

Não há link no site: o acesso é pela URL **`/admin/entrar`**. Depois de entrar, o cabeçalho do site
mostra o link "Painel" e a área tem menu próprio:

| Tela | O que faz |
|---|---|
| Painel | Receita, custo dos produtos, despesas, perdas, lucro e margem do período; gráfico por dia/mês, mais vendidos e pedidos por etapa |
| Pedidos | Lista com filtros, troca de etapa, WhatsApp do cliente e lançamento de pedido feito fora do site |
| Produtos | Cadastro com preço, **custo**, margem, **foto** (upload) e se é **sob encomenda**; tirar do cardápio sem excluir |
| Categorias | Cadastro e ordem no cardápio; só exclui categoria sem produtos |
| Despesas e perdas | Lançamentos que entram no cálculo do lucro |
| Usuários | Criar, redefinir senha, bloquear e excluir (ninguém tira o próprio acesso) |

No computador, o botão no topo do menu (**Recolher menu** / **Expandir menu**) deixa só a coluna dos ícones,
com o nome de cada item ao parar o mouse; a escolha fica no aparelho. No celular o menu é uma gaveta aberta pelo
botão da barra de cima.

**Como o lucro é calculado:** vendas são os pedidos confirmados e não cancelados, pela data desejada.
Lucro = vendas − custo dos produtos vendidos − despesas − perdas. O custo é copiado para o item no momento
do pedido (como o preço), então mudar o custo depois não altera o lucro dos pedidos antigos.

**Cardápio:** pronta entrega primeiro e, por último, o bloco "Sob encomenda" (com o aviso da antecedência).

**Fotos dos produtos:** ficam no MySQL (`IMAGEM_PRODUTO`, separada de `PRODUTO` para a listagem não carregar os
bytes). O navegador reduz a foto para até 1200px em WEBP (ou JPG) antes de enviar; a API aceita JPG, PNG ou WEBP
de até 2 MB, conferindo os bytes do arquivo, e entrega em `GET /api/produtos/{id}/imagem` com cache de um ano
(a URL tem a versão `?v=`, que muda a cada envio).

**Segurança:** login com JWT (validade `JWT_EXPIRACAO_HORAS`), senhas em PBKDF2-SHA256 com 600 mil iterações,
limite de `LOGIN_LIMITE_POR_MINUTO` tentativas por IP e a mesma mensagem para usuário inexistente ou senha errada.

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

Banco que já tem as versões anteriores: aplique só as novas com `-APartirDe` (ex.: `-APartirDe 1.0.6`).
O `V1.0.9` cria a tabela das fotos dos produtos; o `V1.0.10` marca os produtos sob encomenda (os já cadastrados
começam sem: marque na tela Produtos); o `V1.0.11` separa o endereço de entrega em colunas (os pedidos antigos
mantêm só o texto em `PEDI_DS_ENDERECO`); o `V1.0.12` cria a tabela `LOJA` (já aberta); o `V1.0.13` acrescenta
ao pedido a taxa de entrega, o parceiro (Uber/99) e os dados da chamada do entregador.

Usuários iniciais (suellen e tatiane): o script `entreirmas.dados/local/criar-usuarios-iniciais.sql`
**não vai para o git** (está no `.gitignore`, porque o repositório é público) e traz a senha padrão no
cabeçalho. Rode à mão depois dos scripts acima e troque as senhas na tela Usuários:

```powershell
Get-Content .\entreirmas.dados\local\criar-usuarios-iniciais.sql -Raw | mysql -u root -p entreirmas
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
| `JWT_EXPIRACAO_HORAS`, `LOGIN_LIMITE_POR_MINUTO` | comum | só no `appsettings.json` (12 h e 5 tentativas) |
| `WHATSAPP_NUMERO`, `PEDIDO_ANTECEDENCIA_MINIMA_DIAS`, `PEDIDO_LIMITE_POR_MINUTO`, `ACOMPANHAMENTO_LIMITE_POR_MINUTO` | comum | só no `appsettings.json` (acompanhamento: 30 consultas) |
| `VIACEP_URL`, `VIACEP_TIMEOUT_SEGUNDOS`, `VIACEP_CACHE_HORAS`, `CEP_LIMITE_POR_MINUTO` | comum | só no `appsettings.json` (5 s, 24 h e 30 consultas) |
| `ENTREGA_SIMULADOR` | muda por ambiente | `true` só no `Local` e nos testes; `false` nos demais |
| `ENTREGA_TIMEOUT_SEGUNDOS`, `ENTREGA_LIMITE_POR_MINUTO`, `LOJA_*` | comum | só no `appsettings.json` (10 s, 20 cálculos; endereço e coordenadas da loja) |
| `UBER_CLIENT_ID`, `UBER_CLIENT_SECRET`, `UBER_CUSTOMER_ID` | **segredo** | user-secrets no local; ConfigMap/secret nos ambientes. Vazios, a Uber fica desligada |
| `UBER_MODO_TESTE` | muda por ambiente | `true` usa o entregador-robô do Uber Direct (nenhum entregador sai) |
| `UBER_AUTH_URL`, `UBER_API_URL` | comum | só no `appsettings.json` |

No front, `whatsappNumero` e `antecedenciaMinimaDias` estão em todos os `src/environments/*` e precisam
acompanhar os valores da API.

## Adaptações em relação ao guia

- **MySQL no lugar de Oracle.** O MySQL não tem `SEQUENCE`: as chaves usam `AUTO_INCREMENT` e a API
  usa `ValueGeneratedOnAdd()` no lugar de `UseHiLo`. Os scripts mantêm as 5 seções; a seção 5 fica como nota.
  Comentários de coluna vão na própria definição (sintaxe do MySQL). As linhas de separação começam
  com `-- ` (com espaço): no MySQL, `--` sem espaço depois não é comentário.
- **Sem o projeto `docs`.** Os scripts ficam só em `entreirmas.dados`, sem a cópia em `Recursos/sandbox`.
- **EF Core 9 com .NET 10.** O Pomelo (MIT) ainda não tem versão para o EF Core 10, e o provedor da Oracle
  para MySQL é GPL. Atualizar para EF Core 10 quando sair o Pomelo 10.
- **MySqlConnector fixado em 2.6.2**: a 2.4.0 trazida pelo Pomelo tem vulnerabilidade conhecida (GHSA-473q-m89c-ghf8).
- **FluentAssertions 7.2** (Apache 2.0): a 8.x tem licença comercial.
- **FluentValidation 11.11**: o `FluentValidation.AspNetCore` 11.3 não aceita a 12.
- **Sem PrimeNG**: as telas são próprias; o Material entra nos campos, seleções, diálogos e notificações.
- **Autenticação**: `AuthorizeFilter` global; as rotas do site usam `[AllowAnonymous]` e as de `api/admin/*`
  exigem o token. Há um único perfil (quem entra pode tudo); `AuthService` e guards no front.
- **Tabelas da área administrativa** viram cartões pela largura do cartão (container query), não da janela:
  com o menu lateral, 1024px deixa pouco espaço para as colunas.
- **Ainda não criado**: Cypress, manifestos k8s e pipelines do GitLab.

## Conteúdo a revisar com a confeitaria

- Produtos, descrições e **preços da carga inicial** (`V1.0.5__Insert_CardapioInicial.sql`) são exemplos.
- Os produtos usam ilustrações próprias em SVG até receberem uma foto (tela Produtos → Foto).
- Textos da página inicial ("Nossa cozinha") e o número do WhatsApp (tirado da bio do Instagram).
