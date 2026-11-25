import type { Papel, StatusPedido, TipoMovimentacao } from '../api/tipos'
import type { TomEtiqueta } from '../componentes/ui/Etiqueta'

export const tonsDeStatus: Record<StatusPedido, TomEtiqueta> = {
  Aberto: 'marca',
  Finalizado: 'positivo',
  Cancelado: 'neutro',
}

/** Pedidos sem tipo não podem ser classificados como venda por suposição. */
export function filtrarPedidosPorTipo<T extends { tipoMovimentacao?: TipoMovimentacao }>(
  pedidos: readonly T[],
  tipo: TipoMovimentacao,
) {
  return pedidos.filter((pedido) => pedido.tipoMovimentacao === tipo)
}

export function podeOperarPedido(tipo: TipoMovimentacao, papel?: Papel) {
  return papel === 'Admin' || (papel === 'Vendedor' && tipo === 'Saida')
}
