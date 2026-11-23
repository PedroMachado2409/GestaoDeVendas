import { alertasDeErro } from './alertasDeErro.ts'
import { ErroApi, STATUS_SEM_RESPOSTA } from './erros.ts'

/** Alerta global de falha, com a mensagem e o código de referência da API. */
export function notificarErro(erro: unknown, titulo = 'Operação não concluída') {
  if (erro instanceof Error && erro.name === 'AbortError') return

  if (erro instanceof ErroApi) {
    alertasDeErro.adicionar({
      titulo: erro.status === STATUS_SEM_RESPOSTA ? erro.titulo : titulo,
      mensagem: erro.mensagem,
      traceId: erro.traceId,
      // O mesmo 401 também pode ser observado pelo encerramento da sessão.
      chave: erro.status === 401 ? 'sessao:401' : `${erro.status}:${erro.mensagem}:${erro.traceId ?? ''}`,
    })
    return
  }

  alertasDeErro.adicionar({
    titulo,
    mensagem: erro instanceof Error ? erro.message : 'Erro inesperado.',
  })
}
