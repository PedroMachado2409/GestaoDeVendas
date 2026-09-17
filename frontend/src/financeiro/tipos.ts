/**
 * Modelo do financeiro no front. Espelha o desenho combinado para a API
 * (TituloFinanceiro, PrazoPagamento, PrazoPagamentoParcela), que ainda não
 * existe — quando existir, estes tipos passam a vir de api/tipos.ts.
 */

export type TipoTitulo = 'Receber' | 'Pagar'

export type StatusTitulo = 'Aberto' | 'ParcialmenteBaixado' | 'Baixado' | 'Cancelado'

export type OrigemTitulo = 'PedidoDeVenda' | 'PedidoDeCompra' | 'LancamentoManual'

export type FormaPagamento = 'Pix' | 'Boleto' | 'CartaoCredito' | 'CartaoDebito' | 'Dinheiro' | 'Transferencia'

/** Datas sem hora, no formato AAAA-MM-DD. Vencimento não tem fuso. */
export type DataISO = string

export interface Baixa {
  id: number
  data: DataISO
  valor: number
  forma: FormaPagamento
  usuarioNome: string
}

export interface TituloFinanceiro {
  id: number
  tipo: TipoTitulo
  descricao: string
  /** Agrupa as parcelas de um mesmo documento (pedido ou lançamento). */
  numeroDocumento: string
  contraparteId: number
  contraparteNome: string
  pedidoId: number | null
  origem: OrigemTitulo
  numeroParcela: number
  totalParcelas: number
  /** Nome do prazo no momento da geração — cópia, não referência. */
  prazoNome: string
  valorOriginal: number
  valorBaixado: number
  dataCadastro: DataISO
  dataVencimento: DataISO
  status: StatusTitulo
  baixas: Baixa[]
  observacao?: string
}

export interface ParcelaDoPrazo {
  numero: number
  /** Dias a partir da data base (30, 60, 90), não da parcela anterior. */
  dias: number
  percentual: number
}

export interface PrazoPagamento {
  id: number
  nome: string
  ativo: boolean
  parcelas: ParcelaDoPrazo[]
}

export interface Contraparte {
  id: number
  nome: string
  documento: string
}
