import type { HTMLAttributes, ReactNode } from 'react'

import { cn } from '@/lib/utils'

export function Cartao({ className, ...resto }: HTMLAttributes<HTMLDivElement>) {
  return (
    <div
      className={cn(
        'rounded-(--radius-card) border border-line bg-surface shadow-e1',
        className,
      )}
      {...resto}
    />
  )
}

interface PropriedadesCabecalho {
  titulo: ReactNode
  descricao?: ReactNode
  acao?: ReactNode
  className?: string
}

export function CabecalhoDoCartao({
  titulo,
  descricao,
  acao,
  className,
}: PropriedadesCabecalho) {
  return (
    <div
      className={cn(
        'flex flex-wrap items-start justify-between gap-3 border-b border-line px-5 py-4',
        className,
      )}
    >
      <div className="min-w-0">
        <h2 className="text-[0.9375rem] font-semibold tracking-tight text-ink">{titulo}</h2>
        {descricao && <p className="mt-0.5 text-[0.8125rem] text-ink-muted">{descricao}</p>}
      </div>
      {/* max-w-full em vez de shrink-0: um filtro largo quebra para a linha
          de baixo e rola ali dentro, em vez de estourar o cartão. */}
      {acao && <div className="flex max-w-full min-w-0 items-center gap-2">{acao}</div>}
    </div>
  )
}

export function CorpoDoCartao({ className, ...resto }: HTMLAttributes<HTMLDivElement>) {
  return <div className={cn('p-5', className)} {...resto} />
}
