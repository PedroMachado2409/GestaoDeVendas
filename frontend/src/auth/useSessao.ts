import { useContext } from 'react'

import { ContextoSessao } from './contexto'

export function useSessao() {
  const sessao = useContext(ContextoSessao)
  if (!sessao) {
    throw new Error('useSessao precisa estar dentro de <ProvedorDeSessao>.')
  }
  return sessao
}
