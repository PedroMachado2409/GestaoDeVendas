import { X } from 'lucide-react'
import { useEffect, useRef, type ReactNode } from 'react'

import { cn } from '@/lib/utils'

import { Botao } from './Botao'

interface Propriedades {
  aberto: boolean
  aoFechar: () => void
  titulo: string
  descricao?: string
  children: ReactNode
  rodape?: ReactNode
  largura?: 'estreita' | 'media' | 'larga'
}

const larguras = {
  estreita: 'max-w-md',
  media: 'max-w-xl',
  larga: 'max-w-3xl',
} as const

/**
 * Usa <dialog> nativo: o navegador cuida do foco preso, do Escape e da
 * camada superior. Só precisamos sincronizar abertura e fechamento.
 *
 * No celular vira folha presa à base da tela, com altura em dvh: com vh, a
 * barra de endereço do navegador escondia o rodapé e os botões de ação.
 */
export function Modal({
  aberto,
  aoFechar,
  titulo,
  descricao,
  children,
  rodape,
  largura = 'media',
}: Propriedades) {
  const referencia = useRef<HTMLDialogElement>(null)

  useEffect(() => {
    const dialogo = referencia.current
    if (!dialogo) return

    if (aberto && !dialogo.open) {
      dialogo.showModal()
    } else if (!aberto && dialogo.open) {
      dialogo.close()
    }
  }, [aberto])

  // Impede a rolagem do fundo enquanto o modal está aberto.
  useEffect(() => {
    if (!aberto) return
    const anterior = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    return () => {
      document.body.style.overflow = anterior
    }
  }, [aberto])

  return (
    <dialog
      ref={referencia}
      aria-labelledby="titulo-modal"
      onCancel={(evento) => {
        evento.preventDefault()
        aoFechar()
      }}
      onClick={(evento) => {
        // Clique no backdrop: o alvo é o próprio <dialog>.
        if (evento.target === referencia.current) aoFechar()
      }}
      className={cn(
        'animar-surgir m-auto max-h-[calc(100dvh-3rem)] w-[calc(100vw-2rem)] flex-col overflow-hidden rounded-(--radius-card) border border-line bg-surface p-0 text-ink shadow-e3 open:flex',
        'max-sm:mb-0 max-sm:max-h-[92dvh] max-sm:w-full max-sm:max-w-none max-sm:rounded-b-none max-sm:border-b-0',
        'backdrop:bg-black/45 backdrop:backdrop-blur-[2px]',
        larguras[largura],
      )}
    >
      <div className="flex shrink-0 items-start justify-between gap-4 border-b border-line px-5 py-4">
        <div className="min-w-0">
          <h2 id="titulo-modal" className="text-base font-semibold tracking-tight">
            {titulo}
          </h2>
          {descricao && <p className="mt-0.5 text-[0.8125rem] text-ink-muted">{descricao}</p>}
        </div>
        <Botao
          variante="sutil"
          tamanho="icone"
          onClick={aoFechar}
          aria-label="Fechar"
          className="-mt-1 -mr-1 shrink-0"
        >
          <X aria-hidden className="size-4" />
        </Botao>
      </div>

      <div className="scrollbar-fina min-h-0 flex-1 overflow-y-auto overscroll-contain px-5 py-4">
        {children}
      </div>

      {rodape && (
        <div
          className={cn(
            'flex shrink-0 flex-wrap items-center justify-end gap-2 border-t border-line bg-surface-2/50 px-5 pt-3.5',
            'pb-[max(0.875rem,env(safe-area-inset-bottom))]',
            // No celular os botões ocupam a linha: alvo de toque maior e sem
            // o botão principal espremido no canto.
            'max-sm:*:[button]:flex-1 max-sm:*:[button]:justify-center',
          )}
        >
          {rodape}
        </div>
      )}
    </dialog>
  )
}
