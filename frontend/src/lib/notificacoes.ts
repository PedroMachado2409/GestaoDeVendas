import { toast } from 'sonner'

import { ErroApi, STATUS_SEM_RESPOSTA } from './erros'

/** Toast padronizado de falha, com o traceId quando a API o devolve. */
export function notificarErro(erro: unknown, titulo = 'Operação não concluída') {
  if (erro instanceof ErroApi) {
    const rodape =
      erro.traceId && erro.status !== STATUS_SEM_RESPOSTA ? ` (traceId ${erro.traceId})` : ''
    toast.error(erro.status === STATUS_SEM_RESPOSTA ? erro.titulo : titulo, {
      description: erro.mensagem + rodape,
    })
    return
  }

  toast.error(titulo, {
    description: erro instanceof Error ? erro.message : 'Erro inesperado.',
  })
}
