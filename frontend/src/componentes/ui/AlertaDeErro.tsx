import { AlertTriangle } from 'lucide-react'
import { useId, useSyncExternalStore } from 'react'

import { alertasDeErro } from '@/lib/alertasDeErro'

import { Botao } from './Botao'
import { Modal } from './Modal'

/** Fica na raiz do app para atender também login, sessão e erros dentro de outros modais. */
export function AlertaDeErro() {
  const alerta = useSyncExternalStore(alertasDeErro.assinar, alertasDeErro.obterAtual)
  const idMensagem = useId()

  return (
    <Modal
      aberto={alerta !== null}
      aoFechar={alertasDeErro.fecharAtual}
      titulo={alerta?.titulo ?? 'Operação não concluída'}
      papel="alertdialog"
      descritoPor={idMensagem}
      largura="estreita"
      rodape={<Botao variante="primario" onClick={alertasDeErro.fecharAtual} autoFocus>Entendi</Botao>}
    >
      <div className="flex items-start gap-3">
        <div className="grid size-10 shrink-0 place-items-center rounded-xl border border-negative/25 bg-negative-soft text-negative-ink">
          <AlertTriangle aria-hidden className="size-5" />
        </div>
        <p id={idMensagem} className="min-w-0 pt-1 text-sm whitespace-pre-line text-ink-soft wrap-break-word">
          {alerta?.mensagem}
        </p>
      </div>
      {alerta?.traceId && (
        <details className="mt-4 text-xs text-ink-muted">
          <summary className="cursor-pointer">Detalhes do erro</summary>
          <p className="mt-2 break-all">Código de referência: {alerta.traceId}</p>
        </details>
      )}
    </Modal>
  )
}
