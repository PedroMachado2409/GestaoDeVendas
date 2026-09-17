/**
 * DADOS FICTÍCIOS — só para visualizar a tela enquanto a API do financeiro
 * não existe. Nada aqui é lido nem gravado no servidor.
 *
 * As datas são relativas a hoje, para sempre haver títulos vencidos, a
 * vencer e baixados, e os valores das parcelas saem da mesma regra de
 * gerarParcelas() que a tela usa.
 */

import { adicionarDias, diasEntre, hoje } from './datas'
import { gerarParcelas } from './prazos'
import type {
  Contraparte,
  FormaPagamento,
  OrigemTitulo,
  PrazoPagamento,
  TipoTitulo,
  TituloFinanceiro,
} from './tipos'

export const prazosFicticios: PrazoPagamento[] = [
  { id: 1, nome: 'À vista', ativo: true, parcelas: [{ numero: 1, dias: 0, percentual: 100 }] },
  { id: 2, nome: '30 dias', ativo: true, parcelas: [{ numero: 1, dias: 30, percentual: 100 }] },
  {
    id: 3,
    nome: '30/60',
    ativo: true,
    parcelas: [
      { numero: 1, dias: 30, percentual: 50 },
      { numero: 2, dias: 60, percentual: 50 },
    ],
  },
  {
    id: 4,
    nome: '30/60/90',
    ativo: true,
    parcelas: [
      { numero: 1, dias: 30, percentual: 33.33 },
      { numero: 2, dias: 60, percentual: 33.33 },
      { numero: 3, dias: 90, percentual: 33.34 },
    ],
  },
  {
    id: 5,
    nome: 'Entrada + 30',
    ativo: true,
    parcelas: [
      { numero: 1, dias: 0, percentual: 30 },
      { numero: 2, dias: 30, percentual: 70 },
    ],
  },
  {
    id: 6,
    nome: '28/56 (antigo)',
    ativo: false,
    parcelas: [
      { numero: 1, dias: 28, percentual: 50 },
      { numero: 2, dias: 56, percentual: 50 },
    ],
  },
]

export const clientesFicticios: Contraparte[] = [
  { id: 101, nome: 'Mercado Bom Preço Ltda', documento: '12.345.678/0001-90' },
  { id: 102, nome: 'Ana Paula Ferreira', documento: '529.982.247-25' },
  { id: 103, nome: 'Distribuidora Santa Luzia', documento: '98.765.432/0001-10' },
  { id: 104, nome: 'Carlos Eduardo Martins', documento: '111.444.777-35' },
  { id: 105, nome: 'Loja Esportiva Arena', documento: '45.678.901/0001-23' },
]

export const fornecedoresFicticios: Contraparte[] = [
  { id: 201, nome: 'Têxtil Nordeste S.A.', documento: '23.456.789/0001-01' },
  { id: 202, nome: 'Nike do Brasil Com. e Part.', documento: '59.546.515/0001-34' },
  { id: 203, nome: 'Imobiliária Centro', documento: '34.567.890/0001-12' },
  { id: 204, nome: 'Companhia de Energia', documento: '07.522.669/0001-92' },
  { id: 205, nome: 'Transportadora Rápida', documento: '56.789.012/0001-45' },
]

interface Documento {
  tipo: TipoTitulo
  origem: OrigemTitulo
  pedidoId: number | null
  descricao: string
  contraparte: Contraparte
  prazoId: number
  valor: number
  /** Dias atrás em que o documento foi emitido. */
  emitidoHaDias: number
  forma: FormaPagamento
  /** Quantas parcelas já foram baixadas, e se a seguinte está parcial. */
  parcelasBaixadas: number
  proximaParcial?: boolean
  cancelado?: boolean
}

