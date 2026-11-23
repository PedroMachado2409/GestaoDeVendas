export interface AlertaDeErro {
  titulo: string
  mensagem: string
  traceId?: string
  chave?: string
}

/** Uma fila compartilhada mantém os alertas entre telas e evita duplicatas simultâneas. */
export function criarCentralDeAlertas() {
  let pendentes: AlertaDeErro[] = []
  const ouvintes = new Set<() => void>()
  const avisar = () => ouvintes.forEach((ouvinte) => ouvinte())

  return {
    obterAtual: () => pendentes[0] ?? null,
    assinar: (ouvinte: () => void) => {
      ouvintes.add(ouvinte)
      return () => { ouvintes.delete(ouvinte) }
    },
    adicionar: (alerta: AlertaDeErro) => {
      const chave = alerta.chave ?? `${alerta.titulo}:${alerta.mensagem}:${alerta.traceId ?? ''}`
      if (pendentes.some((pendente) => pendente.chave === chave)) return
      pendentes = [...pendentes, { ...alerta, chave }]
      avisar()
    },
    fecharAtual: () => {
      pendentes = pendentes.slice(1)
      avisar()
    },
  }
}

export const alertasDeErro = criarCentralDeAlertas()
