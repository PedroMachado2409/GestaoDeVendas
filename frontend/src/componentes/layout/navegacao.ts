import {
  History,
  Landmark,
  LayoutDashboard,
  Package,
  ShoppingCart,
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

export const itensDeNavegacao: ItemDeNavegacao[] = [
  { para: '/', rotulo: 'Painel', icone: LayoutDashboard, fim: true },
  { para: '/pedidos', rotulo: 'Pedidos', icone: ShoppingCart },
  { para: '/clientes', rotulo: 'Clientes', icone: Users },
  { para: '/produtos', rotulo: 'Produtos', icone: Package },
  { para: '/estoque', rotulo: 'Estoque', icone: History, somenteAdmin: true },
  { para: '/financeiro', rotulo: 'Financeiro', icone: Landmark, somenteAdmin: true },
  { para: '/usuarios', rotulo: 'Usuários', icone: UserCog, somenteAdmin: true },
]