const documentos: Documento[] = [
  { tipo: 'Receber', origem: 'PedidoDeVenda', pedidoId: 12, descricao: 'Venda — pedido #12', contraparte: clientesFicticios[0]!, prazoId: 4, valor: 150_000, emitidoHaDias: 75, forma: 'Boleto', parcelasBaixadas: 1 },
  { tipo: 'Receber', origem: 'PedidoDeVenda', pedidoId: 11, descricao: 'Venda — pedido #11', contraparte: clientesFicticios[1]!, prazoId: 1, valor: 100, emitidoHaDias: 18, forma: 'Pix', parcelasBaixadas: 1 },
  { tipo: 'Receber', origem: 'PedidoDeVenda', pedidoId: 9, descricao: 'Venda — pedido #9', contraparte: clientesFicticios[2]!, prazoId: 3, valor: 600, emitidoHaDias: 48, forma: 'Boleto', parcelasBaixadas: 0 },
  { tipo: 'Receber', origem: 'PedidoDeVenda', pedidoId: 8, descricao: 'Venda — pedido #8', contraparte: clientesFicticios[4]!, prazoId: 5, valor: 1_500, emitidoHaDias: 20, forma: 'Pix', parcelasBaixadas: 1 },
  { tipo: 'Receber', origem: 'PedidoDeVenda', pedidoId: 14, descricao: 'Venda — pedido #14', contraparte: clientesFicticios[3]!, prazoId: 4, valor: 8_990.5, emitidoHaDias: 26, forma: 'Boleto', parcelasBaixadas: 0 },
  { tipo: 'Receber', origem: 'PedidoDeVenda', pedidoId: 7, descricao: 'Venda — pedido #7', contraparte: clientesFicticios[0]!, prazoId: 2, valor: 1_500, emitidoHaDias: 40, forma: 'Boleto', parcelasBaixadas: 0, proximaParcial: true },
  { tipo: 'Receber', origem: 'LancamentoManual', pedidoId: null, descricao: 'Serviço de personalização', contraparte: clientesFicticios[4]!, prazoId: 2, valor: 780, emitidoHaDias: 25, forma: 'Transferencia', parcelasBaixadas: 0 },
  { tipo: 'Receber', origem: 'PedidoDeVenda', pedidoId: 13, descricao: 'Venda — pedido #13', contraparte: clientesFicticios[1]!, prazoId: 3, valor: 169_900, emitidoHaDias: 2, forma: 'Boleto', parcelasBaixadas: 0, cancelado: true },

  { tipo: 'Pagar', origem: 'PedidoDeCompra', pedidoId: 6, descricao: 'Compra — pedido #6', contraparte: fornecedoresFicticios[1]!, prazoId: 4, valor: 42_300, emitidoHaDias: 70, forma: 'Boleto', parcelasBaixadas: 2 },
  { tipo: 'Pagar', origem: 'PedidoDeCompra', pedidoId: 10, descricao: 'Compra — pedido #10', contraparte: fornecedoresFicticios[0]!, prazoId: 3, valor: 12_750, emitidoHaDias: 35, forma: 'Boleto', parcelasBaixadas: 0 },
  { tipo: 'Pagar', origem: 'LancamentoManual', pedidoId: null, descricao: 'Aluguel do galpão', contraparte: fornecedoresFicticios[2]!, prazoId: 1, valor: 6_500, emitidoHaDias: 3, forma: 'Transferencia', parcelasBaixadas: 0 },
  { tipo: 'Pagar', origem: 'LancamentoManual', pedidoId: null, descricao: 'Conta de energia', contraparte: fornecedoresFicticios[3]!, prazoId: 2, valor: 1_284.37, emitidoHaDias: 28, forma: 'Pix', parcelasBaixadas: 0 },
  { tipo: 'Pagar', origem: 'LancamentoManual', pedidoId: null, descricao: 'Frete de mercadorias', contraparte: fornecedoresFicticios[4]!, prazoId: 5, valor: 2_100, emitidoHaDias: 8, forma: 'Pix', parcelasBaixadas: 1 },
]

export function gerarTitulosFicticios(usuarioNome: string): TituloFinanceiro[] {
  const referencia = hoje()
  const titulos: TituloFinanceiro[] = []
  let proximoId = 1
  let proximaBaixa = 1

  documentos.forEach((documento, indiceDoDocumento) => {
    const prazo = prazosFicticios.find((item) => item.id === documento.prazoId)!
    const emissao = adicionarDias(referencia, -documento.emitidoHaDias)
    const parcelas = gerarParcelas(documento.valor, emissao, prazo.parcelas)
    const numeroDocumento = documento.pedidoId
      ? `PED-${documento.pedidoId}`
      : `LAN-${String(indiceDoDocumento + 1).padStart(4, '0')}`

    parcelas.forEach((parcela, indice) => {
      const baixada = indice < documento.parcelasBaixadas
      const parcial = !baixada && documento.proximaParcial && indice === documento.parcelasBaixadas
      const valorBaixado = baixada ? parcela.valor : parcial ? Math.round(parcela.valor * 40) / 100 : 0
      // Baixa alguns dias antes do vencimento, mas nunca no futuro.
      const dataDaBaixa =
        diasEntre(referencia, parcela.vencimento) > 0 ? referencia : adicionarDias(parcela.vencimento, -2)

      titulos.push({
        id: proximoId++,
        tipo: documento.tipo,
        descricao: documento.descricao,
        numeroDocumento,
        contraparteId: documento.contraparte.id,
        contraparteNome: documento.contraparte.nome,
        pedidoId: documento.pedidoId,
        origem: documento.origem,
        numeroParcela: parcela.numero,
        totalParcelas: parcelas.length,
        prazoNome: prazo.nome,
        valorOriginal: parcela.valor,
        valorBaixado,
        dataCadastro: emissao,
        dataVencimento: parcela.vencimento,
        status: documento.cancelado ? 'Cancelado' : baixada ? 'Baixado' : parcial ? 'ParcialmenteBaixado' : 'Aberto',
        baixas:
          valorBaixado > 0
            ? [{ id: proximaBaixa++, data: dataDaBaixa, valor: valorBaixado, forma: documento.forma, usuarioNome }]
            : [],
      })
    })
  })

  return titulos
}
