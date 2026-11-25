import assert from 'node:assert/strict'
import test from 'node:test'
import { precoValido } from '../src/lib/numeros.ts'
import { tokenExpirado } from '../src/lib/jwt.ts'

test('preços aceitam centavos sem erros de arredondamento e rejeitam precisão excedida', () => {
  for (const preco of ['0.01', '1.15', '19.99', '100', '1.2300']) assert.equal(precoValido(preco), true)
  for (const preco of ['', '0', '-1', '1.234', 'Infinity', 'NaN', '9999999999999999.99']) {
    assert.equal(precoValido(preco), false)
  }
})

test('tokens inválidos, expirados ou sem expiração são descartados', () => {
  const token = carga => `header.${Buffer.from(JSON.stringify(carga)).toString('base64url')}.signature`
  assert.equal(tokenExpirado('invalido'), true)
  assert.equal(tokenExpirado(token({})), true)
  assert.equal(tokenExpirado(token({ exp: 1 })), true)
  assert.equal(tokenExpirado(token({ exp: 1e100 })), true)
  assert.equal(tokenExpirado(token({ exp: Date.now() / 1000 + 3600 })), false)
})
