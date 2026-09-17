import { AlertTriangle, Loader2, RefreshCw } from 'lucide-react'
import type { ReactNode } from 'react'

import { ErroApi } from '@/lib/erros'
import { cn } from '@/lib/utils'

import { Botao } from './Botao'

export function Esqueleto({ className }: { className?: string }) {
  return (
    <div
      aria-hidden
      className={cn('animar-cintilar rounded-md bg-surface-3', className)}
    />
  )
}

export function EsqueletoDeTabela({ linhas = 6, colunas = 5 }: { linhas?: number; colunas?: number }) {
  return (
    <div className="p-4" role="status" aria-label="Carregando dados">
      <div className="grid gap-3">
        {Array.from({ length: linhas }).map((_, linha) => (
          <div
            key={linha}
            className="grid items-center gap-4"
            style={{ gridTemplateColumns: `repeat(${colunas}, minmax(0, 1fr))` }}
          >
            {Array.from({ length: colunas }).map((__, coluna) => (
              <Esqueleto
                key={coluna}
                className={cn('h-4', coluna === 0 ? 'w-4/5' : 'w-3/5')}
              />
            ))}
          </div>
        ))}
      </div>
    </div>
  )
}

interface PropriedadesVazio {
  icone?: ReactNode
  titulo: string
  descricao?: string
  acao?: ReactNode
}

export function EstadoVazio({ icone, titulo, descricao, acao }: PropriedadesVazio) {
  return (
    <div className="flex flex-col items-center justify-center gap-3 px-6 py-14 text-center">
      {icone && (
        <div className="grid size-11 place-items-center rounded-xl border border-line bg-surface-2 text-ink-muted">
          {icone}
        </div>
      )}
      <div className="grid gap-1">
        <p className="text-[0.9375rem] font-medium text-ink">{titulo}</p>
        {descricao && <p className="max-w-sm text-sm text-ink-muted">{descricao}</p>}
      </div>
      {acao}
    </div>
  )
}

export function EstadoDeErro({
  erro,
  aoTentarNovamente,
}: {
  erro: unknown
  aoTentarNovamente?: () => void
}) {
  const titulo = erro instanceof ErroApi ? erro.titulo : 'Falha ao carregar'
  const detalhe =
    erro instanceof ErroApi
      ? erro.mensagem
      : erro instanceof Error
        ? erro.message
        : 'Erro inesperado.'
  const traceId = erro instanceof ErroApi ? erro.traceId : undefined

  return (
    <div className="flex flex-col items-center justify-center gap-3 px-6 py-14 text-center">
      <div className="grid size-11 place-items-center rounded-xl border border-negative/25 bg-negative-soft text-negative-ink">
        <AlertTriangle aria-hidden className="size-5" />
      </div>
      <div className="grid gap-1">
        <p className="text-[0.9375rem] font-medium text-ink">{titulo}</p>
        <p className="max-w-md text-sm text-ink-muted">{detalhe}</p>
        {traceId && (
          <p className="font-mono text-[0.6875rem] text-ink-muted/80">traceId: {traceId}</p>
        )}
      </div>
      {aoTentarNovamente && (
        <Botao
          variante="contorno"
          tamanho="pequeno"
          onClick={aoTentarNovamente}
          iconeInicial={<RefreshCw aria-hidden className="size-3.5" />}
        >
          Tentar novamente
        </Botao>
      )}
    </div>
  )
}

export function Carregando({ rotulo = 'Carregando' }: { rotulo?: string }) {
  return (
    <div role="status" className="flex items-center justify-center gap-2 py-14 text-ink-muted">
      <Loader2 aria-hidden className="size-4 animate-spin" />
      <span className="text-sm">{rotulo}…</span>
    </div>
  )
}
