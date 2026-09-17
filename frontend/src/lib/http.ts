import { ErroApi, STATUS_SEM_RESPOSTA, type ProblemDetails } from './erros'

export const URL_API = (import.meta.env.VITE_API_URL ?? 'http://localhost:5143').replace(/\/+$/, '')

type Ouvinte = () => void

let tokenAtual: string | null = null
let aoPerderSessao: Ouvinte | null = null

/** O contexto de sessão injeta o token aqui; a camada HTTP não conhece React. */
export function definirToken(token: string | null) {
  tokenAtual = token
}

/** Chamado quando a API devolve 401 com um token em uso (revogado ou expirado). */
export function definirTratadorDeSessaoExpirada(ouvinte: Ouvinte | null) {
  aoPerderSessao = ouvinte
}

interface Opcoes {
  metodo?: 'GET' | 'POST' | 'PUT' | 'DELETE'
  corpo?: unknown
  /** Endpoints anônimos (autenticar, registrar, health) não enviam o token. */
  anonimo?: boolean
  sinal?: AbortSignal
}

export async function requisitar<T>(caminho: string, opcoes: Opcoes = {}): Promise<T> {
  const { metodo = 'GET', corpo, anonimo = false, sinal } = opcoes

  const cabecalhos = new Headers({ Accept: 'application/json' })
  if (corpo !== undefined) {
    cabecalhos.set('Content-Type', 'application/json')
  }
  if (!anonimo && tokenAtual) {
    cabecalhos.set('Authorization', `Bearer ${tokenAtual}`)
  }

  let resposta: Response
  try {
    resposta = await fetch(`${URL_API}${caminho}`, {
      method: metodo,
      headers: cabecalhos,
      body: corpo === undefined ? undefined : JSON.stringify(corpo),
      signal: sinal,
    })
  } catch (causa) {
    if (causa instanceof DOMException && causa.name === 'AbortError') {
      throw causa
    }
    throw new ErroApi(STATUS_SEM_RESPOSTA)
  }

  if (!resposta.ok) {
    const problema = await lerProblema(resposta)

    // 401 em requisição autenticada significa sessão morta: derruba a sessão
    // local antes de propagar, para a interface não insistir com o token velho.
    if (resposta.status === 401 && !anonimo && tokenAtual) {
      aoPerderSessao?.()
    }

    throw new ErroApi(resposta.status, problema)
  }

  if (resposta.status === 204 || resposta.headers.get('Content-Length') === '0') {
    return undefined as T
  }

  const tipo = resposta.headers.get('Content-Type') ?? ''
  if (!tipo.includes('json')) {
    return (await resposta.text()) as T
  }

  return (await resposta.json()) as T
}

async function lerProblema(resposta: Response): Promise<ProblemDetails> {
  const tipo = resposta.headers.get('Content-Type') ?? ''

  try {
    if (tipo.includes('json')) {
      return (await resposta.json()) as ProblemDetails
    }
    const texto = (await resposta.text()).trim()
    return texto ? { detail: texto } : {}
  } catch {
    return {}
  }
}

export const http = {
  get: <T>(caminho: string, opcoes?: Omit<Opcoes, 'metodo' | 'corpo'>) =>
    requisitar<T>(caminho, { ...opcoes, metodo: 'GET' }),
  post: <T>(caminho: string, corpo?: unknown, opcoes?: Omit<Opcoes, 'metodo' | 'corpo'>) =>
    requisitar<T>(caminho, { ...opcoes, metodo: 'POST', corpo }),
  put: <T>(caminho: string, corpo?: unknown, opcoes?: Omit<Opcoes, 'metodo' | 'corpo'>) =>
    requisitar<T>(caminho, { ...opcoes, metodo: 'PUT', corpo }),
}
