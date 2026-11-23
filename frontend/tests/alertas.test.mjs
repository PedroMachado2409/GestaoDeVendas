import assert from 'node:assert/strict'
import test from 'node:test'

import { alertasDeErro, criarCentralDeAlertas } from '../src/lib/alertasDeErro.ts'
import { ErroApi } from '../src/lib/erros.ts'
import { notificarErro } from '../src/lib/notificacoes.ts'

test('exibe falhas diferentes em sequência sem perder o primeiro alerta', () => {
  const central = criarCentralDeAlertas()
  central.adicionar({ titulo: 'Cadastro', mensagem: 'CPF já cadastrado.' })
  central.adicionar({ titulo: 'Produtos', mensagem: 'Sem permissão.' })
  assert.equal(central.obterAtual().mensagem, 'CPF já cadastrado.')
  central.fecharAtual()
  assert.equal(central.obterAtual().mensagem, 'Sem permissão.')
  central.fecharAtual()
  assert.equal(central.obterAtual(), null)
})

test('falhas simultâneas iguais geram um único alerta e podem ser exibidas novamente após fechar', () => {
  const central = criarCentralDeAlertas()
  const alerta = { titulo: 'Erro', mensagem: 'Servidor indisponível.' }
  central.adicionar(alerta)
  central.adicionar(alerta)
  central.fecharAtual()
  assert.equal(central.obterAtual(), null)
  central.adicionar(alerta)
  assert.equal(central.obterAtual().mensagem, alerta.mensagem)
})

test('notifica assinantes e permite desinscrição', () => {
  const central = criarCentralDeAlertas()
  let chamadas = 0
  const remover = central.assinar(() => chamadas++)
  central.adicionar({ titulo: 'Erro', mensagem: 'Falha.' })
  central.fecharAtual()
  remover()
  central.adicionar({ titulo: 'Erro', mensagem: 'Outra falha.' })
  assert.equal(chamadas, 2)
})

test('alerta inclui os erros de validação e o código de referência da API', () => {
  notificarErro(new ErroApi(400, {
    errors: { Email: ['E-mail já cadastrado.'], Cpf: ['CPF inválido.'] }, traceId: 'teste-123',
  }), 'Não foi possível salvar')
  const alerta = alertasDeErro.obterAtual()
  assert.equal(alerta.titulo, 'Não foi possível salvar')
  assert.equal(alerta.mensagem, 'E-mail já cadastrado. CPF inválido.')
  assert.equal(alerta.traceId, 'teste-123')
  alertasDeErro.fecharAtual()
})

test('sessão expirada gera um único alerta mesmo quando observada por mais de um fluxo', () => {
  notificarErro(new ErroApi(401), 'Não foi possível carregar')
  notificarErro(new ErroApi(401), 'Sessão encerrada')
  alertasDeErro.fecharAtual()
  assert.equal(alertasDeErro.obterAtual(), null)
})

test('cancelamento de requisição não abre alerta', () => {
  notificarErro(new DOMException('Cancelada', 'AbortError'))
  assert.equal(alertasDeErro.obterAtual(), null)
})

test('falha de rede usa mensagem compreensível sem configurações internas', () => {
  notificarErro(new ErroApi(0))
  assert.equal(alertasDeErro.obterAtual().titulo, 'Sem comunicação com a API')
  assert.match(alertasDeErro.obterAtual().mensagem, /Verifique sua conexão/)
  alertasDeErro.fecharAtual()
})
