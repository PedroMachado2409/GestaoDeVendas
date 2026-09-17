import type { FieldValues, Path, UseFormSetError } from 'react-hook-form'

import { ErroApi } from './erros'
import { notificarErro } from './notificacoes'

/**
 * Devolve os erros do ValidationProblemDetails para os campos do formulário.
 *
 * A API nomeia as propriedades em PascalCase ("Nome", "Cpf") e, em coleções,
 * com índice ("Itens[0].Quantidade"). Aqui a chave é normalizada para o
 * caminho que o react-hook-form usa ("nome", "itens.0.quantidade").
 *
 * Retorna as mensagens que não puderam ser associadas a nenhum campo, para
 * a tela decidir onde mostrá-las.
 */
export function aplicarErrosDaApi<T extends FieldValues>(
  erro: unknown,
  definirErro: UseFormSetError<T>,
  camposConhecidos: ReadonlyArray<Path<T>>,
): string[] {
  if (!(erro instanceof ErroApi) || !erro.temErrosDeCampo) {
    return []
  }

  const orfaos: string[] = []
  const indice = new Map(camposConhecidos.map((campo) => [campo.toLowerCase(), campo]))

  for (const [propriedade, mensagens] of Object.entries(erro.erros)) {
    const mensagem = mensagens.join(' ')
    const caminho = normalizarCaminho(propriedade)
    const campo = indice.get(caminho.toLowerCase())

    if (campo) {
      definirErro(campo, { type: 'server', message: mensagem })
    } else {
      orfaos.push(mensagem)
    }
  }

  return orfaos
}

/**
 * Caminho comum de falha ao salvar: o que der para mostrar no campo vai para
 * o campo; o resto vira toast. Evita erro de validação que some da tela.
 */
export function tratarErroDeFormulario<T extends FieldValues>(
  erro: unknown,
  definirErro: UseFormSetError<T>,
  camposConhecidos: ReadonlyArray<Path<T>>,
  titulo = 'Não foi possível salvar',
) {
  const orfaos = aplicarErrosDaApi(erro, definirErro, camposConhecidos)
  const totalmenteTratado =
    erro instanceof ErroApi && erro.temErrosDeCampo && orfaos.length === 0

  if (!totalmenteTratado) {
    notificarErro(erro, titulo)
  }
}

function normalizarCaminho(propriedade: string) {
  return propriedade
    .replace(/\[(\d+)\]/g, '.$1')
    .split('.')
    .map((parte, indice) =>
      indice === 0 || Number.isNaN(Number(parte)) ? descapitalizar(parte) : parte,
    )
    .join('.')
}

function descapitalizar(valor: string) {
  return valor.charAt(0).toLowerCase() + valor.slice(1)
}
