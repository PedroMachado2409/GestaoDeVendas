import { adicionarDias, diasEntre, formatarDiaMes, hoje } from './datas'
import { estaEmAberto, estaVencido, saldoDo } from './titulos'
import type { DataISO, TipoTitulo, TituloFinanceiro } from './tipos'

/** Números do financeiro usados pelo Painel. Tudo sobre o saldo em aberto. */
export interface PosicaoFinanceira {
  aReceber: number
  aPagar: number
  /** A receber menos a pagar, considerando tudo o que está em aberto. */
  saldoPrevisto: number
  vencidoReceber: number
  vencidoPagar: number
  quantidadeVencidaReceber: number
  quantidadeVencidaPagar: number
}

export function calcularPosicao(titulos: TituloFinanceiro[], referencia: DataISO = hoje()): PosicaoFinanceira {
  const soma = (tipo: TipoTitulo, filtro: (titulo: TituloFinanceiro) => boolean) =>
    titulos
      .filter((titulo) => titulo.tipo === tipo && filtro(titulo))
      .reduce((total, titulo) => total + saldoDo(titulo), 0)

  const vencidos = (tipo: TipoTitulo) =>
    titulos.filter((titulo) => titulo.tipo === tipo && estaVencido(titulo, referencia))

  const aReceber = soma('Receber', estaEmAberto)
  const aPagar = soma('Pagar', estaEmAberto)

  return {
    aReceber,
    aPagar,
    saldoPrevisto: aReceber - aPagar,
    vencidoReceber: soma('Receber', (titulo) => estaVencido(titulo, referencia)),
    vencidoPagar: soma('Pagar', (titulo) => estaVencido(titulo, referencia)),
    quantidadeVencidaReceber: vencidos('Receber').length,
    quantidadeVencidaPagar: vencidos('Pagar').length,
  }
}

export interface PeriodoDoFluxo {
  chave: string
  rotulo: string
  rotuloLongo: string
  entradas: number
  saidas: number
  /** O primeiro período junta o que já venceu e não foi baixado. */
  atrasado: boolean
}

/**
 * Saldo em aberto distribuído por semana de vencimento. O que venceu e não
 * foi baixado entra num período próprio, "Atrasado", antes da semana atual:
 * somar ao presente esconderia o atraso; descartar esconderia o dinheiro.
 */
export function fluxoPorSemana(
  titulos: TituloFinanceiro[],
  semanas = 6,
  referencia: DataISO = hoje(),
): PeriodoDoFluxo[] {
  const abertos = titulos.filter(estaEmAberto)

  const atrasado: PeriodoDoFluxo = {
    chave: 'atrasado',
    rotulo: 'Atraso',
    rotuloLongo: 'Vencido e ainda em aberto',
    entradas: 0,
    saidas: 0,
    atrasado: true,
  }

  const periodos: PeriodoDoFluxo[] = Array.from({ length: semanas }, (_, indice) => {
    const inicio = adicionarDias(referencia, indice * 7)
    const fim = adicionarDias(inicio, 6)
    return {
      chave: inicio,
      // Rótulo do eixo sempre curto (cabe no celular); o contexto vai no tooltip.
      rotulo: formatarDiaMes(inicio),
      rotuloLongo: `${indice === 0 ? 'Esta semana: ' : ''}${formatarDiaMes(inicio)} a ${formatarDiaMes(fim)}`,
      entradas: 0,
      saidas: 0,
      atrasado: false,
    }
  })

  for (const titulo of abertos) {
    const dias = diasEntre(referencia, titulo.dataVencimento)
    const destino = dias < 0 ? atrasado : periodos[Math.floor(dias / 7)]
    if (!destino) {
      continue
    }
    if (titulo.tipo === 'Receber') {
      destino.entradas += saldoDo(titulo)
    } else {
      destino.saidas += saldoDo(titulo)
    }
  }

  return [atrasado, ...periodos]
}

/** Títulos em aberto ordenados por vencimento: os atrasados vêm primeiro. */
export function proximosVencimentos(titulos: TituloFinanceiro[], limite = 6) {
  return titulos
    .filter(estaEmAberto)
    .sort((a, b) => a.dataVencimento.localeCompare(b.dataVencimento) || a.id - b.id)
    .slice(0, limite)
}
