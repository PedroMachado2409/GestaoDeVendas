import type { LucideIcon } from 'lucide-react'
import type { ReactNode } from 'react'

import { cn } from '@/lib/utils'

import { Esqueleto } from './Estados'

interface Propriedades {
  rotulo: string
  valor: string
  Icone: LucideIcon
  /** Linha de apoio: composição do número, não elogio ao número. */
  apoio?: ReactNode
  carregando?: boolean
  destaque?: boolean
}

/**
 * Um número é um número: sem tabular-nums no valor grande (deixa os dígitos
 * frouxos em tamanho de destaque) e sem cor que sinalize desempenho.
 */
export function Indicador({ rotulo, valor, Icone, apoio, carregando, destaque }: Propriedades) {
  return (
    <div
      className={cn(
        'min-w-0 rounded-(--radius-card) border border-line bg-surface p-3.5 shadow-e1 sm:p-4',
        destaque && 'ring-1 ring-brand/20',
      )}
    >
      <div className="flex items-center justify-between gap-3">
        <p className="min-w-0 truncate text-[0.8125rem] font-medium text-ink-muted">{rotulo}</p>
        <span
          aria-hidden
          className={cn(
            'grid size-7 shrink-0 place-items-center rounded-lg',
            destaque ? 'bg-brand-soft text-brand-ink' : 'bg-surface-2 text-ink-muted',
          )}
        >
          <Icone className="size-3.5" />
        </span>
      </div>

      {carregando ? (
        <Esqueleto className="mt-3 h-8 w-24" />
      ) : (
        // Quebra em vez de truncar: um valor cortado com reticências é um valor errado.
        <p className="mt-2 text-[1.375rem] [overflow-wrap:anywhere] leading-tight font-semibold tracking-tight text-ink sm:text-[1.75rem] sm:leading-none">
          {valor}
        </p>
      )}

      {apoio && <p className="mt-2 text-xs text-pretty text-ink-muted">{apoio}</p>}
    </div>
  )
}
