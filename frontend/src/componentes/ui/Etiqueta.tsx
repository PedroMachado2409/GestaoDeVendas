import type { ReactNode } from 'react'

import { cn } from '@/lib/utils'

export type TomEtiqueta = 'neutro' | 'positivo' | 'atencao' | 'negativo' | 'marca'

const tons: Record<TomEtiqueta, string> = {
  neutro: 'bg-surface-2 text-ink-soft border-line',
  positivo: 'bg-positive-soft text-positive-ink border-positive/25',
  atencao: 'bg-caution-soft text-caution-ink border-caution/30',
  negativo: 'bg-negative-soft text-negative-ink border-negative/25',
  marca: 'bg-brand-soft text-brand-ink border-brand/25',
}

interface Propriedades {
  tom?: TomEtiqueta
  children: ReactNode
  /** Ponto colorido à esquerda, para status em tabelas. */
  comPonto?: boolean
  className?: string
}

export function Etiqueta({ tom = 'neutro', comPonto = false, className, children }: Propriedades) {
  return (
    <span
      className={cn(
        'inline-flex items-center gap-1.5 rounded-full border px-2 py-0.5 text-xs font-medium whitespace-nowrap',
        tons[tom],
        className,
      )}
    >
      {comPonto && <span aria-hidden className="size-1.5 rounded-full bg-current opacity-70" />}
      {children}
    </span>
  )
}
