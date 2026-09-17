import { useEffect, useState } from 'react'

import { Botao } from '@/componentes/ui/Botao'
import { Campo, Entrada } from '@/componentes/ui/Campo'
import { Modal } from '@/componentes/ui/Modal'
import { cn, formatarMoeda } from '@/lib/utils'

import { diasEntre, formatarDataISO, hoje } from './datas'
import { lerNumero } from './prazos'
import { rotuloDeBaixado, rotulosDeForma, saldoDo } from './titulos'
import type { DataISO, FormaPagamento, TituloFinanceiro } from './tipos'

const formas = Object.keys(rotulosDeForma) as FormaPagamento[]

interface Propriedades {
  titulo: TituloFinanceiro | null
  aoConfirmar: (dados: { valor: number; data: DataISO; forma: FormaPagamento }) => void
  aoFechar: () => void
}

export function BaixaDeTitulo({ titulo, aoConfirmar, aoFechar }: Propriedades) {
  const [valorTexto, definirValorTexto] = useState('')
  const [data, definirData] = useState<DataISO>(hoje())
  const [forma, definirForma] = useState<FormaPagamento>('Pix')
  const [tentou, definirTentou] = useState(false)

  useEffect(() => {
    if (titulo) {
      definirValorTexto(saldoDo(titulo).toFixed(2).replace('.', ','))
      definirData(hoje())
      definirForma('Pix')
      definirTentou(false)
    }
  }, [titulo])

  if (!titulo) {
    return null
  }

  const saldo = saldoDo(titulo)
  const valor = lerNumero(valorTexto)
  const acao = titulo.tipo === 'Receber' ? 'recebimento' : 'pagamento'

  let erroValor: string | undefined
  if (!Number.isFinite(valor) || valor <= 0) {
    erroValor = 'Informe um valor maior que zero.'
  } else if (Math.round(valor * 100) > Math.round(saldo * 100)) {
    erroValor = `O valor não pode passar do saldo (${formatarMoeda(saldo)}).`
  }

  let erroData: string | undefined
  if (!data) {
    erroData = 'Informe a data.'
  } else if (diasEntre(hoje(), data) > 0) {
    erroData = 'A baixa não pode ter data futura.'
  }

  const valido = !erroValor && !erroData
  const parcial = valido && Math.round(valor * 100) < Math.round(saldo * 100)

  function confirmar() {
    definirTentou(true)
    if (!valido) {
      return
    }
    aoConfirmar({ valor: Math.round(valor * 100) / 100, data, forma })
  }

  return (
    <Modal
      aberto={titulo !== null}
      aoFechar={aoFechar}
      titulo={`Registrar ${acao}`}
      descricao={`${titulo.descricao} · parcela ${titulo.numeroParcela}/${titulo.totalParcelas} · vence ${formatarDataISO(titulo.dataVencimento)}`}
      largura="estreita"
      rodape={
        <>
          <Botao variante="sutil" onClick={aoFechar}>
            Cancelar
          </Botao>
          <Botao variante="primario" onClick={confirmar}>
            {parcial ? `Baixar parcialmente` : `Confirmar ${acao}`}
          </Botao>
        </>
      }
    >
      <form
        className="grid gap-4"
        noValidate
        onSubmit={(evento) => {
          evento.preventDefault()
          confirmar()
        }}
      >
        <dl className="grid grid-cols-3 gap-2 rounded-xl border border-line bg-surface-2/60 p-3 text-center">
          <div>
            <dt className="text-[0.6875rem] text-ink-muted uppercase">Original</dt>
            <dd className="numerico mt-0.5 text-sm font-medium text-ink">{formatarMoeda(titulo.valorOriginal)}</dd>
          </div>
          <div>
            <dt className="text-[0.6875rem] text-ink-muted uppercase">{rotuloDeBaixado(titulo.tipo)}</dt>
            <dd className="numerico mt-0.5 text-sm font-medium text-ink">{formatarMoeda(titulo.valorBaixado)}</dd>
          </div>
          <div>
            <dt className="text-[0.6875rem] text-ink-muted uppercase">Saldo</dt>
            <dd className="numerico mt-0.5 text-sm font-semibold text-ink">{formatarMoeda(saldo)}</dd>
          </div>
        </dl>

        <div className="grid gap-4 sm:grid-cols-2">
          <Campo
            rotulo="Valor (R$)"
            erro={tentou ? erroValor : undefined}
            dica={parcial ? `Fica em aberto ${formatarMoeda(Math.round((saldo - valor) * 100) / 100)}.` : undefined}
            obrigatorio
          >
            {(propriedades) => (
              <Entrada
                {...propriedades}
                value={valorTexto}
                onChange={(evento) => definirValorTexto(evento.target.value.replace(/[^\d.,]/g, ''))}
                inputMode="decimal"
                className="numerico"
                autoFocus
              />
            )}
          </Campo>
          <Campo rotulo="Data" erro={tentou ? erroData : undefined} obrigatorio>
            {(propriedades) => (
              <Entrada
                {...propriedades}
                type="date"
                value={data}
                max={hoje()}
                onChange={(evento) => definirData(evento.target.value)}
                className="numerico"
              />
            )}
          </Campo>
        </div>

        <fieldset className="grid gap-1.5">
          <legend className="mb-1.5 text-[0.8125rem] font-medium text-ink-soft">Forma</legend>
          <div className="grid grid-cols-2 gap-1.5 sm:grid-cols-3">
            {formas.map((opcao) => (
              <button
                key={opcao}
                type="button"
                role="radio"
                aria-checked={forma === opcao}
                onClick={() => definirForma(opcao)}
                className={cn(
                  'h-9 rounded-lg border px-2 text-[0.8125rem] font-medium transition-colors',
                  forma === opcao
                    ? 'border-brand bg-brand-soft text-brand-ink'
                    : 'border-line bg-surface text-ink-soft hover:border-line-strong hover:text-ink',
                )}
              >
                {rotulosDeForma[opcao]}
              </button>
            ))}
          </div>
        </fieldset>
      </form>
    </Modal>
  )
}
