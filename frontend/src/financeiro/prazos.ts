import { adicionarDias } from './datas'
import type { DataISO, ParcelaDoPrazo, PrazoPagamento } from './tipos'

/**
 * Regras do prazo de pagamento. Espelham as da entidade PrazoPagamento
 * combinada para a API; enquanto ela não existe, são estas que valem na tela.
 *
 * Dinheiro e percentual são tratados em inteiros (centavos e centésimos de
 * ponto percentual): em ponto flutuante, 33,33 + 33,33 + 33,34 não dá 100.
 */

export interface ParcelaEmEdicao {
  dias: string
  percentual: string
}

export interface ParcelaGerada {
  numero: number
  dias: number
  percentual: number
  vencimento: DataISO
  valor: number
}

const CENTESIMOS_EM_100 = 10_000

function emCentesimos(percentual: number) {
  return Math.round(percentual * 100)
}

/** Aceita "33,33" e "33.33". */
export function lerNumero(texto: string) {
  const normalizado = texto.trim().replace(',', '.')
  if (normalizado === '') {
    return Number.NaN
  }
  return Number(normalizado)
}

export function somaDosPercentuais(percentuais: number[]) {
  const total = percentuais.reduce(
    (soma, percentual) => soma + (Number.isFinite(percentual) ? emCentesimos(percentual) : 0),
    0,
  )
  return total / 100
}

/**
 * Valida o conjunto inteiro. A soma dos percentuais só existe olhando todas
 * as parcelas juntas, então não há validação de parcela isolada.
 */
export function validarPrazo(
  nome: string,
  parcelas: ParcelaEmEdicao[],
  nomesExistentes: string[] = [],
): string[] {
  const erros: string[] = []
  const nomeLimpo = nome.trim()

  if (!nomeLimpo) {
    erros.push('Informe o nome do prazo.')
  } else if (nomesExistentes.some((existente) => existente.trim().toLowerCase() === nomeLimpo.toLowerCase())) {
    erros.push('Já existe um prazo com este nome.')
  }

  if (parcelas.length === 0) {
    erros.push('O prazo deve ter ao menos uma parcela.')
    return erros
  }

  const dias = parcelas.map((parcela) => lerNumero(parcela.dias))
  const percentuais = parcelas.map((parcela) => lerNumero(parcela.percentual))

  if (dias.some((valor) => !Number.isInteger(valor) || valor < 0)) {
    erros.push('Os dias devem ser números inteiros a partir de 0.')
  }

  if (percentuais.some((valor) => !Number.isFinite(valor) || valor <= 0)) {
    erros.push('Todo percentual deve ser maior que zero.')
  }

  if (percentuais.some((valor) => Number.isFinite(valor) && Math.abs(valor * 100 - emCentesimos(valor)) > 1e-6)) {
    erros.push('Use no máximo duas casas decimais no percentual.')
  }

  for (let indice = 1; indice < dias.length; indice++) {
    const atual = dias[indice] ?? Number.NaN
    const anterior = dias[indice - 1] ?? Number.NaN
    if (Number.isFinite(atual) && Number.isFinite(anterior) && atual <= anterior) {
      erros.push('Os dias das parcelas devem ser crescentes, sem repetição.')
      break
    }
  }

  const totalEmCentesimos = percentuais.reduce(
    (soma, valor) => soma + (Number.isFinite(valor) ? emCentesimos(valor) : 0),
    0,
  )
  if (totalEmCentesimos !== CENTESIMOS_EM_100) {
    erros.push(`A soma dos percentuais deve ser 100% (hoje: ${formatarPercentual(totalEmCentesimos / 100)}).`)
  }

  return erros
}

/** Converte o formulário em parcelas numeradas. Só chame depois de validar. */
export function montarParcelas(parcelas: ParcelaEmEdicao[]): ParcelaDoPrazo[] {
  return parcelas.map((parcela, indice) => ({
    numero: indice + 1,
    dias: lerNumero(parcela.dias),
    percentual: lerNumero(parcela.percentual),
  }))
}

/**
 * Divide o valor pelas parcelas do prazo. Os centavos que sobram do
 * arredondamento vão para a última parcela, para a soma bater exatamente
 * com o total: R$ 100,00 em 33,33/33,33/33,34 dá 33,33 + 33,33 + 33,34.
 */
export function gerarParcelas(valorTotal: number, dataBase: DataISO, parcelas: ParcelaDoPrazo[]): ParcelaGerada[] {
  const totalEmCentavos = Math.round(valorTotal * 100)
  let acumulado = 0

  return parcelas.map((parcela, indice) => {
    const ultima = indice === parcelas.length - 1
    const centavos = ultima
      ? totalEmCentavos - acumulado
      : Math.round((totalEmCentavos * emCentesimos(parcela.percentual)) / CENTESIMOS_EM_100)

    acumulado += centavos

    return {
      numero: parcela.numero,
      dias: parcela.dias,
      percentual: parcela.percentual,
      vencimento: adicionarDias(dataBase, parcela.dias),
      valor: centavos / 100,
    }
  })
}

/**
 * Divide 100% em partes iguais, com o resto na última: 3 parcelas dão
 * 33,33 / 33,33 / 33,34.
 */
export function distribuirIgualmente(quantidade: number): number[] {
  if (quantidade <= 0) {
    return []
  }
  const parte = Math.floor(CENTESIMOS_EM_100 / quantidade)
  return Array.from({ length: quantidade }, (_, indice) =>
    indice === quantidade - 1 ? (CENTESIMOS_EM_100 - parte * (quantidade - 1)) / 100 : parte / 100,
  )
}

const formatoPercentual = new Intl.NumberFormat('pt-BR', { maximumFractionDigits: 2 })

export function formatarPercentual(valor: number) {
  return `${formatoPercentual.format(valor)}%`
}

/** "30 d · 50%  →  60 d · 50%" em forma curta: "30/60". */
export function resumirDias(prazo: Pick<PrazoPagamento, 'parcelas'>) {
  return prazo.parcelas.map((parcela) => (parcela.dias === 0 ? 'à vista' : `${parcela.dias}d`)).join(' / ')
}

export function descreverParcelas(prazo: Pick<PrazoPagamento, 'parcelas'>) {
  return prazo.parcelas
    .map((parcela) => `${parcela.dias === 0 ? 'à vista' : `${parcela.dias} dias`}: ${formatarPercentual(parcela.percentual)}`)
    .join(' · ')
}
