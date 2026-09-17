import { AlertCircle, CheckCircle2, Divide, Plus, Trash2 } from 'lucide-react'
import { useEffect, useState } from 'react'

import { Botao } from '@/componentes/ui/Botao'
import { Campo, Entrada } from '@/componentes/ui/Campo'
import { Modal } from '@/componentes/ui/Modal'
import { cn, formatarMoeda } from '@/lib/utils'

import { formatarDiaMes, hoje } from './datas'
import {
  distribuirIgualmente,
  formatarPercentual,
  gerarParcelas,
  lerNumero,
  montarParcelas,
  somaDosPercentuais,
  validarPrazo,
  type ParcelaEmEdicao,
} from './prazos'
import type { PrazoPagamento } from './tipos'

const VALOR_DE_EXEMPLO = 1_000

interface Propriedades {
  aberto: boolean
  /** Prazo sendo editado; null para criar um novo. */
  prazo: PrazoPagamento | null
  /** Na criação, preenche o formulário a partir de outro prazo (duplicar). */
  modelo?: PrazoPagamento | null
  nomesExistentes: string[]
  aoSalvar: (dados: Pick<PrazoPagamento, 'nome' | 'parcelas'>) => void
  aoFechar: () => void
}

function paraEdicao(prazo: PrazoPagamento | null): ParcelaEmEdicao[] {
  if (!prazo) {
    return [
      { dias: '30', percentual: '50' },
      { dias: '60', percentual: '50' },
    ]
  }
  return prazo.parcelas.map((parcela) => ({
    dias: String(parcela.dias),
    percentual: String(parcela.percentual).replace('.', ','),
  }))
}

