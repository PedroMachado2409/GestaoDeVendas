import { useContext } from 'react'

import { ContextoFinanceiro } from './contextoFinanceiro'

export function useFinanceiro() {
  const contexto = useContext(ContextoFinanceiro)
  if (!contexto) {
    throw new Error('useFinanceiro precisa estar dentro de <ProvedorFinanceiro>.')
  }
  return contexto
}
