import assert from 'node:assert/strict'
import test from 'node:test'

import { filtrarPedidosPorTipo, podeOperarPedido } from '../src/pedidos/tipos.ts'

const pedidos = [
  { id: 1, tipoMovimentacao: 'Saida' },
  { id: 2, tipoMovimentacao: 'Entrada' },
  { id: 3 },
  { id: 4, tipoMovimentacao: 'Saida' },
  { id: 5, tipoMovimentacao: null },
  { id: 6, tipoMovimentacao: 'Invalido' },
]

test('vendas excluem compras e pedidos sem classificação', () => {
  assert.deepEqual(filtrarPedidosPorTipo(pedidos, 'Saida').map(p => p.id), [1, 4])
})

test('compras excluem vendas e pedidos sem classificação', () => {
  assert.deepEqual(filtrarPedidosPorTipo(pedidos, 'Entrada').map(p => p.id), [2])
})

test('classificação não altera a lista compartilhada entre as abas', () => {
  const copia = structuredClone(pedidos)
  filtrarPedidosPorTipo(pedidos, 'Entrada')
  assert.deepEqual(pedidos, copia)
})

for (const [tipo, papel, permitido] of [
  ['Saida', 'Admin', true],
  ['Entrada', 'Admin', true],
  ['Saida', 'Vendedor', true],
  ['Entrada', 'Vendedor', false],
  ['Entrada', undefined, false],
  ['Saida', undefined, false],
]) {
  test(`permissão para ${papel ?? 'sem sessão'} em ${tipo}`, () => {
    assert.equal(podeOperarPedido(tipo, papel), permitido)
  })
}
