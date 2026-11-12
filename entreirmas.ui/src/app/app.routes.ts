import { Routes } from '@angular/router';
import { ROTAS } from '@constants/rotas';
import { sacolaPreenchidaGuard } from '@guards/sacola-preenchida.guard';

const NOME_DO_SITE = 'Entre Irmãs Confeitaria';

export const routes: Routes = [
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
    path: '**',
    title: `Página não encontrada · ${NOME_DO_SITE}`,
    data: { title: 'Não encontrado' },
    loadComponent: () =>
      import('@nao-encontrado/nao-encontrado.component').then((m) => m.NaoEncontradoComponent),
  },
];
