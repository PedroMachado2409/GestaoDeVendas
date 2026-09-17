/**
 * A API responde falhas em application/problem+json (RFC 9457).
 * Erros de validação chegam como ValidationProblemDetails, com o dicionário
 * `errors` indexado pelo nome da propriedade em PascalCase — inclusive com
 * índice, no caso de coleções: "Itens[0].Quantidade".
 */
export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  instance?: string
  traceId?: string
  errors?: Record<string, string[]>
}

export class ErroApi extends Error {
  readonly status: number
  readonly titulo: string
  readonly detalhe: string
  readonly erros: Record<string, string[]>
  readonly traceId?: string

  constructor(status: number, problema: ProblemDetails = {}) {
    const detalhe = problema.detail?.trim()
    const titulo = problema.title?.trim() || tituloPadrao(status)
    super(detalhe || titulo)

    this.name = 'ErroApi'
    this.status = status
    this.titulo = titulo
    this.detalhe = detalhe || mensagemPadrao(status)
    this.erros = problema.errors ?? {}
    this.traceId = problema.traceId
  }

  /** Verdadeiro quando há erros por campo para devolver ao formulário. */
  get temErrosDeCampo() {
    return Object.keys(this.erros).length > 0
  }

  /** Junta todas as mensagens de validação em uma linha só. */
  get resumoDeValidacao() {
    return Object.values(this.erros).flat().join(' ')
  }

  /** Mensagem pronta para toast: prioriza validação, depois detalhe. */
  get mensagem() {
    if (this.temErrosDeCampo) {
      return this.resumoDeValidacao || this.detalhe
    }
    return this.detalhe
  }
}

/** Falha de rede, CORS ou API fora do ar — nunca chegou a haver resposta. */
export const STATUS_SEM_RESPOSTA = 0

function tituloPadrao(status: number) {
  switch (status) {
    case STATUS_SEM_RESPOSTA:
      return 'Sem comunicação com a API'
    case 400:
      return 'Requisição inválida'
    case 401:
      return 'Não autorizado'
    case 403:
      return 'Acesso negado'
    case 404:
      return 'Recurso não encontrado'
    case 409:
      return 'Conflito de dados'
    case 429:
      return 'Tentativas em excesso'
    default:
      return status >= 500 ? 'Erro interno no servidor' : 'Erro na aplicação'
  }
}

function mensagemPadrao(status: number) {
  switch (status) {
    case STATUS_SEM_RESPOSTA:
      return 'Não foi possível falar com a API. Verifique se ela está no ar e se a origem deste site está liberada em Cors:AllowedOrigins.'
    case 401:
      return 'Sua sessão expirou ou o token foi revogado. Entre novamente.'
    case 403:
      return 'Seu perfil não tem permissão para esta operação.'
    case 429:
      return 'Muitas tentativas em pouco tempo. Aguarde um minuto e tente de novo.'
    default:
      return status >= 500
        ? 'A API encontrou um erro inesperado. Tente novamente em instantes.'
        : 'Não foi possível concluir a operação.'
  }
}

export function mensagemDeErro(erro: unknown) {
  if (erro instanceof ErroApi) return erro.mensagem
  if (erro instanceof Error) return erro.message
  return 'Ocorreu um erro inesperado.'
}
