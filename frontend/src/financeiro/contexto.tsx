import { createContext, useCallback, useMemo, useState, type ReactNode } from 'react'

import { gerarTitulosFicticios, prazosFicticios } from './dadosFicticios'
import type { Baixa, PrazoPagamento, TituloFinanceiro } from './tipos'

export type NovoTitulo = Omit<TituloFinanceiro, 'id'>
export type DadosDaBaixa = Pick<Baixa, 'valor' | 'data' | 'forma'>

export interface Financeiro {
  titulos: TituloFinanceiro[]
  prazos: PrazoPagamento[]
  /** Verdadeiro enquanto o financeiro usa dados fictícios, sem API. */
  ficticio: boolean
  lancar: (novos: NovoTitulo[]) => void
  baixar: (tituloId: number, dados: DadosDaBaixa) => void
  cancelar: (tituloId: number) => void
  salvarPrazo: (prazoId: number | null, dados: Pick<PrazoPagamento, 'nome' | 'parcelas'>) => void
  alternarPrazo: (prazoId: number) => void
}

export const ContextoFinanceiro = createContext<Financeiro | null>(null)

/**
 * Estado do financeiro compartilhado entre a tela do Financeiro e o Painel:
 * uma baixa registrada em uma aparece na outra sem recarregar.
 *
 * Fica montado dentro da área logada (Shell), então sobrevive à navegação
 * entre telas e some ao sair ou recarregar — o que é o esperado enquanto os
 * dados são fictícios. Quando a API existir, este provedor dá lugar a
 * consultas do react-query, e as telas continuam usando o mesmo hook.
 */
export function ProvedorFinanceiro({ usuarioNome, children }: { usuarioNome: string; children: ReactNode }) {
  const [titulos, definirTitulos] = useState(() => gerarTitulosFicticios(usuarioNome))
  const [prazos, definirPrazos] = useState<PrazoPagamento[]>(prazosFicticios)

  const lancar = useCallback((novos: NovoTitulo[]) => {
    definirTitulos((atuais) => {
      let proximoId = Math.max(0, ...atuais.map((titulo) => titulo.id)) + 1
      return [...atuais, ...novos.map((novo) => ({ ...novo, id: proximoId++ }))]
    })
  }, [])

  const baixar = useCallback(
    (tituloId: number, dados: DadosDaBaixa) => {
      definirTitulos((atuais) => {
        const proximaBaixa = Math.max(0, ...atuais.flatMap((titulo) => titulo.baixas.map((baixa) => baixa.id))) + 1
        return atuais.map((titulo) => {
          if (titulo.id !== tituloId) {
            return titulo
          }
          const valorBaixado = Math.round((titulo.valorBaixado + dados.valor) * 100) / 100
          const quitado = Math.round(valorBaixado * 100) >= Math.round(titulo.valorOriginal * 100)
          return {
            ...titulo,
            valorBaixado,
            status: quitado ? 'Baixado' : 'ParcialmenteBaixado',
            baixas: [...titulo.baixas, { id: proximaBaixa, usuarioNome, ...dados }],
          }
        })
      })
    },
    [usuarioNome],
  )

  const cancelar = useCallback((tituloId: number) => {
    definirTitulos((atuais) =>
      atuais.map((titulo) => (titulo.id === tituloId ? { ...titulo, status: 'Cancelado' } : titulo)),
    )
  }, [])

  const salvarPrazo = useCallback((prazoId: number | null, dados: Pick<PrazoPagamento, 'nome' | 'parcelas'>) => {
    definirPrazos((atuais) => {
      if (prazoId !== null) {
        return atuais.map((prazo) => (prazo.id === prazoId ? { ...prazo, ...dados } : prazo))
      }
      return [...atuais, { id: Math.max(0, ...atuais.map((prazo) => prazo.id)) + 1, ativo: true, ...dados }]
    })
  }, [])

  const alternarPrazo = useCallback((prazoId: number) => {
    definirPrazos((atuais) => atuais.map((prazo) => (prazo.id === prazoId ? { ...prazo, ativo: !prazo.ativo } : prazo)))
  }, [])

  const valor = useMemo<Financeiro>(
    () => ({ titulos, prazos, ficticio: true, lancar, baixar, cancelar, salvarPrazo, alternarPrazo }),
    [titulos, prazos, lancar, baixar, cancelar, salvarPrazo, alternarPrazo],
  )

  return <ContextoFinanceiro.Provider value={valor}>{children}</ContextoFinanceiro.Provider>
}
