import assert from 'node:assert/strict'
import test from 'node:test'

import { obterCategoria, rotuloDaCategoria } from '../src/pessoas/categorias.ts'

test('reconhece todas as categorias por nome e pelo enum numérico, incluindo zero', () => {
  for (const [numero, nome] of ['Cliente', 'Fornecedor', 'Funcionario', 'Parceiro'].entries()) {
    assert.equal(obterCategoria(numero), nome)
    assert.equal(obterCategoria(nome), nome)
  }
})

test('categoria ausente ou inválida não é classificada como cliente', () => {
  for (const valor of [undefined, null, '', 'Outra', -1, 4, 0.5, NaN]) {
    assert.equal(obterCategoria(valor), null)
    assert.equal(rotuloDaCategoria(valor), 'Sem categoria')
  }
})

test('exibe Funcionário com acento sem alterar o nome do enum na API', () => {
  assert.equal(rotuloDaCategoria('Funcionario'), 'Funcionário')
  assert.equal(rotuloDaCategoria(2), 'Funcionário')
})
