import { useState } from 'react'

import { cn } from '@/lib/utils'

export interface PontoDoFluxo {
  chave: string
  rotulo: string
  rotuloLongo: string
  entradas: number
  saidas: number
  /** Destaca o período (ex.: atrasado) no rótulo do eixo. */
  destacado?: boolean
}

interface Propriedades {
  titulo: string
  dados: PontoDoFluxo[]
  formatarValor: (valor: number) => string
  formatarEixo: (valor: number) => string
  rotuloEntradas: string
  rotuloSaidas: string
}

const ALTURA_DO_PLOT = 176
const LINHAS_DE_GRADE = 4

/** Arredonda o topo do eixo para 1, 2, 2,5 ou 5 × 10ⁿ: ticks limpos. */
function topoDoEixo(maximo: number) {
  if (maximo <= 0) {
    return 1
  }
  const bruto = maximo / LINHAS_DE_GRADE
  const potencia = 10 ** Math.floor(Math.log10(bruto))
  const passo = [1, 2, 2.5, 5, 10].map((fator) => fator * potencia).find((candidato) => candidato >= bruto) ?? bruto
  return passo * LINHAS_DE_GRADE
}

/**
 * Colunas agrupadas: entradas e saídas previstas por período, num único eixo.
 *
 * Duas séries → legenda sempre visível; valores no tooltip (hover ou foco) e
 * numa tabela para leitor de tela, não em cima de cada coluna. As cores vêm
 * de --serie-1/--serie-2, validadas para daltonismo nos dois temas.
 */
