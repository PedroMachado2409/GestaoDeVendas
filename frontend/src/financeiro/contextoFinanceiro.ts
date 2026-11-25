import { createContext } from 'react'
import type { Baixa, PrazoPagamento, TituloFinanceiro } from './tipos'

export type NovoTitulo = Omit<TituloFinanceiro, 'id'>
export type DadosDaBaixa = Pick<Baixa, 'valor' | 'data' | 'forma'>

export interface Financeiro {
  titulos: TituloFinanceiro[]
  prazos: PrazoPagamento[]
  ficticio: boolean
  lancar: (novos: NovoTitulo[]) => void
  baixar: (tituloId: number, dados: DadosDaBaixa) => void
  cancelar: (tituloId: number) => void
  salvarPrazo: (prazoId: number | null, dados: Pick<PrazoPagamento, 'nome' | 'parcelas'>) => void
  alternarPrazo: (prazoId: number) => void
}

export const ContextoFinanceiro = createContext<Financeiro | null>(null)
