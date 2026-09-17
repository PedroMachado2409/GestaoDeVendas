import { clsx, type ClassValue } from 'clsx'
import { twMerge } from 'tailwind-merge'

/** Junta classes resolvendo conflitos de utilitários do Tailwind. */
export function cn(...entradas: ClassValue[]) {
  return twMerge(clsx(entradas))
}

const moeda = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL',
})

const moedaCompacta = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL',
  notation: 'compact',
  maximumFractionDigits: 1,
})

const inteiro = new Intl.NumberFormat('pt-BR')

const dataCurta = new Intl.DateTimeFormat('pt-BR', {
  day: '2-digit',
  month: '2-digit',
  year: 'numeric',
})

const dataHora = new Intl.DateTimeFormat('pt-BR', {
  day: '2-digit',
  month: '2-digit',
  year: 'numeric',
  hour: '2-digit',
  minute: '2-digit',
})

const mesAno = new Intl.DateTimeFormat('pt-BR', { month: 'short', year: '2-digit' })

export function formatarMoeda(valor: number) {
  return moeda.format(valor)
}

/** Usa notação compacta a partir de 100 mil para não estourar o cartão. */
export function formatarMoedaCompacta(valor: number) {
  return Math.abs(valor) >= 100_000 ? moedaCompacta.format(valor) : moeda.format(valor)
}

export function formatarNumero(valor: number) {
  return inteiro.format(valor)
}

export function formatarData(valor: string | Date) {
  return dataCurta.format(new Date(valor))
}

export function formatarDataHora(valor: string | Date) {
  return dataHora.format(new Date(valor))
}

export function formatarMesAno(valor: Date) {
  return mesAno.format(valor).replace('.', '')
}

/** "há 3 dias", "agora" — para carimbos de atualização. */
export function formatarTempoRelativo(valor: string | Date) {
  const alvo = new Date(valor).getTime()
  const segundos = Math.round((alvo - Date.now()) / 1000)
  const escalas: Array<[Intl.RelativeTimeFormatUnit, number]> = [
    ['second', 60],
    ['minute', 60],
    ['hour', 24],
    ['day', 30],
    ['month', 12],
    ['year', Number.POSITIVE_INFINITY],
  ]

  const formatador = new Intl.RelativeTimeFormat('pt-BR', { numeric: 'auto' })
  let quantidade = segundos

  for (const [unidade, limite] of escalas) {
    if (Math.abs(quantidade) < limite) {
      return formatador.format(Math.round(quantidade), unidade)
    }
    quantidade /= limite
  }

  return formatador.format(Math.round(quantidade), 'year')
}

export function somenteDigitos(valor: string) {
  return valor.replace(/\D/g, '')
}

export function formatarCpf(valor: string) {
  const digitos = somenteDigitos(valor).slice(0, 11)
  return digitos
    .replace(/^(\d{3})(\d)/, '$1.$2')
    .replace(/^(\d{3})\.(\d{3})(\d)/, '$1.$2.$3')
    .replace(/\.(\d{3})(\d{1,2})$/, '.$1-$2')
}

export function iniciais(nome: string) {
  const partes = nome.trim().split(/\s+/).filter(Boolean)
  if (partes.length === 0) return '?'
  const primeira = partes[0]?.[0] ?? ''
  const ultima = partes.length > 1 ? (partes[partes.length - 1]?.[0] ?? '') : ''
  return (primeira + ultima).toUpperCase()
}

/** Remove acentos e caixa para busca tolerante. */
export function normalizarTexto(valor: string) {
  return valor
    .normalize('NFD')
    .replace(/[̀-ͯ]/g, '')
    .toLowerCase()
    .trim()
}

export function pluralizar(quantidade: number, singular: string, plural: string) {
  return quantidade === 1 ? singular : plural
}

/** Ordena preservando estabilidade e tratando string com locale pt-BR. */
export function comparar(a: unknown, b: unknown): number {
  if (typeof a === 'string' && typeof b === 'string') {
    return a.localeCompare(b, 'pt-BR', { sensitivity: 'base' })
  }
  if (typeof a === 'boolean' && typeof b === 'boolean') {
    return Number(a) - Number(b)
  }
  return Number(a) - Number(b)
}
