-- -------------------------------------------------------
-- CARGA INICIAL DO CARDAPIO
-- -------------------------------------------------------
-- ATENCAO: nomes, descricoes e PRECOS sao EXEMPLOS para o site funcionar.
-- Revise com a confeitaria antes de publicar (novo script V1.0.x com UPDATE).
-- Ilustracoes disponiveis no site: bolo, bolo-pote, brownie, fudge, brigadeiro, cupcake, torta, salgado.

INSERT INTO CATEGORIA (CATE_NM_NOME, CATE_DS_SLUG, CATE_DS_DESCRICAO, CATE_NR_ORDEM, CATE_NM_CADASTRADO_POR) VALUES
  ('Bolos',              'bolos',      'Bolos recheados e caseiros, feitos sob encomenda.', 1, 'CARGA_INICIAL'),
  ('Brownies & Fudges',  'brownies',   'Chocolate em todas as texturas: casquinha, cremoso e molhadinho.', 2, 'CARGA_INICIAL'),
  ('Docinhos',           'docinhos',   'Docinhos de festa enrolados a mao, um por um.', 3, 'CARGA_INICIAL'),
  ('Sobremesas',         'sobremesas', 'Bolos de pote e tortas para adocar a semana.', 4, 'CARGA_INICIAL'),
  ('Salgados',           'salgados',   'Salgados fresquinhos para festas e reunioes.', 5, 'CARGA_INICIAL');

INSERT INTO PRODUTO (CATE_SQ_CATEGORIA, PROD_NM_NOME, PROD_DS_SLUG, PROD_TX_DESCRICAO, PROD_NR_PRECO, PROD_DS_UNIDADE,
                     PROD_NR_QUANTIDADE_MINIMA, PROD_CD_ILUSTRACAO, PROD_IN_DESTAQUE, PROD_NM_CADASTRADO_POR)
SELECT c.CATE_SQ_CATEGORIA, p.NOME, p.SLUG, p.DESCRICAO, p.PRECO, p.UNIDADE, p.MINIMO, p.ILUSTRACAO, p.DESTAQUE, 'CARGA_INICIAL'
  FROM (
    SELECT 'bolos' AS CATEGORIA, 'Bolo de Chocolate com Morango' AS NOME, 'bolo-de-chocolate-com-morango' AS SLUG,
           'Massa fofinha de chocolate, recheio de brigadeiro cremoso e morangos frescos.' AS DESCRICAO,
           95.00 AS PRECO, 'unidade (1,5 kg)' AS UNIDADE, 1 AS MINIMO, 'bolo' AS ILUSTRACAO, 1 AS DESTAQUE
    UNION ALL SELECT 'bolos', 'Bolo Ninho com Morango', 'bolo-ninho-com-morango',
           'Massa branca, creme de leite Ninho e camadas de morango.', 105.00, 'unidade (1,5 kg)', 1, 'bolo', 0
    UNION ALL SELECT 'bolos', 'Bolo de Cenoura com Brigadeiro', 'bolo-de-cenoura-com-brigadeiro',
           'O classico da casa da vo, com cobertura generosa de brigadeiro.', 45.00, 'unidade (1 kg)', 1, 'bolo', 0
    UNION ALL SELECT 'bolos', 'Fatia de Bolo Recheado', 'fatia-de-bolo-recheado',
           'Uma fatia generosa do bolo do dia. Pergunte os sabores disponiveis.', 14.00, 'fatia', 1, 'bolo', 0
    UNION ALL SELECT 'brownies', 'Brownie Tradicional', 'brownie-tradicional',
           'Casquinha crocante por fora e molhadinho por dentro.', 9.00, 'unidade', 1, 'brownie', 1
    UNION ALL SELECT 'brownies', 'Brownie de Nutella', 'brownie-de-nutella',
           'Nosso brownie recheado com uma camada farta de Nutella.', 12.00, 'unidade', 1, 'brownie', 0
    UNION ALL SELECT 'brownies', 'Caixa de Brownies', 'caixa-de-brownies',
           'Quatro brownies sortidos em caixa para presente.', 42.00, 'caixa com 4', 1, 'brownie', 0
    UNION ALL SELECT 'brownies', 'Fudge de Chocolate', 'fudge-de-chocolate',
           'Chocolate denso e cremoso para comer de colher.', 18.00, 'pote 200 g', 1, 'fudge', 1
    UNION ALL SELECT 'docinhos', 'Brigadeiro Gourmet', 'brigadeiro-gourmet',
           'Chocolate nobre e granulado belga. Pedido minimo de 25 unidades.', 2.50, 'unidade', 25, 'brigadeiro', 1
    UNION ALL SELECT 'docinhos', 'Beijinho', 'beijinho',
           'Coco fresco e leite condensado, finalizado com cravo opcional.', 2.50, 'unidade', 25, 'brigadeiro', 0
    UNION ALL SELECT 'docinhos', 'Bicho de Pe', 'bicho-de-pe',
           'O docinho rosa de morango que toda festa precisa.', 2.50, 'unidade', 25, 'brigadeiro', 0
    UNION ALL SELECT 'docinhos', 'Cupcake Decorado', 'cupcake-decorado',
           'Massa macia com chantininho decorado no tema da sua festa.', 8.00, 'unidade', 6, 'cupcake', 1
    UNION ALL SELECT 'sobremesas', 'Bolo de Pote de Ninho com Nutella', 'bolo-de-pote-de-ninho-com-nutella',
           'Camadas de bolo, creme de Ninho e Nutella no pote.', 15.00, 'pote 250 ml', 1, 'bolo-pote', 1
    UNION ALL SELECT 'sobremesas', 'Torta de Limao', 'torta-de-limao',
           'Massa amanteigada, creme de limao e merengue macaricado.', 60.00, 'unidade (20 cm)', 1, 'torta', 0
    UNION ALL SELECT 'salgados', 'Mini Salgados Sortidos', 'mini-salgados-sortidos',
           'Coxinha, bolinha de queijo, risole e kibe. Fritos na hora ou congelados.', 95.00, 'cento', 1, 'salgado', 0
    UNION ALL SELECT 'salgados', 'Empadinha de Frango', 'empadinha-de-frango',
           'Massa que derrete na boca e recheio cremoso de frango.', 3.50, 'unidade', 20, 'salgado', 0
  ) p
  JOIN CATEGORIA c ON c.CATE_DS_SLUG = p.CATEGORIA;
