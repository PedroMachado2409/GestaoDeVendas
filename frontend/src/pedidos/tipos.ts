import type { Papel, TipoMovimentacao } from '../api/tipos'

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
