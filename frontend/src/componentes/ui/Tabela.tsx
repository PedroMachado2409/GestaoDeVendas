import {
  ArrowDownWideNarrow,
  ArrowUpNarrowWide,
  Check,
  ChevronDown,
  ChevronsUpDown,
  ChevronUp,
} from 'lucide-react'
import { useMemo, useState, type ReactNode } from 'react'

import { cn, comparar } from '@/lib/utils'

import { ItemDeMenu, Menu } from './Menu'

export interface Coluna<T> {
  chave: string
  cabecalho: ReactNode
  celula: (item: T) => ReactNode
  /** Valor usado para ordenar; sem ele a coluna não é ordenável. */
  valor?: (item: T) => string | number | boolean
  alinhamento?: 'inicio' | 'fim' | 'centro'
  larguraMinima?: string
  /** Colunas secundárias somem em telas estreitas (na tabela e no cartão). */
  ocultarEmTelaPequena?: boolean
  /**
   * Rótulo em texto para o cartão e para o seletor de ordenação no celular.
   * Obrigatório na prática quando o cabeçalho não é texto simples.
   */
  rotulo?: string
}

type Direcao = 'crescente' | 'decrescente'

interface Propriedades<T> {
  itens: T[]
  colunas: Array<Coluna<T>>
  chaveDoItem: (item: T) => string | number
  ordenacaoInicial?: { chave: string; direcao: Direcao }
  /** Renderizado no lugar do corpo quando a lista está vazia. */
  vazio?: ReactNode
  aoClicarNaLinha?: (item: T) => void
  legenda?: string
}

const alinhamentos = {
  inicio: 'text-left',
  fim: 'text-right',
  centro: 'text-center',
} as const

// Convenção das telas: a coluna de menu de ações se chama "acoes". No cartão
// ela vai para o canto, em vez de virar um par rótulo/valor.
const CHAVE_ACOES = 'acoes'

function rotuloDaColuna<T>(coluna: Coluna<T>) {
  if (coluna.rotulo) {
    return coluna.rotulo
  }
  return typeof coluna.cabecalho === 'string' ? coluna.cabecalho : coluna.chave
}

/**
 * Tabela ordenável. Abaixo de 768 px a mesma lista vira cartões empilhados:
 * rolar uma tabela de cinco colunas na horizontal num celular esconde
 * justamente o que a pessoa foi procurar.
 */
