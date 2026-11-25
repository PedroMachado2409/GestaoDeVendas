import { createContext } from 'react'
import type { LoginRequest, Usuario } from '../api/tipos'

export type MotivoSaida = 'manual' | 'expirada'

export interface Sessao {
  token: string | null
  usuario: Usuario | null
  carregando: boolean
  autenticado: boolean
  ehAdmin: boolean
  entrar: (credenciais: LoginRequest) => Promise<void>
  sair: (motivo?: MotivoSaida) => void
  motivoDaUltimaSaida: MotivoSaida | null
  limparMotivoDeSaida: () => void
}

export const ContextoSessao = createContext<Sessao | null>(null)
