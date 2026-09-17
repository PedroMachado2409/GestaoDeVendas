import type { DataISO } from './tipos'

/**
 * Vencimento é data de calendário, sem hora. `new Date('2026-09-17')` lê a
 * string como meia-noite UTC e, no Brasil, mostra o dia anterior — por isso
 * estas funções montam a data sempre no fuso local.
 */

function paraData(iso: DataISO) {
  const [ano, mes, dia] = iso.split('-').map(Number)
  return new Date(ano ?? 1970, (mes ?? 1) - 1, dia ?? 1)
}

function paraISO(data: Date): DataISO {
  const mes = String(data.getMonth() + 1).padStart(2, '0')
  const dia = String(data.getDate()).padStart(2, '0')
  return `${data.getFullYear()}-${mes}-${dia}`
}

export function hoje(): DataISO {
  return paraISO(new Date())
}

export function adicionarDias(iso: DataISO, dias: number): DataISO {
  const data = paraData(iso)
  data.setDate(data.getDate() + dias)
  return paraISO(data)
}

/** Positivo quando `ate` vem depois de `de`. */
export function diasEntre(de: DataISO, ate: DataISO) {
  const milissegundosPorDia = 86_400_000
  return Math.round((paraData(ate).getTime() - paraData(de).getTime()) / milissegundosPorDia)
}

const formatoCurto = new Intl.DateTimeFormat('pt-BR', { day: '2-digit', month: '2-digit', year: 'numeric' })
const formatoDiaMes = new Intl.DateTimeFormat('pt-BR', { day: '2-digit', month: 'short' })

export function formatarDataISO(iso: DataISO) {
  return formatoCurto.format(paraData(iso))
}

/** "23 set" — sem o "de" e o ponto que o pt-BR põe no mês abreviado. */
export function formatarDiaMes(iso: DataISO) {
  const partes = formatoDiaMes.formatToParts(paraData(iso))
  const dia = partes.find((parte) => parte.type === 'day')?.value ?? ''
  const mes = (partes.find((parte) => parte.type === 'month')?.value ?? '').replace('.', '')
  return `${dia} ${mes}`
}

/** "vence hoje", "vence em 3 dias", "venceu há 5 dias". */
export function descreverVencimento(vencimento: DataISO, referencia: DataISO = hoje()) {
  const diferenca = diasEntre(referencia, vencimento)
  if (diferenca === 0) {
    return 'vence hoje'
  }
  if (diferenca === 1) {
    return 'vence amanhã'
  }
  if (diferenca > 0) {
    return `vence em ${diferenca} dias`
  }
  if (diferenca === -1) {
    return 'venceu ontem'
  }
  return `venceu há ${Math.abs(diferenca)} dias`
}
