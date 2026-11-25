export function paginarItens<T>(itens: readonly T[], pagina: number, tamanhoPagina = 20) {
  const totalPaginas = Math.max(1, Math.ceil(itens.length / tamanhoPagina))
  const atual = Math.min(Math.max(1, pagina), totalPaginas)
  const inicio = (atual - 1) * tamanhoPagina
  return {
    itens: itens.slice(inicio, inicio + tamanhoPagina),
    pagina: atual,
    totalPaginas,
    inicio: itens.length === 0 ? 0 : inicio + 1,
    fim: Math.min(inicio + tamanhoPagina, itens.length),
  }
}

/** Busca as páginas necessárias para manter buscas, filtros e seletores completos. */
export async function listarTodasPaginas<T extends { id: number }>(
  buscar: (pagina: number, tamanhoPagina: number, sinal?: AbortSignal) => Promise<T[]>,
  sinal?: AbortSignal,
  tamanhoPagina = 100,
): Promise<T[]> {
  const itens: T[] = []
  const vistos = new Set<number>()

  for (let pagina = 1; ; pagina += 1) {
    sinal?.throwIfAborted()
    const lote = await buscar(pagina, tamanhoPagina, sinal)
    sinal?.throwIfAborted()
    if (!Array.isArray(lote)) throw new Error('A API não retornou uma lista válida.')

    let adicionados = 0
    for (const item of lote) {
      if (!Number.isSafeInteger(item?.id) || item.id <= 0) {
        throw new Error('A API retornou um registro sem identificador válido.')
      }
      if (vistos.has(item.id)) continue
      vistos.add(item.id)
      itens.push(item)
      adicionados += 1
    }

    // Mais itens que o solicitado indica a versão antiga, que ignora a paginação.
    // Uma página repetida também encerra a leitura sem duplicar os registros.
    if (lote.length !== tamanhoPagina || adicionados === 0) return itens
  }
}
