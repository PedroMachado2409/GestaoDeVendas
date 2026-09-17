import { useQuery, useQueryClient } from '@tanstack/react-query'
import {
  createContext,
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
  type ReactNode,
} from 'react'

import { usuariosApi } from '@/api/recursos'
import type { LoginRequest, Papel, Usuario } from '@/api/tipos'
import { definirToken, definirTratadorDeSessaoExpirada } from '@/lib/http'
import { lerToken, tokenExpirado } from '@/lib/jwt'

const CHAVE_TOKEN = 'gp.token'

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

function lerTokenSalvo() {
  const token = localStorage.getItem(CHAVE_TOKEN)
  if (!token) return null
  if (tokenExpirado(token)) {
    localStorage.removeItem(CHAVE_TOKEN)
    return null
  }
  return token
}

export function ProvedorDeSessao({ children }: { children: ReactNode }) {
  const clienteDeConsultas = useQueryClient()
  const [token, definirTokenNoEstado] = useState<string | null>(() => {
    const salvo = lerTokenSalvo()
    definirToken(salvo)
    return salvo
  })
  const [motivoDaUltimaSaida, definirMotivo] = useState<MotivoSaida | null>(null)
  const temporizador = useRef<number | null>(null)

  const sair = useCallback(
    (motivo: MotivoSaida = 'manual') => {
      localStorage.removeItem(CHAVE_TOKEN)
      definirToken(null)
      definirTokenNoEstado(null)
      definirMotivo(motivo)
      clienteDeConsultas.clear()
    },
    [clienteDeConsultas],
  )

  // Um 401 vindo de qualquer requisição derruba a sessão local.
  useEffect(() => {
    definirTratadorDeSessaoExpirada(() => sair('expirada'))
    return () => definirTratadorDeSessaoExpirada(null)
  }, [sair])

  // Encerra sozinha no instante em que o token expira, sem esperar o próximo
  // 401 — a API usa ClockSkew zero, então o corte é exato.
  useEffect(() => {
    if (temporizador.current) {
      window.clearTimeout(temporizador.current)
      temporizador.current = null
    }
    if (!token) return

    const expiraEm = lerToken(token)?.expiraEm
    if (!expiraEm) return

    const restante = expiraEm.getTime() - Date.now()
    if (restante <= 0) {
      sair('expirada')
      return
    }

    // setTimeout satura acima de ~24 dias; a expiração aqui é de minutos.
    temporizador.current = window.setTimeout(() => sair('expirada'), restante)
    return () => {
      if (temporizador.current) window.clearTimeout(temporizador.current)
    }
  }, [token, sair])

  const consultaUsuario = useQuery({
    queryKey: ['usuario-autenticado', token],
    queryFn: ({ signal }) => usuariosApi.autenticado(signal),
    enabled: Boolean(token),
    staleTime: 5 * 60 * 1000,
    retry: false,
  })

  const entrar = useCallback(
    async (credenciais: LoginRequest) => {
      const resposta = await usuariosApi.autenticar(credenciais)
      localStorage.setItem(CHAVE_TOKEN, resposta.token)
      definirToken(resposta.token)
      definirMotivo(null)
      definirTokenNoEstado(resposta.token)
      await clienteDeConsultas.invalidateQueries({ queryKey: ['usuario-autenticado'] })
    },
    [clienteDeConsultas],
  )

  const valor = useMemo<Sessao>(() => {
    const usuario = consultaUsuario.data ?? null
    return {
      token,
      usuario,
      carregando: Boolean(token) && consultaUsuario.isPending,
      autenticado: Boolean(token) && Boolean(usuario),
      ehAdmin: usuario?.role === ('Admin' satisfies Papel),
      entrar,
      sair,
      motivoDaUltimaSaida,
      limparMotivoDeSaida: () => definirMotivo(null),
    }
  }, [token, consultaUsuario.data, consultaUsuario.isPending, entrar, sair, motivoDaUltimaSaida])

  return <ContextoSessao.Provider value={valor}>{children}</ContextoSessao.Provider>
}
