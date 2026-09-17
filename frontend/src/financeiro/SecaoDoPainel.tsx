import { ArrowDownLeft, ArrowRight, ArrowUpRight, CalendarClock, Landmark, Scale, TriangleAlert, Wallet } from 'lucide-react'
import { Link } from 'react-router-dom'

import { FluxoPorPeriodo } from '@/componentes/graficos/FluxoPorPeriodo'
import { CabecalhoDoCartao, Cartao, CorpoDoCartao } from '@/componentes/ui/Cartao'
import { EstadoVazio } from '@/componentes/ui/Estados'
import { Etiqueta } from '@/componentes/ui/Etiqueta'
import { Indicador } from '@/componentes/ui/Indicador'
import { cn, formatarMoeda, formatarMoedaCompacta, pluralizar } from '@/lib/utils'

import { descreverVencimento, formatarDataISO, hoje } from './datas'
import { calcularPosicao, fluxoPorSemana, proximosVencimentos } from './resumo'
import { estaVencido, saldoDo } from './titulos'
import { useFinanceiro } from './useFinanceiro'

const SEMANAS_NO_FLUXO = 6

const eixoCompacto = new Intl.NumberFormat('pt-BR', { notation: 'compact', maximumFractionDigits: 1 })

/** Resumo do financeiro no Painel: posição, fluxo previsto e o que vence primeiro. */
export function SecaoFinanceiraDoPainel() {
  const { titulos, ficticio } = useFinanceiro()
  const referencia = hoje()
  const posicao = calcularPosicao(titulos, referencia)
  const fluxo = fluxoPorSemana(titulos, SEMANAS_NO_FLUXO, referencia)
  const proximos = proximosVencimentos(titulos, 6)

  return (
    <section aria-labelledby="titulo-financeiro-painel" className="grid grid-cols-1 gap-4">
      <div className="flex flex-wrap items-center justify-between gap-x-4 gap-y-2">
        <div className="flex items-center gap-2.5">
          <span aria-hidden className="grid size-7 place-items-center rounded-lg bg-surface-2 text-ink-muted">
            <Landmark className="size-3.5" />
          </span>
          <h2 id="titulo-financeiro-painel" className="text-base font-semibold tracking-tight text-ink">
            Financeiro
          </h2>
          {ficticio && <Etiqueta tom="atencao">Dados fictícios</Etiqueta>}
        </div>
        <Link
          to="/financeiro"
          className="inline-flex items-center gap-1 rounded text-[0.8125rem] font-medium text-brand-ink hover:underline"
        >
          Abrir financeiro
          <ArrowRight aria-hidden className="size-3.5" />
        </Link>
      </div>

      <div className="grid grid-cols-1 gap-3 min-[420px]:grid-cols-2 sm:gap-4 xl:grid-cols-4">
        <Indicador
          rotulo="A receber"
          Icone={Wallet}
          valor={formatarMoedaCompacta(posicao.aReceber)}
          apoio={
            posicao.quantidadeVencidaReceber > 0 ? (
              <span className="text-negative-ink">
                {formatarMoeda(posicao.vencidoReceber)} em atraso
              </span>
            ) : (
              'Nada em atraso'
            )
          }
        />
        <Indicador
          rotulo="A pagar"
          Icone={ArrowUpRight}
          valor={formatarMoedaCompacta(posicao.aPagar)}
          apoio={
            posicao.quantidadeVencidaPagar > 0 ? (
              <span className="text-negative-ink">
                {formatarMoeda(posicao.vencidoPagar)} em atraso
              </span>
            ) : (
              'Nada em atraso'
            )
          }
        />
        <Indicador
          rotulo="Saldo previsto"
          Icone={Scale}
          destaque
          valor={formatarMoedaCompacta(posicao.saldoPrevisto)}
          apoio="A receber menos a pagar, em aberto"
        />
        <Indicador
          rotulo="Títulos em atraso"
          Icone={TriangleAlert}
          valor={String(posicao.quantidadeVencidaReceber + posicao.quantidadeVencidaPagar)}
          apoio={`${posicao.quantidadeVencidaReceber} a receber · ${posicao.quantidadeVencidaPagar} a pagar`}
        />
      </div>

      <div className="grid grid-cols-1 gap-5 sm:gap-6 lg:grid-cols-3">
        <Cartao className="lg:col-span-2">
          <CabecalhoDoCartao
            titulo="Fluxo previsto"
            descricao={`Saldo em aberto por semana de vencimento, nas próximas ${SEMANAS_NO_FLUXO} semanas`}
          />
          <CorpoDoCartao>
            <FluxoPorPeriodo
              titulo={`Entradas e saídas previstas nas próximas ${SEMANAS_NO_FLUXO} semanas`}
              dados={fluxo.map((periodo) => ({ ...periodo, destacado: periodo.atrasado && (periodo.entradas > 0 || periodo.saidas > 0) }))}
              formatarValor={formatarMoeda}
              formatarEixo={(valor) => eixoCompacto.format(valor)}
              rotuloEntradas="Entradas"
              rotuloSaidas="Saídas"
            />
          </CorpoDoCartao>
        </Cartao>

        <Cartao>
          <CabecalhoDoCartao titulo="Próximos vencimentos" descricao="Em aberto, dos atrasados para os próximos" />
          {proximos.length === 0 ? (
            <EstadoVazio
              icone={<CalendarClock aria-hidden className="size-5" />}
              titulo="Nada em aberto"
              descricao="Todos os títulos estão baixados ou cancelados."
            />
          ) : (
            <ul className="divide-y divide-line">
              {proximos.map((titulo) => {
                const receber = titulo.tipo === 'Receber'
                const vencido = estaVencido(titulo, referencia)
                return (
                  <li key={titulo.id}>
                    <Link
                      to={`/financeiro${receber ? '' : '?aba=pagar'}`}
                      className="flex items-center gap-3 px-5 py-3 transition-colors hover:bg-surface-2/70"
                    >
                      {/* Ícone + texto: o sentido não depende só da cor. */}
                      <span
                        aria-hidden
                        className={cn(
                          'grid size-8 shrink-0 place-items-center rounded-lg',
                          receber ? 'bg-brand-soft text-brand-ink' : 'bg-surface-2 text-ink-soft',
                        )}
                      >
                        {receber ? <ArrowDownLeft className="size-4" /> : <ArrowUpRight className="size-4" />}
                      </span>
                      <div className="min-w-0 flex-1">
                        <p className="truncate text-sm text-ink">{titulo.contraparteNome}</p>
                        <p className="truncate text-xs text-ink-muted">
                          <span className="sr-only">{receber ? 'A receber' : 'A pagar'}: </span>
                          <span className="numerico">{formatarDataISO(titulo.dataVencimento)}</span>
                          {' · '}
                          <span className={cn(vencido && 'font-medium text-negative-ink')}>
                            {descreverVencimento(titulo.dataVencimento, referencia)}
                          </span>
                        </p>
                      </div>
                      <div className="shrink-0 text-right">
                        <p className="numerico text-sm font-semibold text-ink">{formatarMoeda(saldoDo(titulo))}</p>
                        <p className="text-xs text-ink-muted">
                          {receber ? 'receber' : 'pagar'} · {titulo.numeroParcela}/{titulo.totalParcelas}
                        </p>
                      </div>
                    </Link>
                  </li>
                )
              })}
            </ul>
          )}
          <div className="border-t border-line px-5 py-2.5 text-xs text-ink-muted">
            {proximos.length > 0 &&
              `${titulos.filter((titulo) => estaVencido(titulo, referencia)).length} ${pluralizar(
                titulos.filter((titulo) => estaVencido(titulo, referencia)).length,
                'título vencido',
                'títulos vencidos',
              )} no total`}
          </div>
        </Cartao>
      </div>
    </section>
  )
}
