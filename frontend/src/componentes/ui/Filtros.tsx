import { Search, X } from 'lucide-react'

import { cn } from '@/lib/utils'

import { Entrada } from './Campo'

interface PropriedadesBusca {
  valor: string
  aoMudar: (valor: string) => void
  placeholder?: string
  rotulo: string
  className?: string
}

export function CampoDeBusca({
  valor,
  aoMudar,
  placeholder,
  rotulo,
  className,
}: PropriedadesBusca) {
  return (
    <div className={cn('relative w-full sm:max-w-xs', className)}>
      <Entrada
        type="search"
        role="searchbox"
        aria-label={rotulo}
        value={valor}
        placeholder={placeholder}
        onChange={(evento) => aoMudar(evento.target.value)}
        prefixo={<Search aria-hidden className="size-4" />}
        className="pr-9"
      />
      {valor && (
        <button
          type="button"
          onClick={() => aoMudar('')}
          aria-label="Limpar busca"
          className="absolute inset-y-0 right-0 grid w-9 place-items-center text-ink-muted transition-colors hover:text-ink"
        >
          <X aria-hidden className="size-4" />
        </button>
      )}
    </div>
  )
}

export interface OpcaoDeSegmento<T extends string> {
  valor: T
  rotulo: string
  contagem?: number
}

interface PropriedadesSegmentos<T extends string> {
  opcoes: Array<OpcaoDeSegmento<T>>
  valor: T
  aoMudar: (valor: T) => void
  rotulo: string
}

/** Controle segmentado para filtros mutuamente exclusivos (ativo/inativo/todos). */
export function Segmentos<T extends string>({
  opcoes,
  valor,
  aoMudar,
  rotulo,
}: PropriedadesSegmentos<T>) {
  return (
    // Em tela estreita o grupo rola na horizontal dentro de si mesmo, em vez
    // de empurrar a largura do cartão.
    <div
      role="group"
      aria-label={rotulo}
      className="scrollbar-oculta inline-flex max-w-full items-center gap-0.5 overflow-x-auto rounded-lg border border-line bg-surface-2 p-0.5"
    >
      {opcoes.map((opcao) => {
        const ativo = opcao.valor === valor
        return (
          <button
            key={opcao.valor}
            type="button"
            aria-pressed={ativo}
            onClick={(evento) => {
              aoMudar(opcao.valor)
              evento.currentTarget.scrollIntoView({ block: 'nearest', inline: 'nearest' })
            }}
            className={cn(
              'inline-flex h-8 shrink-0 items-center gap-1.5 rounded-md px-2.5 text-[0.8125rem] font-medium whitespace-nowrap transition-colors sm:h-7',
              ativo
                ? 'bg-surface text-ink shadow-e1'
                : 'text-ink-muted hover:text-ink',
            )}
          >
            {opcao.rotulo}
            {opcao.contagem !== undefined && (
              <span
                className={cn(
                  'numerico rounded px-1 text-[0.6875rem]',
                  ativo ? 'bg-surface-3 text-ink-soft' : 'text-ink-muted',
                )}
              >
                {opcao.contagem}
              </span>
            )}
          </button>
        )
      })}
    </div>
  )
}
