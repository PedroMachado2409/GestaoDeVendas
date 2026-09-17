/**
 * Espelho dos DTOs da API (GestaoPedidos/Application/DTO).
 * Enums chegam como texto porque o backend registra JsonStringEnumConverter.
 */

export type Papel = 'Admin' | 'Vendedor'

export type StatusPedido = 'Aberto' | 'Finalizado' | 'Cancelado'

/** Saida = venda (reserva estoque); Entrada = compra (anota pendência). */
export type TipoMovimentacao = 'Entrada' | 'Saida'

export type OrigemMovimentacao = 'PedidoDeVenda' | 'PedidoDeCompra' | 'AjusteManual'

export interface Cliente {
  id: number
  nome: string
  email: string
  cpf: string
  ativo: boolean
  dataCadastro: string
}

export interface ClienteCreate {
  nome: string
  email: string
  cpf: string
}

export interface ClienteUpdate extends ClienteCreate {
  id: number
}

export interface Produto {
  id: number
  nome: string
  preco: number
  marca: string
  estoque: number
  quantidadeReservada: number
  dataCadastro: string
  ativo: boolean
}

export interface ProdutoCreate {
  nome: string
  preco: number
  marca: string
  estoque: number
}

export interface ProdutoUpdate extends ProdutoCreate {
  id: number
}

/** Positivo soma ao disponível, negativo retira. Zero é recusado pela API. */
export interface AjusteEstoque {
  quantidade: number
}

export interface MovimentacaoEstoque {
  id: number
  observacao: string
  produtoId: number
  /** Nome do produto no momento da movimentação, não o atual. */
  produtoNome: string
  /** Sempre positiva; o sentido está em tipoMovimentacao. */
  quantidade: number
  /** Pedido de origem, ou o usuário que fez o ajuste manual. */
  idOrigem: number
  origemMovimentacao: OrigemMovimentacao
  tipoMovimentacao: TipoMovimentacao
  dataMovimentacao: string
}

export interface PedidoItem {
  id: number
  produtoId: number
  preco: number
  quantidade: number
  subTotal: number
}

export interface Pedido {
  id: number
  clienteId: number
  /** Vazio quando o repositório não carrega o cliente junto. */
  clienteNome: string
  /** Autor do pedido. Pedidos históricos podem vir sem autor (0 e ""). */
  usuarioId: number
  usuarioNome: string
  status: StatusPedido
  dataCadastro: string
  valorTotal: number
  itens: PedidoItem[]
}

export interface CriarPedidoItem {
  produtoId: number
  quantidade: number
}

export interface CriarPedido {
  clienteId: number
  // A API usa camelCase aqui ("tipoMovimentacao") e o padrão do enum é
  // Entrada: omitir o campo transforma uma venda em compra.
  tipoMovimentacao: TipoMovimentacao
  itens: CriarPedidoItem[]
}

export interface Usuario {
  id: number
  nome: string
  email: string
  role: Papel
  dataCadastro: string
  ativo: boolean
}

export interface LoginRequest {
  email: string
  senha: string
}

export interface LoginResponse {
  nome: string
  email: string
  token: string
}

export interface UsuarioCreate {
  nome: string
  email: string
  senha: string
}

export interface UsuarioUpdate {
  nome: string
  email: string
}

export interface UsuarioUpdateSenha {
  senhaAntiga: string
  novaSenha: string
}
