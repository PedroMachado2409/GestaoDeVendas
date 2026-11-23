import assert from 'node:assert/strict'
import test from 'node:test'

import { cnpjValido, formatarCpfCnpj, normalizarCpfCnpj } from '../src/lib/documentos.ts'

for (const documento of ['11222333000181', '11.222.333/0001-81', '12ABC34501DE35', '12.abc.345/01de-35']) {
  test(`aceita CNPJ válido: ${documento}`, () => {
    assert.equal(cnpjValido(documento), true)
  })
}

for (const documento of ['', '00000000000000', '11111111111111', '11222333000182', '12ABC34501DE34', '12ABC34501DE3A', '１２ABC34501DE35', '12ABC34501DE35!']) {
  test(`recusa CNPJ inválido: ${documento || '(vazio)'}`, () => {
    assert.equal(cnpjValido(documento), false)
  })
}

test('normaliza máscara e caixa sem apagar letras ou caracteres inválidos', () => {
  assert.equal(normalizarCpfCnpj(' 12.abc.345/01de-35 '), '12ABC34501DE35')
  assert.equal(normalizarCpfCnpj('529.982.247-25'), '52998224725')
  assert.equal(normalizarCpfCnpj('12ABC34501DE35!'), '12ABC34501DE35!')
})

for (const [documento, formatado] of [
  ['52998224725', '529.982.247-25'],
  ['11222333000181', '11.222.333/0001-81'],
  ['12abc34501de35', '12.ABC.345/01DE-35'],
]) {
  test(`formata sem perder informação: ${documento}`, () => {
    assert.equal(formatarCpfCnpj(documento), formatado)
    assert.equal(formatarCpfCnpj(formatado), formatado)
    assert.equal(normalizarCpfCnpj(formatado), normalizarCpfCnpj(documento))
  })
}

test('mantém documentos incompletos para validação sem truncar o CNPJ', () => {
  assert.equal(formatarCpfCnpj('112223330001'), '112223330001')
  assert.equal(formatarCpfCnpj('12ABC34501DE35!'), '12ABC34501DE35!')
})
