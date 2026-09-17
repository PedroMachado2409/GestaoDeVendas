import { Loader2 } from 'lucide-react'
import { forwardRef, type ButtonHTMLAttributes, type ReactNode } from 'react'
import { Link, type LinkProps } from 'react-router-dom'

import { cn } from '@/lib/utils'

export type VarianteBotao = 'primario' | 'secundario' | 'sutil' | 'contorno' | 'perigo'
export type TamanhoBotao = 'pequeno' | 'medio' | 'grande' | 'icone'

const variantes: Record<VarianteBotao, string> = {
  primario:
    'bg-brand text-on-brand shadow-e1 hover:bg-brand-hover active:translate-y-px disabled:hover:bg-brand',
  secundario:
    'bg-surface-2 text-ink border border-line hover:bg-surface-3 active:translate-y-px disabled:hover:bg-surface-2',
  sutil: 'text-ink-soft hover:bg-surface-2 hover:text-ink active:translate-y-px',
  contorno:
    'border border-line-strong text-ink hover:bg-surface-2 active:translate-y-px disabled:hover:bg-transparent',
  perigo:
    'bg-negative text-white shadow-e1 hover:brightness-110 active:translate-y-px disabled:hover:brightness-100',
}

const tamanhos: Record<TamanhoBotao, string> = {
  pequeno: 'h-8 px-3 text-[0.8125rem] gap-1.5 rounded-lg',
  medio: 'h-9.5 px-4 text-sm gap-2 rounded-lg',
  grande: 'h-11 px-5 text-[0.9375rem] gap-2 rounded-xl',
  icone: 'size-9 justify-center rounded-lg',
}

const base =
  'inline-flex items-center font-medium whitespace-nowrap transition-[background-color,color,box-shadow,transform,filter] duration-150 select-none disabled:pointer-events-none disabled:opacity-55'

interface PropriedadesComuns {
  variante?: VarianteBotao
  tamanho?: TamanhoBotao
  carregando?: boolean
  iconeInicial?: ReactNode
  iconeFinal?: ReactNode
}

export type PropriedadesBotao = ButtonHTMLAttributes<HTMLButtonElement> & PropriedadesComuns

export const Botao = forwardRef<HTMLButtonElement, PropriedadesBotao>(function Botao(
  {
    variante = 'secundario',
    tamanho = 'medio',
    carregando = false,
    iconeInicial,
    iconeFinal,
    className,
    children,
    disabled,
    type = 'button',
    ...resto
  },
  ref,
) {
  return (
    <button
      ref={ref}
      type={type}
      disabled={disabled || carregando}
      aria-busy={carregando || undefined}
      className={cn(base, variantes[variante], tamanhos[tamanho], className)}
      {...resto}
    >
      {carregando ? (
        <Loader2 aria-hidden className="size-4 shrink-0 animate-spin" />
      ) : (
        iconeInicial
      )}
      {children}
      {!carregando && iconeFinal}
    </button>
  )
})

export type PropriedadesLinkBotao = LinkProps & PropriedadesComuns

export function LinkBotao({
  variante = 'secundario',
  tamanho = 'medio',
  iconeInicial,
  iconeFinal,
  className,
  children,
  ...resto
}: PropriedadesLinkBotao) {
  return (
    <Link className={cn(base, variantes[variante], tamanhos[tamanho], className)} {...resto}>
      {iconeInicial}
      {children}
      {iconeFinal}
    </Link>
  )
}
