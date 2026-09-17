export interface ConteudoToken {
  expiraEm: Date | null
  papel: string | null
  email: string | null
  nome: string | null
}

/**
 * Lê o payload do JWT apenas para saber quando ele expira e adiantar o
 * encerramento da sessão. A autoridade sobre identidade e papel continua
 * sendo GET /api/Usuario/autenticado — nada aqui é usado como decisão de
 * segurança, só de experiência de uso.
 */
export function lerToken(token: string): ConteudoToken | null {
  const partes = token.split('.')
  if (partes.length !== 3) return null

  try {
    const carga = JSON.parse(decodificarBase64Url(partes[1] ?? '')) as Record<string, unknown>
    const exp = typeof carga.exp === 'number' ? new Date(carga.exp * 1000) : null

    return {
      expiraEm: exp,
      papel: textoDaClaim(carga, 'role'),
      email: textoDaClaim(carga, 'emailaddress'),
      nome: textoDaClaim(carga, 'name'),
    }
  } catch {
    return null
  }
}

export function tokenExpirado(token: string) {
  const conteudo = lerToken(token)
  if (!conteudo?.expiraEm) return false
  return conteudo.expiraEm.getTime() <= Date.now()
}

/**
 * As claims saem do ASP.NET com URI completa
 * (schemas.microsoft.com/ws/2008/06/identity/claims/role, por exemplo);
 * o sufixo é o suficiente para localizá-las.
 */
function textoDaClaim(carga: Record<string, unknown>, sufixo: string) {
  const chave = Object.keys(carga).find((nome) => nome === sufixo || nome.endsWith(`/${sufixo}`))
  const valor = chave ? carga[chave] : undefined
  if (typeof valor === 'string') return valor
  if (Array.isArray(valor) && typeof valor[0] === 'string') return valor[0]
  return null
}

function decodificarBase64Url(valor: string) {
  const base64 = valor.replace(/-/g, '+').replace(/_/g, '/')
  const preenchido = base64.padEnd(base64.length + ((4 - (base64.length % 4)) % 4), '=')
  const binario = atob(preenchido)
  const bytes = Uint8Array.from(binario, (caractere) => caractere.charCodeAt(0))
  return new TextDecoder().decode(bytes)
}
