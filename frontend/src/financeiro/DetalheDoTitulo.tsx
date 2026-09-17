import { Ban, HandCoins } from 'lucide-react'

import { Botao } from '@/componentes/ui/Botao'
import { Etiqueta } from '@/componentes/ui/Etiqueta'
import { Modal } from '@/componentes/ui/Modal'
import { cn, formatarMoeda } from '@/lib/utils'

import { descreverVencimento, formatarDataISO } from './datas'
import {
  estaEmAberto,
  rotuloDeBaixado,
  rotulosDeForma,
  rotulosDeOrigem,
  rotulosDeSituacao,
  saldoDo,
  situacaoDo,
  tonsDeSituacao,
} from './titulos'
import type { TituloFinanceiro } from './tipos'

interface Propriedades {
  titulo: TituloFinanceiro | null
  /** Todas as parcelas do mesmo documento, inclusive a aberta. */
  parcelasDoDocumento: TituloFinanceiro[]
  aoBaixar: (titulo: TituloFinanceiro) => void
  aoCancelar: (titulo: TituloFinanceiro) => void
  aoEscolherParcela: (titulo: TituloFinanceiro) => void
  aoFechar: () => void
}

export function DetalheDoTitulo({
  titulo,
  parcelasDoDocumento,
  aoBaixar,
  aoCancelar,
  aoEscolherParcela,
  aoFechar,
}: Propriedades) {
  if (!titulo) {
    return null
  }

  const situacao = situacaoDo(titulo)
  const aberto = estaEmAberto(titulo)
  const totalDoDocumento = parcelasDoDocumento.reduce((soma, parcela) => soma + parcela.valorOriginal, 0)

  return (
    <Modal
      aberto={titulo !== null}
      aoFechar={aoFechar}
      titulo={`${titulo.numeroDocumento} · parcela ${titulo.numeroParcela}/${titulo.totalParcelas}`}
      descricao={titulo.descricao}
      largura="larga"
      rodape={
        <>
          <div className="mr-auto flex items-baseline gap-2 max-sm:w-full max-sm:justify-between">
            <span className="text-[0.8125rem] text-ink-muted">Saldo</span>
            <span className="numerico text-lg font-semibold tracking-tight text-ink">
              {formatarMoeda(saldoDo(titulo))}
            </span>
          </div>
          {aberto ? (
            <>
              <Botao
                variante="sutil"
                onClick={() => aoCancelar(titulo)}
                disabled={titulo.valorBaixado > 0}
                title={titulo.valorBaixado > 0 ? 'Título com baixa registrada não pode ser cancelado' : undefined}
                iconeInicial={<Ban aria-hidden className="size-4" />}
                className="text-negative-ink hover:bg-negative-soft hover:text-negative-ink"
              >
                Cancelar
              </Botao>
              <Botao
                variante="primario"
                onClick={() => aoBaixar(titulo)}
                iconeInicial={<HandCoins aria-hidden className="size-4" />}
              >
                {titulo.tipo === 'Receber' ? 'Receber' : 'Pagar'}
              </Botao>
            </>
          ) : (
            <Botao variante="secundario" onClick={aoFechar}>
              Fechar
            </Botao>
          )}
        </>
      }
    >
      <div className="grid gap-5">
        <dl className="grid grid-cols-2 gap-x-4 gap-y-3 sm:grid-cols-4">
          <Dado rotulo={titulo.tipo === 'Receber' ? 'Cliente' : 'Fornecedor'}>{titulo.contraparteNome}</Dado>
          <Dado rotulo="Situação">
            <Etiqueta tom={tonsDeSituacao[situacao]} comPonto>
              {rotulosDeSituacao[situacao]}
            </Etiqueta>
          </Dado>
          <Dado rotulo="Vencimento">
            <span className="numerico">{formatarDataISO(titulo.dataVencimento)}</span>
            {aberto && (
              <span className={cn('block text-xs font-normal', situacao === 'Vencido' ? 'text-negative-ink' : 'text-ink-muted')}>
                {descreverVencimento(titulo.dataVencimento)}
              </span>
            )}
          </Dado>
          <Dado rotulo="Origem">
            {rotulosDeOrigem[titulo.origem]}
            {titulo.pedidoId && <span className="numerico block text-xs font-normal text-ink-muted">pedido #{titulo.pedidoId}</span>}
          </Dado>
          <Dado rotulo="Valor original">
            <span className="numerico">{formatarMoeda(titulo.valorOriginal)}</span>
          </Dado>
          <Dado rotulo={rotuloDeBaixado(titulo.tipo)}>
            <span className="numerico">{formatarMoeda(titulo.valorBaixado)}</span>
          </Dado>
          <Dado rotulo="Prazo">{titulo.prazoNome}</Dado>
          <Dado rotulo="Emissão">
            <span className="numerico">{formatarDataISO(titulo.dataCadastro)}</span>
          </Dado>
        </dl>

        {titulo.observacao && (
          <p className="rounded-lg border border-line bg-surface-2/60 px-3 py-2 text-sm text-ink-soft">{titulo.observacao}</p>
        )}

        <section className="grid gap-2">
          <h3 className="flex items-baseline justify-between gap-2 text-[0.8125rem] font-medium text-ink-soft">
            Parcelas do documento
            <span className="numerico text-xs font-normal text-ink-muted">total {formatarMoeda(totalDoDocumento)}</span>
          </h3>
          <ol className="grid gap-1.5">
            {parcelasDoDocumento.map((parcela) => {
              const situacaoDaParcela = situacaoDo(parcela)
              const atual = parcela.id === titulo.id
              return (
                <li key={parcela.id}>
                  <button
                    type="button"
                    onClick={() => aoEscolherParcela(parcela)}
                    aria-current={atual}
                    className={cn(
                      'flex w-full flex-wrap items-center gap-x-3 gap-y-1 rounded-lg border px-3 py-2 text-left text-sm transition-colors',
                      atual ? 'border-brand/40 bg-brand-soft/60' : 'border-line hover:bg-surface-2',
                    )}
                  >
                    <span className="numerico w-10 font-medium text-ink">
                      {parcela.numeroParcela}/{parcela.totalParcelas}
                    </span>
                    <span className="numerico text-ink-soft">{formatarDataISO(parcela.dataVencimento)}</span>
                    <span className="numerico ml-auto font-semibold text-ink">{formatarMoeda(parcela.valorOriginal)}</span>
                    <Etiqueta tom={tonsDeSituacao[situacaoDaParcela]} comPonto className="max-sm:basis-auto">
                      {rotulosDeSituacao[situacaoDaParcela]}
                    </Etiqueta>
                  </button>
                </li>
              )
            })}
          </ol>
        </section>

        <section className="grid gap-2">
          <h3 className="text-[0.8125rem] font-medium text-ink-soft">
            {titulo.tipo === 'Receber' ? 'Recebimentos' : 'Pagamentos'}
          </h3>
          {titulo.baixas.length === 0 ? (
            <p className="rounded-lg border border-dashed border-line-strong px-4 py-4 text-center text-xs text-ink-muted">
              Nenhuma baixa registrada nesta parcela.
            </p>
          ) : (
            <ul className="divide-y divide-line rounded-lg border border-line">
              {titulo.baixas.map((baixa) => (
                <li key={baixa.id} className="flex flex-wrap items-center gap-x-3 gap-y-0.5 px-3 py-2 text-sm">
                  <span className="numerico text-ink">{formatarDataISO(baixa.data)}</span>
                  <span className="text-ink-soft">{rotulosDeForma[baixa.forma]}</span>
                  <span className="text-xs text-ink-muted">por {baixa.usuarioNome}</span>
                  <span className="numerico ml-auto font-semibold text-positive-ink">{formatarMoeda(baixa.valor)}</span>
                </li>
              ))}
            </ul>
          )}
        </section>
      </div>
    </Modal>
  )
}

function Dado({ rotulo, children }: { rotulo: string; children: React.ReactNode }) {
  return (
    <div className="min-w-0">
      <dt className="text-xs text-ink-muted">{rotulo}</dt>
      <dd className="mt-0.5 text-sm font-medium text-pretty text-ink">{children}</dd>
    </div>
  )
}
