import { Routes } from '@angular/router';
import { ROTAS_ADMIN } from '@constants/admin';
import { ROTAS } from '@constants/rotas';
import { autenticadoGuard, visitanteGuard } from '@guards/autenticado.guard';
import { sacolaPreenchidaGuard } from '@guards/sacola-preenchida.guard';

const NOME_DO_SITE = 'Entre Irmãs Confeitaria';
const AREA_ADMIN = 'Área da confeitaria';

export const routes: Routes = [
  // Área administrativa: sem link no site, acessada pela URL (/admin/entrar).
  {
    path: ROTAS_ADMIN.BASE,
    children: [
      {
        path: ROTAS_ADMIN.LOGIN,
        title: `Entrar · ${AREA_ADMIN}`,
        canActivate: [visitanteGuard],
        loadComponent: () => import('@admin/login/login.component').then((m) => m.LoginComponent),
      },
      {
        path: '',
        canActivate: [autenticadoGuard],
        canActivateChild: [autenticadoGuard],
        loadComponent: () => import('@admin/layout/admin-layout.component').then((m) => m.AdminLayoutComponent),
        children: [
          { path: '', pathMatch: 'full', redirectTo: ROTAS_ADMIN.PAINEL },
          {
            path: ROTAS_ADMIN.PAINEL,
            title: `Painel · ${AREA_ADMIN}`,
            loadComponent: () => import('@admin/painel/painel.component').then((m) => m.PainelComponent),
          },
          {
            path: ROTAS_ADMIN.PEDIDOS,
            title: `Pedidos · ${AREA_ADMIN}`,
            loadComponent: () => import('@admin/pedidos/pedidos.component').then((m) => m.PedidosComponent),
          },
          {
            path: ROTAS_ADMIN.PRODUTOS,
            title: `Produtos · ${AREA_ADMIN}`,
            loadComponent: () => import('@admin/produtos/produtos.component').then((m) => m.ProdutosComponent),
          },
          {
            path: ROTAS_ADMIN.CATEGORIAS,
            title: `Categorias · ${AREA_ADMIN}`,
            loadComponent: () => import('@admin/categorias/categorias.component').then((m) => m.CategoriasComponent),
          },
          {
            path: ROTAS_ADMIN.LANCAMENTOS,
            title: `Despesas e perdas · ${AREA_ADMIN}`,
            loadComponent: () =>
              import('@admin/lancamentos/lancamentos.component').then((m) => m.LancamentosComponent),
          },
          {
            path: ROTAS_ADMIN.LOJA,
            title: `Loja · ${AREA_ADMIN}`,
            loadComponent: () => import('@admin/loja/loja.component').then((m) => m.LojaComponent),
          },
          {
            path: ROTAS_ADMIN.USUARIOS,
            title: `Usuários · ${AREA_ADMIN}`,
            loadComponent: () => import('@admin/usuarios/usuarios.component').then((m) => m.UsuariosComponent),
          },
        ],
      },
    ],
  },

  {
    path: ROTAS.INICIO,
    pathMatch: 'full',
    title: `${NOME_DO_SITE} · Doces feitos à mão`,
    data: { title: 'Início' },
    loadComponent: () => import('@inicio/inicio.component').then((m) => m.InicioComponent),
  },
  {
    path: ROTAS.CARDAPIO,
    title: `Cardápio · ${NOME_DO_SITE}`,
    data: { title: 'Cardápio' },
    loadComponent: () => import('@cardapio/cardapio.component').then((m) => m.CardapioComponent),
  },
  {
    path: `${ROTAS.PRODUTO}/:slug`,
    title: `Doce · ${NOME_DO_SITE}`,
    data: { title: 'Produto' },
    loadComponent: () => import('@produto/produto.component').then((m) => m.ProdutoComponent),
  },
  {
    path: ROTAS.FINALIZAR_PEDIDO,
    title: `Finalizar pedido · ${NOME_DO_SITE}`,
    data: { title: 'Finalizar pedido' },
    canActivate: [sacolaPreenchidaGuard],
    loadComponent: () =>
      import('@finalizar-pedido/finalizar-pedido.component').then((m) => m.FinalizarPedidoComponent),
  },
  {
    path: ROTAS.PEDIDO_CONFIRMADO,
    title: `Pedido recebido · ${NOME_DO_SITE}`,
    data: { title: 'Pedido recebido' },
    loadComponent: () =>
      import('@pedido-confirmado/pedido-confirmado.component').then((m) => m.PedidoConfirmadoComponent),
  },
  {
    path: ROTAS.ACOMPANHAR_PEDIDO,
    title: `Acompanhar pedido · ${NOME_DO_SITE}`,
    data: { title: 'Acompanhar pedido' },
    loadComponent: () =>
      import('@acompanhar-pedido/acompanhar-pedido.component').then((m) => m.AcompanharPedidoComponent),
  },
  {
    // Link direto, aberto pela tela de pedido recebido ou pela lista de pedidos do aparelho.
    path: `${ROTAS.ACOMPANHAR_PEDIDO}/:codigo`,
    title: `Acompanhar pedido · ${NOME_DO_SITE}`,
    data: { title: 'Acompanhar pedido' },
    loadComponent: () =>
      import('@acompanhar-pedido/acompanhar-pedido.component').then((m) => m.AcompanharPedidoComponent),
  },
  {
    path: '**',
    title: `Página não encontrada · ${NOME_DO_SITE}`,
    data: { title: 'Não encontrado' },
    loadComponent: () =>
      import('@nao-encontrado/nao-encontrado.component').then((m) => m.NaoEncontradoComponent),
  },
];
