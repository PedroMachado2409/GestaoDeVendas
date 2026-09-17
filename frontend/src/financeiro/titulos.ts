import type { TomEtiqueta } from '@/componentes/ui/Etiqueta'

import { diasEntre, hoje } from './datas'
import type { FormaPagamento, OrigemTitulo, StatusTitulo, TipoTitulo, TituloFinanceiro } from './tipos'

/**
 * "Vencido" não é status gravado: é o título em aberto com vencimento no
 * passado. Gravado, ficaria desatualizado no dia seguinte.
 */
export type Situacao = StatusTitulo | 'Vencido'

export function saldoDo(titulo: TituloFinanceiro) {
  return Math.round((titulo.valorOriginal - titulo.valorBaixado) * 100) / 100
}

export function estaEmAberto(titulo: TituloFinanceiro) {
  return titulo.status === 'Aberto' || titulo.status === 'ParcialmenteBaixado'
}

export function estaVencido(titulo: TituloFinanceiro, referencia = hoje()) {
  return estaEmAberto(titulo) && diasEntre(referencia, titulo.dataVencimento) < 0
}

export function situacaoDo(titulo: TituloFinanceiro, referencia = hoje()): Situacao {
  return estaVencido(titulo, referencia) ? 'Vencido' : titulo.status
}

export const rotulosDeSituacao: Record<Situacao, string> = {
  Aberto: 'Em aberto',
  ParcialmenteBaixado: 'Parcial',
  Baixado: 'Baixado',
  Cancelado: 'Cancelado',
  Vencido: 'Vencido',
}

export function rotuloDeBaixado(tipo: TipoTitulo) {
  return tipo === 'Receber' ? 'Recebido' : 'Pago'
}

export const tonsDeSituacao: Record<Situacao, TomEtiqueta> = {
  Aberto: 'marca',
  ParcialmenteBaixado: 'atencao',
  Baixado: 'positivo',
  Cancelado: 'neutro',
  Vencido: 'negativo',
}

export const rotulosDeOrigem: Record<OrigemTitulo, string> = {
  PedidoDeVenda: 'Pedido de venda',
  PedidoDeCompra: 'Pedido de compra',
  LancamentoManual: 'Lançamento manual',
}

export const rotulosDeForma: Record<FormaPagamento, string> = {
  Pix: 'Pix',
  Boleto: 'Boleto',
  CartaoCredito: 'Cartão de crédito',
  CartaoDebito: 'Cartão de débito',
  Dinheiro: 'Dinheiro',
  Transferencia: 'Transferência',
}
