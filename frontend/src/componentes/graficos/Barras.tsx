import { cn } from '@/lib/utils'

export interface PontoDeSerie {
  rotulo: string
  valor: number
  /** Texto completo do rótulo, quando o eixo mostra uma forma abreviada. */
  rotuloLongo?: string
}

interface PropriedadesComuns {
  dados: PontoDeSerie[]
  formatarValor: (valor: number) => string
  /** Cabeçalho da coluna de valores na tabela equivalente. */
  nomeDaMedida: string
  titulo: string
}

/**
 * Série única, uma cor só (a da marca): o comprimento já codifica a magnitude,
 * então variar o matiz por barra não acrescentaria informação.
 *
 * Os gráficos são HTML/CSS em vez de SVG para que os rótulos fiquem no mesmo
 * tamanho em qualquer largura de tela. Toda barra traz o valor por extenso e
 * há uma tabela equivalente para leitores de tela — a cor nunca é o único
 * canal de leitura.
 */
export function ColunasPorPeriodo({
  dados,
  formatarValor,
  nomeDaMedida,
  titulo,
}: PropriedadesComuns) {
  const maximo = Math.max(...dados.map((ponto) => ponto.valor), 1)

  return (
    <figure className="m-0">
      <div className="relative pt-6 pb-1">
        {/* Grade em linhas cheias e discretas, um tom acima da superfície. */}
        <div aria-hidden className="pointer-events-none absolute inset-x-0 top-6 bottom-7 grid">
          {[0, 1, 2, 3].map((linha) => (
            <div key={linha} className="border-t border-line first:border-transparent" />
          ))}
        </div>

        <ul className="relative flex h-40 items-end gap-2">
          {dados.map((ponto) => {
            const proporcao = ponto.valor / maximo
            return (
              <li
                key={ponto.rotulo}
                className="group flex h-full min-w-0 flex-1 flex-col justify-end gap-1.5"
                title={`${ponto.rotuloLongo ?? ponto.rotulo}: ${formatarValor(ponto.valor)}`}
              >
                <span className="numerico text-center text-[0.6875rem] font-medium text-ink-soft">
                  {ponto.valor === 0 ? '' : formatarValor(ponto.valor)}
                </span>
                <div
                  className={cn(
                    'w-full rounded-t bg-brand transition-opacity group-hover:opacity-85',
                    ponto.valor === 0 && 'bg-line',
                  )}
                  style={{
                    height: `${Math.max(proporcao * 100, ponto.valor === 0 ? 2 : 6)}%`,
                  }}
                />
              </li>
            )
          })}
        </ul>

        <ul className="mt-2 flex gap-2 border-t border-line pt-2">
          {dados.map((ponto) => (
            <li
              key={ponto.rotulo}
              className="min-w-0 flex-1 truncate text-center text-[0.6875rem] text-ink-muted"
            >
              {ponto.rotulo}
            </li>
          ))}
        </ul>
      </div>

      <TabelaEquivalente titulo={titulo} dados={dados} formatarValor={formatarValor} nomeDaMedida={nomeDaMedida} />
    </figure>
  )
}

export function BarrasHorizontais({
  dados,
  formatarValor,
  nomeDaMedida,
  titulo,
}: PropriedadesComuns) {
  const maximo = Math.max(...dados.map((ponto) => ponto.valor), 1)

  return (
    <figure className="m-0">
      <ul className="grid grid-cols-1 gap-3">
        {dados.map((ponto) => (
          <li
            key={ponto.rotulo}
            className="group grid min-w-0 grid-cols-1 gap-1.5"
            title={`${ponto.rotuloLongo ?? ponto.rotulo}: ${formatarValor(ponto.valor)}`}
          >
            <div className="flex items-baseline justify-between gap-3">
              <span className="min-w-0 truncate text-[0.8125rem] text-ink-soft">
                {ponto.rotulo}
              </span>
              <span className="numerico shrink-0 text-[0.8125rem] font-medium text-ink">
                {formatarValor(ponto.valor)}
              </span>
            </div>
            <div className="h-1.5 w-full rounded-full bg-surface-3">
              <div
                className="h-full rounded-full bg-brand transition-opacity group-hover:opacity-85"
                style={{ width: `${Math.max((ponto.valor / maximo) * 100, 2)}%` }}
              />
            </div>
          </li>
        ))}
      </ul>

      <TabelaEquivalente titulo={titulo} dados={dados} formatarValor={formatarValor} nomeDaMedida={nomeDaMedida} />
    </figure>
  )
}

function TabelaEquivalente({
  titulo,
  dados,
  formatarValor,
  nomeDaMedida,
}: PropriedadesComuns) {
  // sr-only num <div> e não direto na <table>: tabela ignora width e overflow,
  // e a "invisível" ocupava a largura inteira do conteúdo — no celular, a
  // página ganhava rolagem lateral.
  return (
    <div className="sr-only">
      <table>
        <caption>{titulo}</caption>
        <thead>
          <tr>
            <th scope="col">Item</th>
            <th scope="col">{nomeDaMedida}</th>
          </tr>
        </thead>
        <tbody>
          {dados.map((ponto) => (
            <tr key={ponto.rotulo}>
              <th scope="row">{ponto.rotuloLongo ?? ponto.rotulo}</th>
              <td>{formatarValor(ponto.valor)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
