import type { TipoPessoa } from '../api/tipos'

export const CATEGORIAS_PESSOA: ReadonlyArray<{ valor: TipoPessoa; rotulo: string }> = [
  { valor: 'Cliente', rotulo: 'Cliente' },
  { valor: 'Fornecedor', rotulo: 'Fornecedor' },
  { valor: 'Funcionario', rotulo: 'Funcionário' },
  { valor: 'Parceiro', rotulo: 'Parceiro' },
]

/** Aceita o enum numérico e a serialização com JsonStringEnumConverter. */
export function obterCategoria(valor: unknown): TipoPessoa | null {
  if (typeof valor === 'number') return CATEGORIAS_PESSOA[valor]?.valor ?? null
  return CATEGORIAS_PESSOA.find((categoria) => categoria.valor === valor)?.valor ?? null
}

export function rotuloDaCategoria(valor: unknown): string {
  const categoria = obterCategoria(valor)
  return CATEGORIAS_PESSOA.find((opcao) => opcao.valor === categoria)?.rotulo ?? 'Sem categoria'
}