export function Tabela<T>({
  itens,
  colunas,
  chaveDoItem,
  ordenacaoInicial,
  vazio,
  aoClicarNaLinha,
  legenda,
}: Propriedades<T>) {
  const [ordenacao, definirOrdenacao] = useState<{ chave: string; direcao: Direcao } | null>(
    ordenacaoInicial ?? null,
  )

  const ordenados = useMemo(() => {
    if (!ordenacao) {
      return itens
    }
    const coluna = colunas.find((c) => c.chave === ordenacao.chave)
    if (!coluna?.valor) {
      return itens
    }

    const extrair = coluna.valor
    const sinal = ordenacao.direcao === 'crescente' ? 1 : -1
    return [...itens].sort((a, b) => comparar(extrair(a), extrair(b)) * sinal)
  }, [itens, colunas, ordenacao])

  function alternar(chave: string) {
    definirOrdenacao((atual) => {
      if (atual?.chave !== chave) {
        return { chave, direcao: 'crescente' }
      }
      return {
        chave,
        direcao: atual.direcao === 'crescente' ? 'decrescente' : 'crescente',
      }
    })
  }

  if (itens.length === 0 && vazio) {
    return <>{vazio}</>
  }

  const [principal, ...demais] = colunas
  const acoes = demais.find((coluna) => coluna.chave === CHAVE_ACOES)
  const secundarias = demais.filter(
    (coluna) => coluna.chave !== CHAVE_ACOES && !coluna.ocultarEmTelaPequena,
  )
  const ordenaveis = colunas.filter((coluna) => coluna.valor)
  const colunaOrdenada = ordenaveis.find((coluna) => coluna.chave === ordenacao?.chave)

  return (
    <>
      {/* Celular e tablet em retrato */}
      <div className="md:hidden">
        {ordenaveis.length > 0 && (
          <div className="flex items-center gap-2 border-b border-line bg-surface-2/40 px-4 py-2">
            <span className="text-xs font-medium text-ink-muted">Ordenar por</span>
            <Menu
              rotulo="Escolher coluna de ordenação"
              alinhamento="inicio"
              className="min-w-0 flex-1"
              classeGatilho="size-auto h-8 w-full justify-between gap-2 rounded-md border border-line bg-surface px-2.5 text-[0.8125rem] text-ink hover:bg-surface md:h-8 md:w-full"
              gatilho={
                <>
                  <span className="truncate">
                    {colunaOrdenada ? rotuloDaColuna(colunaOrdenada) : 'Padrão'}
                  </span>
                  <ChevronDown aria-hidden className="size-4 shrink-0 text-ink-muted" />
                </>
              }
            >
              {(fechar) =>
                ordenaveis.map((coluna) => (
                  <ItemDeMenu
                    key={coluna.chave}
                    icone={
                      <Check
                        aria-hidden
                        className={cn(
                          'size-4',
                          ordenacao?.chave === coluna.chave ? 'text-brand' : 'invisible',
                        )}
                      />
                    }
                    onClick={() => {
                      fechar()
                      definirOrdenacao({
                        chave: coluna.chave,
                        direcao: ordenacao?.direcao ?? 'crescente',
                      })
                    }}
                  >
                    {rotuloDaColuna(coluna)}
                  </ItemDeMenu>
                ))
              }
            </Menu>
            <button
              type="button"
              disabled={!ordenacao}
              onClick={() => ordenacao && alternar(ordenacao.chave)}
              aria-label={
                ordenacao?.direcao === 'decrescente'
                  ? 'Ordem decrescente; mudar para crescente'
                  : 'Ordem crescente; mudar para decrescente'
              }
              className="grid size-8 shrink-0 place-items-center rounded-md border border-line bg-surface text-ink-soft transition-colors hover:text-ink disabled:opacity-50"
            >
              {ordenacao?.direcao === 'decrescente' ? (
                <ArrowDownWideNarrow aria-hidden className="size-4" />
              ) : (
                <ArrowUpNarrowWide aria-hidden className="size-4" />
              )}
            </button>
          </div>
        )}

        <ul aria-label={legenda} className="relative divide-y divide-line">
          {ordenados.map((item) => (
            <li
              key={chaveDoItem(item)}
              onClick={aoClicarNaLinha ? () => aoClicarNaLinha(item) : undefined}
              className={cn(
                'flex items-start gap-3 px-4 py-3.5',
                aoClicarNaLinha && 'cursor-pointer active:bg-surface-2/70',
              )}
            >
              <div className="min-w-0 flex-1">
                {principal && <div className="min-w-0 text-sm">{principal.celula(item)}</div>}

                {secundarias.length > 0 && (
                  <dl className="mt-2.5 grid grid-cols-2 gap-x-4 gap-y-2.5">
                    {secundarias.map((coluna) => (
                      <div key={coluna.chave} className="min-w-0">
                        <dt className="text-[0.6875rem] font-medium tracking-wide text-ink-muted uppercase">
                          {rotuloDaColuna(coluna)}
                        </dt>
                        <dd className="mt-0.5 line-clamp-2 min-w-0 text-sm [overflow-wrap:anywhere]">
                          {coluna.celula(item)}
                        </dd>
                      </div>
                    ))}
                  </dl>
                )}
              </div>

              {acoes && (
                <div className="-mt-1 -mr-1.5 shrink-0" onClick={(evento) => evento.stopPropagation()}>
                  {acoes.celula(item)}
                </div>
              )}
            </li>
          ))}
        </ul>
      </div>

      {/* A partir de 768 px. "relative" prende os rótulos sr-only (absolutos)
          dentro da área que rola; sem ele, escapavam e alargavam a página. */}
      <div className="scrollbar-fina relative hidden overflow-x-auto md:block">
        <table className="w-full border-collapse text-sm">
          {legenda && <caption className="sr-only">{legenda}</caption>}
          <thead>
            <tr className="border-b border-line">
              {colunas.map((coluna) => {
                const ativa = ordenacao?.chave === coluna.chave
                const ordemAria = !ativa
                  ? undefined
                  : ordenacao.direcao === 'crescente'
                    ? 'ascending'
                    : 'descending'

                return (
                  <th
                    key={coluna.chave}
                    scope="col"
                    aria-sort={coluna.valor ? (ordemAria ?? 'none') : undefined}
                    style={coluna.larguraMinima ? { minWidth: coluna.larguraMinima } : undefined}
                    className={cn(
                      'bg-surface-2/60 px-4 py-2.5 text-xs font-semibold tracking-wide whitespace-nowrap text-ink-muted uppercase',
                      alinhamentos[coluna.alinhamento ?? 'inicio'],
                      coluna.ocultarEmTelaPequena && 'hidden xl:table-cell',
                    )}
                  >
                    {coluna.valor ? (
                      <button
                        type="button"
                        onClick={() => alternar(coluna.chave)}
                        className={cn(
                          'inline-flex items-center gap-1 rounded uppercase transition-colors hover:text-ink',
                          ativa && 'text-ink',
                          coluna.alinhamento === 'fim' && 'flex-row-reverse',
                        )}
                      >
                        {coluna.cabecalho}
                        {!ativa ? (
                          <ChevronsUpDown aria-hidden className="size-3.5 opacity-50" />
                        ) : ordenacao.direcao === 'crescente' ? (
                          <ChevronUp aria-hidden className="size-3.5" />
                        ) : (
                          <ChevronDown aria-hidden className="size-3.5" />
                        )}
                      </button>
                    ) : (
                      coluna.cabecalho
                    )}
                  </th>
                )
              })}
            </tr>
          </thead>
          <tbody>
            {ordenados.map((item) => (
              <tr
                key={chaveDoItem(item)}
                onClick={aoClicarNaLinha ? () => aoClicarNaLinha(item) : undefined}
                className={cn(
                  'border-b border-line/70 last:border-0',
                  aoClicarNaLinha && 'cursor-pointer',
                  'transition-colors hover:bg-surface-2/70',
                )}
              >
                {colunas.map((coluna) => (
                  <td
                    key={coluna.chave}
                    className={cn(
                      'px-4 py-3 align-middle',
                      alinhamentos[coluna.alinhamento ?? 'inicio'],
                      coluna.ocultarEmTelaPequena && 'hidden xl:table-cell',
                    )}
                  >
                    {coluna.celula(item)}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </>
  )
}
