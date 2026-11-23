import {
  History,
  Landmark,
  LayoutDashboard,
  Package,
  ShoppingCart,
  ShoppingBag,
  UserCog,
  Users,
  type LucideIcon,
} from 'lucide-react'

export interface ItemDeNavegacao {
  para: string
  rotulo: string
  icone: LucideIcon
  /** Somente Admin enxerga o item; a rota também é protegida. */
  somenteAdmin?: boolean
  fim?: boolean
}

export interface GrupoDeNavegacao {
  rotulo: string
  itens: ItemDeNavegacao[]
}

export const gruposDeNavegacao: GrupoDeNavegacao[] = [
  {
    rotulo: 'Geral',
    itens: [
      { para: '/', rotulo: 'Painel', icone: LayoutDashboard, fim: true },
      { para: '/pessoas', rotulo: 'Pessoas', icone: Users },
      { para: '/usuarios', rotulo: 'Usuários', icone: UserCog, somenteAdmin: true },
    ],
  },
  {
    rotulo: 'Financeiro',
    itens: [{ para: '/financeiro', rotulo: 'Financeiro', icone: Landmark, somenteAdmin: true }],
  },
  {
    rotulo: 'Estoque',
    itens: [
      { para: '/compras', rotulo: 'Pedidos de compra', icone: ShoppingBag, somenteAdmin: true },
      { para: '/produtos', rotulo: 'Produtos', icone: Package },
      { para: '/estoque', rotulo: 'Estoque', icone: History, somenteAdmin: true },
    ],
  },
  {
    rotulo: 'Faturamento',
    itens: [{ para: '/pedidos', rotulo: 'Pedidos de venda', icone: ShoppingCart }],
  },
]