export function FormularioDePrazo({ aberto, prazo, modelo = null, nomesExistentes, aoSalvar, aoFechar }: Propriedades) {
  const [nome, definirNome] = useState('')
  const [parcelas, definirParcelas] = useState<ParcelaEmEdicao[]>([])
  const [tentouSalvar, definirTentouSalvar] = useState(false)

  useEffect(() => {
    if (aberto) {
      const base = prazo ?? modelo
      definirNome(base?.nome ?? '')
      definirParcelas(paraEdicao(base))
      definirTentouSalvar(false)
    }
  }, [aberto, prazo, modelo])

  const outrosNomes = nomesExistentes.filter((existente) => existente !== prazo?.nome)
  const erros = validarPrazo(nome, parcelas, outrosNomes)
  const valido = erros.length === 0
  const soma = somaDosPercentuais(parcelas.map((parcela) => lerNumero(parcela.percentual)))
  const somaFechada = Math.round(soma * 100) === 10_000
  const previa = valido ? gerarParcelas(VALOR_DE_EXEMPLO, hoje(), montarParcelas(parcelas)) : []

  function alterar(indice: number, campo: keyof ParcelaEmEdicao, valor: string) {
    definirParcelas((atuais) =>
      atuais.map((parcela, posicao) => (posicao === indice ? { ...parcela, [campo]: valor } : parcela)),
    )
  }

  // A nova parcela sugere 30 dias depois da última e o percentual que falta.
  function adicionarParcela() {
    definirParcelas((atuais) => {
      const ultimosDias = lerNumero(atuais.at(-1)?.dias ?? '')
      const falta = Math.max(0, Math.round((100 - somaDosPercentuais(atuais.map((p) => lerNumero(p.percentual)))) * 100) / 100)
      return [
        ...atuais,
        {
          dias: String(Number.isFinite(ultimosDias) ? ultimosDias + 30 : 30),
          percentual: falta > 0 ? String(falta).replace('.', ',') : '',
        },
      ]
    })
  }

  function removerParcela(indice: number) {
    definirParcelas((atuais) => atuais.filter((_, posicao) => posicao !== indice))
  }

  function distribuir() {
    const partes = distribuirIgualmente(parcelas.length)
    definirParcelas((atuais) =>
      atuais.map((parcela, indice) => ({ ...parcela, percentual: String(partes[indice] ?? 0).replace('.', ',') })),
    )
  }

  function salvar() {
    definirTentouSalvar(true)
    if (!valido) {
      return
    }
    aoSalvar({ nome: nome.trim(), parcelas: montarParcelas(parcelas) })
  }

  return (
    <Modal
      aberto={aberto}
      aoFechar={aoFechar}
      titulo={prazo ? `Editar prazo “${prazo.nome}”` : 'Novo prazo de pagamento'}
      descricao="Os dias contam a partir da data base do documento. A soma dos percentuais precisa fechar 100%."
      largura="media"
      rodape={
        <>
          <Botao variante="sutil" onClick={aoFechar}>
            Cancelar
          </Botao>
          <Botao variante="primario" onClick={salvar} disabled={tentouSalvar && !valido}>
            {prazo ? 'Salvar alterações' : 'Criar prazo'}
          </Botao>
        </>
      }
    >
      <form
        className="grid gap-5"
        noValidate
        onSubmit={(evento) => {
          evento.preventDefault()
          salvar()
        }}
      >
        <Campo
          rotulo="Nome"
          obrigatorio
          dica="Como aparece na escolha do prazo. Ex.: 30/60/90, Entrada + 30."
        >
          {(propriedades) => (
            <Entrada
              {...propriedades}
              value={nome}
              onChange={(evento) => definirNome(evento.target.value)}
              placeholder="Ex.: 30/60"
              autoFocus
            />
          )}
        </Campo>

        <div className="grid gap-2">
          <div className="flex flex-wrap items-center justify-between gap-2">
            <span className="text-[0.8125rem] font-medium text-ink-soft">Parcelas</span>
            <span
              aria-live="polite"
              className={cn(
                'inline-flex items-center gap-1.5 rounded-full border px-2 py-0.5 text-xs font-medium',
                somaFechada
                  ? 'border-positive/25 bg-positive-soft text-positive-ink'
                  : 'border-caution/30 bg-caution-soft text-caution-ink',
              )}
            >
              {somaFechada ? (
                <CheckCircle2 aria-hidden className="size-3" />
              ) : (
                <AlertCircle aria-hidden className="size-3" />
              )}
              <span className="numerico">
                Soma {formatarPercentual(soma)}
                {!somaFechada && soma < 100 && ` · faltam ${formatarPercentual(Math.round((100 - soma) * 100) / 100)}`}
                {!somaFechada && soma > 100 && ` · sobram ${formatarPercentual(Math.round((soma - 100) * 100) / 100)}`}
              </span>
            </span>
          </div>

          {/* Barra proporcional: mostra de relance como o valor se divide. */}
          <div aria-hidden className="flex h-2 overflow-hidden rounded-full bg-surface-3">
            {parcelas.map((parcela, indice) => {
              const percentual = lerNumero(parcela.percentual)
              return (
                <div
                  key={indice}
                  className={cn('h-full border-r border-surface last:border-r-0', indice % 2 === 0 ? 'bg-brand' : 'bg-brand/60')}
                  style={{ width: `${Number.isFinite(percentual) ? Math.min(Math.max(percentual, 0), 100) : 0}%` }}
                />
              )
            })}
          </div>

          <div className="overflow-hidden rounded-lg border border-line">
            <div className="grid grid-cols-[2.25rem_minmax(0,1fr)_minmax(0,1fr)_2.5rem] items-center gap-2 border-b border-line bg-surface-2/60 px-3 py-2 text-xs font-semibold tracking-wide text-ink-muted uppercase">
              <span>Nº</span>
              <span>Dias</span>
              <span>Percentual</span>
              <span className="sr-only">Remover</span>
            </div>
            <ul className="divide-y divide-line">
              {parcelas.map((parcela, indice) => (
                <li
                  key={indice}
                  className="grid grid-cols-[2.25rem_minmax(0,1fr)_minmax(0,1fr)_2.5rem] items-center gap-2 px-3 py-2"
                >
                  <span className="numerico text-sm font-medium text-ink-soft">{indice + 1}</span>
                  <Entrada
                    value={parcela.dias}
                    onChange={(evento) => alterar(indice, 'dias', evento.target.value.replace(/[^\d]/g, ''))}
                    inputMode="numeric"
                    aria-label={`Dias da parcela ${indice + 1}`}
                    sufixo={<span className="text-xs">dias</span>}
                    className="numerico"
                  />
                  <Entrada
                    value={parcela.percentual}
                    onChange={(evento) => alterar(indice, 'percentual', evento.target.value.replace(/[^\d.,]/g, ''))}
                    inputMode="decimal"
                    aria-label={`Percentual da parcela ${indice + 1}`}
                    sufixo={<span className="text-xs">%</span>}
                    className="numerico"
                  />
                  <Botao
                    variante="sutil"
                    tamanho="icone"
                    onClick={() => removerParcela(indice)}
                    disabled={parcelas.length === 1}
                    aria-label={`Remover parcela ${indice + 1}`}
                    className="hover:bg-negative-soft hover:text-negative-ink"
                  >
                    <Trash2 aria-hidden className="size-4" />
                  </Botao>
                </li>
              ))}
            </ul>
          </div>

          <div className="flex flex-wrap gap-2">
            <Botao
              variante="secundario"
              tamanho="pequeno"
              onClick={adicionarParcela}
              iconeInicial={<Plus aria-hidden className="size-3.5" />}
            >
              Adicionar parcela
            </Botao>
            <Botao
              variante="sutil"
              tamanho="pequeno"
              onClick={distribuir}
              disabled={parcelas.length === 0}
              iconeInicial={<Divide aria-hidden className="size-3.5" />}
            >
              Distribuir igualmente
            </Botao>
          </div>
        </div>

        {tentouSalvar && !valido && (
          <ul role="alert" className="grid gap-1 rounded-lg border border-negative/25 bg-negative-soft px-3 py-2.5">
            {erros.map((erro) => (
              <li key={erro} className="flex items-start gap-1.5 text-xs text-negative-ink">
                <AlertCircle aria-hidden className="mt-px size-3.5 shrink-0" />
                {erro}
              </li>
            ))}
          </ul>
        )}

        <div className="grid gap-2">
          <span className="text-[0.8125rem] font-medium text-ink-soft">
            Prévia com {formatarMoeda(VALOR_DE_EXEMPLO)} emitidos hoje
          </span>
          {previa.length === 0 ? (
            <p className="rounded-lg border border-dashed border-line-strong px-4 py-5 text-center text-xs text-ink-muted">
              A prévia aparece quando as parcelas estiverem válidas.
            </p>
          ) : (
            <ol className="grid gap-1.5 sm:grid-cols-2">
              {previa.map((parcela) => (
                <li
                  key={parcela.numero}
                  className="flex items-center justify-between gap-3 rounded-lg border border-line bg-surface-2/50 px-3 py-2 text-sm"
                >
                  <span className="text-ink-soft">
                    <span className="numerico font-medium text-ink">{parcela.numero}ª</span> ·{' '}
                    {parcela.dias === 0 ? 'à vista' : `${parcela.dias} dias`}
                    <span className="numerico ml-1 text-xs text-ink-muted">({formatarDiaMes(parcela.vencimento)})</span>
                  </span>
                  <span className="numerico font-semibold text-ink">{formatarMoeda(parcela.valor)}</span>
                </li>
              ))}
            </ol>
          )}
        </div>

        {prazo && (
          <p className="text-xs text-ink-muted">
            Alterar o prazo não muda títulos já gerados: cada título guarda o vencimento e o valor
            calculados no momento em que foi criado.
          </p>
        )}
      </form>
    </Modal>
  )
}
