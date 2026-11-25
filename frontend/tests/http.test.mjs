import assert from 'node:assert/strict'
import test from 'node:test'
import { definirToken, definirTratadorDeSessaoExpirada, http } from '../src/lib/http.ts'
import { pessoasApi, pedidosApi } from '../src/api/recursos.ts'

function preparar(t, buscar) {
  const original = globalThis.fetch
  globalThis.fetch = buscar
  definirToken(null)
  definirTratadorDeSessaoExpirada(null)
  t.after(() => {
    globalThis.fetch = original
    definirToken(null)
    definirTratadorDeSessaoExpirada(null)
  })
}

test('criação de pedido envia pessoaId e mantém o tipo da movimentação', async t => {
  let corpo
  preparar(t, async (_url, opcoes) => {
    corpo = JSON.parse(opcoes.body)
    return Response.json({ id: 1 })
  })
  await pedidosApi.criar({ pessoaId: 7, tipoMovimentacao: 'Saida', itens: [{ produtoId: 3, quantidade: 2 }] })
  assert.equal(corpo.pessoaId, 7)
  assert.equal(corpo.tipoMovimentacao, 'Saida')
  assert.equal(Object.hasOwn(corpo, 'clienteId'), false)
})

test('listagem de pessoas envia os parâmetros acordados com a API', async t => {
  const chamadas = []
  preparar(t, async url => {
    const endereco = new URL(url)
    chamadas.push(endereco.search)
    const pagina = Number(endereco.searchParams.get('pagina'))
    return Response.json(pagina === 1 ? Array.from({ length: 100 }, (_, i) => ({ id: i + 1 })) : [{ id: 101 }])
  })
  const pessoas = await pessoasApi.listar()
  assert.equal(pessoas.length, 101)
  assert.deepEqual(chamadas, ['?pagina=1&tamanhoPagina=100', '?pagina=2&tamanhoPagina=100'])
})

test('401 da sessão atual encerra a sessão e preserva o erro da API', async t => {
  preparar(t, async () => Response.json({ detail: 'Token revogado' }, { status: 401 }))
  let encerramentos = 0
  definirToken('atual')
  definirTratadorDeSessaoExpirada(() => { encerramentos++ })
  await assert.rejects(http.get('/api/Pessoa'), erro => erro.status === 401 && erro.message === 'Token revogado')
  assert.equal(encerramentos, 1)
})

for (const status of [200, 401]) {
  test(`resposta ${status} da sessão anterior não encerra nem atualiza a sessão nova`, async t => {
    let responder
    preparar(t, () => new Promise(resolve => { responder = resolve }))
    let encerramentos = 0
    definirTratadorDeSessaoExpirada(() => { encerramentos++ })
    definirToken('anterior')
    const requisicao = http.get('/api/Pessoa')
    definirToken('nova')
    responder(Response.json([], { status }))
    await assert.rejects(requisicao, { name: 'AbortError' })
    assert.equal(encerramentos, 0)
  })
}

test('falha de login anônimo não derruba uma sessão existente', async t => {
  preparar(t, async () => Response.json({}, { status: 401 }))
  let encerramentos = 0
  definirToken('atual')
  definirTratadorDeSessaoExpirada(() => { encerramentos++ })
  await assert.rejects(http.post('/api/Usuario/autenticar', {}, { anonimo: true }))
  assert.equal(encerramentos, 0)
})
