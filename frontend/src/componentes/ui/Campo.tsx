import { AlertCircle } from 'lucide-react'
import { forwardRef, useId, type InputHTMLAttributes, type ReactNode } from 'react'

import { cn } from '@/lib/utils'

const controle =
  'w-full rounded-lg border bg-surface text-ink placeholder:text-ink-muted/80 transition-[border-color,box-shadow] outline-none disabled:cursor-not-allowed disabled:bg-surface-2 disabled:text-ink-muted'

const estadoNormal = 'border-line hover:border-line-strong focus:border-brand'
const estadoInvalido = 'border-negative focus:border-negative'

interface PropriedadesCampo {
  rotulo: string
  erro?: string
  dica?: string
  obrigatorio?: boolean
  children: (propriedades: {
    id: string
    'aria-invalid': boolean
    'aria-describedby': string | undefined
  }) => ReactNode
}

/** Rótulo, controle, dica e mensagem de erro ligados por id — sem exceção. */
export function Campo({ rotulo, erro, dica, obrigatorio, children }: PropriedadesCampo) {
  const id = useId()
  const idDica = dica ? `${id}-dica` : undefined
  const idErro = erro ? `${id}-erro` : undefined
  const descrito = [idErro, idDica].filter(Boolean).join(' ') || undefined

  return (
    <div className="grid gap-1.5">
      <label htmlFor={id} className="text-[0.8125rem] font-medium text-ink-soft">
        {rotulo}
        {obrigatorio && (
          <span aria-hidden className="ml-0.5 text-negative">
            *
          </span>
        )}
      </label>

      {children({ id, 'aria-invalid': Boolean(erro), 'aria-describedby': descrito })}

      {erro ? (
        <p id={idErro} role="alert" className="flex items-start gap-1 text-xs text-negative-ink">
          <AlertCircle aria-hidden className="mt-px size-3.5 shrink-0" />
          {erro}
        </p>
      ) : (
        dica && (
          <p id={idDica} className="text-xs text-ink-muted">
            {dica}
          </p>
        )
      )}
    </div>
  )
}

export interface PropriedadesEntrada extends InputHTMLAttributes<HTMLInputElement> {
  prefixo?: ReactNode
  sufixo?: ReactNode
}

export const Entrada = forwardRef<HTMLInputElement, PropriedadesEntrada>(function Entrada(
  { className, prefixo, sufixo, ...resto },
  ref,
) {
  const invalido = resto['aria-invalid'] === true || resto['aria-invalid'] === 'true'

  const campo = (
    <input
      ref={ref}
      className={cn(
        controle,
        'h-9.5 px-3 text-sm',
        invalido ? estadoInvalido : estadoNormal,
        prefixo && 'pl-9',
        sufixo && 'pr-9',
        className,
      )}
      {...resto}
    />
  )

  if (!prefixo && !sufixo) return campo

  return (
    <div className="relative">
      {prefixo && (
        <span
          aria-hidden
          className="pointer-events-none absolute inset-y-0 left-0 grid w-9 place-items-center text-ink-muted"
        >
          {prefixo}
        </span>
      )}
      {campo}
      {sufixo && (
        <span className="absolute inset-y-0 right-0 grid w-9 place-items-center text-ink-muted">
          {sufixo}
        </span>
      )}
    </div>
  )
})

