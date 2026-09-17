import { MoreHorizontal } from 'lucide-react'
import {
  useCallback,
  useEffect,
  useId,
  useLayoutEffect,
  useRef,
  useState,
  type CSSProperties,
  type ReactNode,
} from 'react'
import { createPortal } from 'react-dom'

import { cn } from '@/lib/utils'

interface PropriedadesMenu {
  rotulo?: string
  gatilho?: ReactNode
  classeGatilho?: string
  children: (fechar: () => void) => ReactNode
  alinhamento?: 'inicio' | 'fim'
  /** "acima" evita que o menu do rodapé da barra lateral saia da tela. */
  posicao?: 'abaixo' | 'acima'
  className?: string
}

const MARGEM = 4
const BORDA_DA_TELA = 8

// Primeira pintura invisível e fora do fluxo, só para medir o painel antes
// de decidir se ele abre para cima ou para baixo.
const ESTILO_PARA_MEDIR: CSSProperties = { position: 'fixed', top: 0, left: 0, visibility: 'hidden' }

/**
 * Menu de ações por linha. Fecha ao clicar fora, no Escape e ao escolher.
 *
 * O painel é renderizado no <body> com posição fixa. Dentro da tabela ele
 * ficava preso ao contêiner com overflow-x: auto — que também corta no eixo
 * vertical — e sumia nas últimas linhas.
 */
export function Menu({
  rotulo = 'Ações',
  gatilho,
  classeGatilho,
  children,
  alinhamento = 'fim',
  posicao = 'abaixo',
  className,
}: PropriedadesMenu) {
  const [aberto, definirAberto] = useState(false)
  const [estilo, definirEstilo] = useState<CSSProperties>(ESTILO_PARA_MEDIR)
  const botao = useRef<HTMLButtonElement>(null)
  const painel = useRef<HTMLDivElement>(null)
  const idMenu = useId()

  const posicionar = useCallback(() => {
    const alvo = botao.current?.getBoundingClientRect()
    const altura = painel.current?.offsetHeight ?? 0
    const largura = painel.current?.offsetWidth ?? 0
    if (!alvo) {
      return
    }

    const cabeAbaixo = alvo.bottom + MARGEM + altura <= window.innerHeight - BORDA_DA_TELA
    const cabeAcima = alvo.top - MARGEM - altura >= BORDA_DA_TELA
    // Respeita a preferência, mas vira para o outro lado se não couber.
    const paraCima = posicao === 'acima' ? cabeAcima || !cabeAbaixo : !cabeAbaixo && cabeAcima

    const esquerdaDesejada = alinhamento === 'fim' ? alvo.right - largura : alvo.left
    const esquerda = Math.min(
      Math.max(BORDA_DA_TELA, esquerdaDesejada),
      window.innerWidth - largura - BORDA_DA_TELA,
    )

    definirEstilo({
      position: 'fixed',
      left: esquerda,
      top: paraCima ? alvo.top - MARGEM - altura : alvo.bottom + MARGEM,
    })
  }, [alinhamento, posicao])

  useLayoutEffect(() => {
    if (aberto) {
      posicionar()
    } else {
      definirEstilo(ESTILO_PARA_MEDIR)
    }
  }, [aberto, posicionar])

  useEffect(() => {
    if (!aberto) {
      return
    }

    function aoClicarFora(evento: MouseEvent) {
      const alvo = evento.target as Node
      if (!botao.current?.contains(alvo) && !painel.current?.contains(alvo)) {
        definirAberto(false)
      }
    }
    function aoTeclar(evento: KeyboardEvent) {
      if (evento.key === 'Escape') {
        definirAberto(false)
        botao.current?.focus()
      }
    }

    document.addEventListener('mousedown', aoClicarFora)
    document.addEventListener('keydown', aoTeclar)
    // Captura: a rolagem que importa costuma ser a do contêiner da tabela.
    window.addEventListener('scroll', posicionar, true)
    window.addEventListener('resize', posicionar)
    return () => {
      document.removeEventListener('mousedown', aoClicarFora)
      document.removeEventListener('keydown', aoTeclar)
      window.removeEventListener('scroll', posicionar, true)
      window.removeEventListener('resize', posicionar)
    }
  }, [aberto, posicionar])

  return (
    <div className={cn('relative inline-flex', className)}>
      <button
        ref={botao}
        type="button"
        aria-haspopup="menu"
        aria-expanded={aberto}
        aria-controls={aberto ? idMenu : undefined}
        aria-label={rotulo}
        onClick={() => definirAberto((valor) => !valor)}
        className={cn(
          'inline-flex size-9 items-center justify-center rounded-lg text-ink-muted transition-colors hover:bg-surface-3 hover:text-ink md:size-8',
          aberto && 'bg-surface-3 text-ink',
          classeGatilho,
        )}
      >
        {gatilho ?? <MoreHorizontal aria-hidden className="size-4" />}
      </button>

      {aberto &&
        createPortal(
          <div
            ref={painel}
            id={idMenu}
            role="menu"
            aria-label={rotulo}
            style={estilo}
            className="animar-surgir z-[60] max-h-[min(24rem,calc(100dvh-1rem))] min-w-48 overflow-y-auto rounded-xl border border-line bg-surface p-1 shadow-e3"
          >
            {children(() => definirAberto(false))}
          </div>,
          document.body,
        )}
    </div>
  )
}

interface PropriedadesItem {
  children: ReactNode
  onClick?: () => void
  icone?: ReactNode
  tom?: 'neutro' | 'perigo'
  disabled?: boolean
}

export function ItemDeMenu({ children, onClick, icone, tom = 'neutro', disabled }: PropriedadesItem) {
  return (
    <button
      type="button"
      role="menuitem"
      disabled={disabled}
      onClick={onClick}
      className={cn(
        'flex w-full items-center gap-2.5 rounded-lg px-2.5 py-2 text-left text-[0.8125rem] transition-colors disabled:pointer-events-none disabled:opacity-50 md:py-1.5',
        tom === 'perigo'
          ? 'text-negative-ink hover:bg-negative-soft'
          : 'text-ink-soft hover:bg-surface-2 hover:text-ink',
      )}
    >
      {icone && <span className="shrink-0 text-ink-muted">{icone}</span>}
      {children}
    </button>
  )
}

export function SeparadorDeMenu() {
  return <div role="separator" className="my-1 h-px bg-line" />
}
