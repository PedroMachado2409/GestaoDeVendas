import type { ReactNode } from 'react'

interface Propriedades {
  titulo: string
  descricao?: string
  acoes?: ReactNode
}

export function CabecalhoDaPagina({ titulo, descricao, acoes }: Propriedades) {
  return (
    <header className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
      <div className="min-w-0 flex-1">
        <h1 className="text-xl font-semibold tracking-tight text-balance text-ink sm:text-2xl">
          {titulo}
        </h1>
        {descricao && (
          <p className="mt-1 max-w-3xl text-sm text-pretty text-ink-muted">{descricao}</p>
        )}
      </div>
      {acoes && (
        // No celular as ações dividem a linha em partes iguais.
        <div className="flex flex-wrap items-center gap-2 max-sm:*:flex-1 max-sm:*:justify-center sm:shrink-0">
          {acoes}
        </div>
      )}
    </header>
  )
}
