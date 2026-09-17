import { Monitor, Moon, Sun } from 'lucide-react'
import { useEffect, useState } from 'react'

import { aplicarTema, lerTema, observarTemaDoSistema, type Tema } from '@/lib/tema'
import { cn } from '@/lib/utils'

const opcoes: Array<{ valor: Tema; rotulo: string; Icone: typeof Sun }> = [
  { valor: 'claro', rotulo: 'Tema claro', Icone: Sun },
  { valor: 'escuro', rotulo: 'Tema escuro', Icone: Moon },
  { valor: 'sistema', rotulo: 'Seguir o sistema', Icone: Monitor },
]

export function SeletorDeTema() {
  const [tema, definirTema] = useState<Tema>(() => lerTema())

  useEffect(() => {
    aplicarTema(tema)
  }, [tema])

  useEffect(() => {
    if (tema !== 'sistema') return
    return observarTemaDoSistema(() => aplicarTema('sistema'))
  }, [tema])

  return (
    <div
      role="group"
      aria-label="Aparência"
      className="inline-flex items-center gap-0.5 rounded-lg border border-line bg-surface-2 p-0.5"
    >
      {opcoes.map(({ valor, rotulo, Icone }) => (
        <button
          key={valor}
          type="button"
          title={rotulo}
          aria-label={rotulo}
          aria-pressed={tema === valor}
          onClick={() => definirTema(valor)}
          className={cn(
            'grid size-7 place-items-center rounded-md transition-colors',
            tema === valor
              ? 'bg-surface text-ink shadow-e1'
              : 'text-ink-muted hover:text-ink',
          )}
        >
          <Icone aria-hidden className="size-3.5" />
        </button>
      ))}
    </div>
  )
}