export function FluxoPorPeriodo({
  titulo,
  dados,
  formatarValor,
  formatarEixo,
  rotuloEntradas,
  rotuloSaidas,
}: Propriedades) {
  const [ativo, definirAtivo] = useState<number | null>(null)
  const maximo = Math.max(...dados.flatMap((ponto) => [ponto.entradas, ponto.saidas]), 0)
  const topo = topoDoEixo(maximo)
  const totalEntradas = dados.reduce((soma, ponto) => soma + ponto.entradas, 0)
  const totalSaidas = dados.reduce((soma, ponto) => soma + ponto.saidas, 0)
  const pontoAtivo = ativo === null ? null : dados[ativo]

  const altura = (valor: number) => (valor <= 0 ? 0 : Math.max((valor / topo) * ALTURA_DO_PLOT, 3))

  return (
    // grid-cols-1 (minmax(0,1fr)): sem ele a coluna cresce até a largura
    // natural dos rótulos e o gráfico empurra a página no celular.
    <figure className="m-0 grid min-w-0 grid-cols-1 gap-4">
      <ul className="flex flex-wrap gap-x-5 gap-y-1.5 text-[0.8125rem]" aria-label="Legenda">
        <li className="flex items-center gap-2">
          <span aria-hidden className="size-2.5 rounded-sm bg-serie-1" />
          <span className="text-ink-soft">{rotuloEntradas}</span>
          <span className="numerico font-semibold text-ink">{formatarValor(totalEntradas)}</span>
        </li>
        <li className="flex items-center gap-2">
          <span aria-hidden className="size-2.5 rounded-sm bg-serie-2" />
          <span className="text-ink-soft">{rotuloSaidas}</span>
          <span className="numerico font-semibold text-ink">{formatarValor(totalSaidas)}</span>
        </li>
      </ul>

      <div className="relative flex gap-2" onMouseLeave={() => definirAtivo(null)}>
        {/* Eixo Y: ticks arredondados, texto em tom de apoio. */}
        <div aria-hidden className="relative w-10 shrink-0 sm:w-14" style={{ height: ALTURA_DO_PLOT }}>
          {Array.from({ length: LINHAS_DE_GRADE + 1 }, (_, indice) => {
            const valor = (topo / LINHAS_DE_GRADE) * indice
            return (
              <span
                key={indice}
                className="numerico absolute right-0 -translate-y-1/2 text-[0.625rem] whitespace-nowrap text-ink-muted sm:text-[0.6875rem]"
                style={{ bottom: (valor / topo) * ALTURA_DO_PLOT }}
              >
                {formatarEixo(valor)}
              </span>
            )
          })}
        </div>

        <div className="relative min-w-0 flex-1">
          <div aria-hidden className="pointer-events-none absolute inset-x-0 top-0" style={{ height: ALTURA_DO_PLOT }}>
            {Array.from({ length: LINHAS_DE_GRADE + 1 }, (_, indice) => (
              <div
                key={indice}
                className={cn('absolute inset-x-0 h-px', indice === 0 ? 'bg-line-strong' : 'bg-line')}
                style={{ bottom: (indice / LINHAS_DE_GRADE) * ALTURA_DO_PLOT }}
              />
            ))}
          </div>

          <ul className="relative flex" style={{ height: ALTURA_DO_PLOT }}>
            {dados.map((ponto, indice) => (
              <li
                key={ponto.chave}
                tabIndex={0}
                aria-label={`${ponto.rotuloLongo}: ${rotuloEntradas.toLowerCase()} ${formatarValor(ponto.entradas)}, ${rotuloSaidas.toLowerCase()} ${formatarValor(ponto.saidas)}`}
                onMouseEnter={() => definirAtivo(indice)}
                onFocus={() => definirAtivo(indice)}
                onBlur={() => definirAtivo(null)}
                // A área de toque é a faixa inteira do período, não só a coluna.
                className={cn(
                  'relative flex min-w-0 flex-1 cursor-default items-end justify-center gap-[2px] rounded-t-md px-1 outline-none',
                  ativo === indice && 'bg-surface-2/70',
                  'focus-visible:ring-2 focus-visible:ring-brand',
                )}
              >
                <span
                  className="w-full max-w-6 rounded-t-[4px] bg-serie-1"
                  style={{ height: altura(ponto.entradas) }}
                />
                <span
                  className="w-full max-w-6 rounded-t-[4px] bg-serie-2"
                  style={{ height: altura(ponto.saidas) }}
                />
              </li>
            ))}
          </ul>

          <ul aria-hidden className="mt-2 flex">
            {dados.map((ponto) => (
              <li
                key={ponto.chave}
                className={cn(
                  'min-w-0 flex-1 truncate text-center text-[0.625rem] sm:px-0.5 sm:text-[0.6875rem]',
                  ponto.destacado ? 'font-medium text-negative-ink' : 'text-ink-muted',
                )}
              >
                {ponto.rotulo}
              </li>
            ))}
          </ul>

          {pontoAtivo && ativo !== null && (
            <div
              role="status"
              className={cn(
                'pointer-events-none absolute z-10 w-52 rounded-lg border border-line bg-surface p-3 text-xs shadow-e3',
                // Nas pontas o tooltip encosta na borda em vez de sair do cartão.
                ativo === 0 ? 'left-0' : ativo === dados.length - 1 ? 'right-0' : '-translate-x-1/2',
              )}
              style={{
                bottom: ALTURA_DO_PLOT + 8,
                left: ativo === 0 || ativo === dados.length - 1 ? undefined : `${((ativo + 0.5) / dados.length) * 100}%`,
              }}
            >
              <p className="font-medium text-ink">{pontoAtivo.rotuloLongo}</p>
              <dl className="mt-2 grid gap-1">
                <div className="flex items-center gap-2">
                  <span aria-hidden className="size-2 rounded-sm bg-serie-1" />
                  <dt className="flex-1 text-ink-soft">{rotuloEntradas}</dt>
                  <dd className="numerico font-medium text-ink">{formatarValor(pontoAtivo.entradas)}</dd>
                </div>
                <div className="flex items-center gap-2">
                  <span aria-hidden className="size-2 rounded-sm bg-serie-2" />
                  <dt className="flex-1 text-ink-soft">{rotuloSaidas}</dt>
                  <dd className="numerico font-medium text-ink">{formatarValor(pontoAtivo.saidas)}</dd>
                </div>
                <div className="mt-1 flex items-center gap-2 border-t border-line pt-1.5">
                  <dt className="flex-1 text-ink-soft">Saldo do período</dt>
                  <dd className="numerico font-semibold text-ink">
                    {formatarValor(pontoAtivo.entradas - pontoAtivo.saidas)}
                  </dd>
                </div>
              </dl>
            </div>
          )}
        </div>
      </div>

      <div className="sr-only">
        <table>
          <caption>{titulo}</caption>
          <thead>
            <tr>
              <th scope="col">Período</th>
              <th scope="col">{rotuloEntradas}</th>
              <th scope="col">{rotuloSaidas}</th>
            </tr>
          </thead>
          <tbody>
            {dados.map((ponto) => (
              <tr key={ponto.chave}>
                <th scope="row">{ponto.rotuloLongo}</th>
                <td>{formatarValor(ponto.entradas)}</td>
                <td>{formatarValor(ponto.saidas)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </figure>
  )
}
