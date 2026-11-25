import assert from 'node:assert/strict'
import test from 'node:test'
import { listarTodasPaginas, paginarItens } from '../src/lib/paginacao.ts'

const registros = Array.from({ length: 205 }, (_, indice) => ({ id: indice + 1 }))

test('carrega todas as páginas e preserva registros além da primeira página', async () => {
  const chamadas = []
  const lista = await listarTodasPaginas(async (pagina, tamanho) => {
    chamadas.push([pagina, tamanho])
    return registros.slice((pagina - 1) * tamanho, pagina * tamanho)
  })
  assert.deepEqual(lista, registros)
  assert.deepEqual(chamadas, [[1, 100], [2, 100], [3, 100]])
})

test('encerra uma página completa seguida de lista vazia', async () => {
  const chamadas = []
  const lista = await listarTodasPaginas(async (pagina) => {
    chamadas.push(pagina)
    return pagina === 1 ? registros.slice(0, 100) : []
  })
  assert.equal(lista.length, 100)
  assert.deepEqual(chamadas, [1, 2])
})

test('API antiga que ignora paginação não gera laço nem registros duplicados', async () => {
  let chamadas = 0
  const lista = await listarTodasPaginas(async () => {
    chamadas++
    return registros.slice(0, 100)
  })
  assert.equal(chamadas, 2)
  assert.equal(lista.length, 100)
})

test('API antiga com lista maior que o tamanho pedido mantém todos os registros', async () => {
  let chamadas = 0
  const lista = await listarTodasPaginas(async () => { chamadas++; return registros })
  assert.equal(chamadas, 1)
  assert.equal(lista.length, 205)
})

test('falha em página posterior não devolve lista incompleta como sucesso', async () => {
  await assert.rejects(listarTodasPaginas(async (pagina) => {
    if (pagina === 2) throw new Error('Falha de rede')
    return registros.slice(0, 100)
  }), /Falha de rede/)
})

test('cancelamento interrompe a leitura antes de pedir outra página', async () => {
  const controle = new AbortController()
  let chamadas = 0
  await assert.rejects(listarTodasPaginas(async () => {
    chamadas++
    controle.abort()
    return registros.slice(0, 100)
  }, controle.signal), { name: 'AbortError' })
  assert.equal(chamadas, 1)
})

test('formato inesperado é reportado em vez de parecer uma lista vazia', async () => {
  await assert.rejects(listarTodasPaginas(async () => ({ itens: registros })), /lista válida/)
})

test('apresentação da segunda página não altera a lista original', () => {
  const pagina = paginarItens(registros, 2)
  assert.deepEqual(pagina.itens.map(item => item.id), Array.from({ length: 20 }, (_, i) => 21 + i))
  assert.equal(pagina.inicio, 21)
  assert.equal(pagina.fim, 40)
  assert.equal(registros.length, 205)
})

test('página fora do intervalo é limitada e lista vazia permanece vazia', () => {
  assert.equal(paginarItens(registros, 999).pagina, 11)
  assert.equal(paginarItens(registros, 999).itens.length, 5)
  assert.deepEqual(paginarItens([], 5).itens, [])
  assert.equal(paginarItens([], 5).inicio, 0)
})
