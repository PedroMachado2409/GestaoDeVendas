import { ShieldAlert } from 'lucide-react'
import type { ReactNode } from 'react'
import { Navigate, useLocation } from 'react-router-dom'

import { LinkBotao } from '@/componentes/ui/Botao'
import { Cartao } from '@/componentes/ui/Cartao'

import { useSessao } from './useSessao'

/**
 * Esconder a rota é conveniência, não segurança: quem decide é a API, que
 * responde 403 para vendedor em endpoint de admin. Aqui só evitamos levar o
 * usuário a uma tela que ele não conseguiria usar.
 */
export function RotaProtegida({
  children,
  somenteAdmin = false,
}: {
  children: ReactNode
  somenteAdmin?: boolean
}) {
  const { autenticado, carregando, ehAdmin, token } = useSessao()
  const local = useLocation()

  if (token && carregando) {
    return <TelaDeCarregamento />
  }

  if (!autenticado) {
    return <Navigate to="/entrar" replace state={{ de: local.pathname + local.search }} />
  }

  if (somenteAdmin && !ehAdmin) {
    return <SemPermissao />
  }

  return <>{children}</>
}

function TelaDeCarregamento() {
  return (
    <div className="grid min-h-dvh place-items-center bg-canvas" role="status">
      <div className="flex flex-col items-center gap-3">
        <span
          aria-hidden
          className="grid size-11 animate-pulse place-items-center rounded-xl bg-brand text-sm font-bold text-on-brand"
        >
          GP
        </span>
        <p className="text-sm text-ink-muted">Restaurando sua sessão…</p>
      </div>
    </div>
  )
}

function SemPermissao() {
  return (
    <Cartao className="mx-auto max-w-lg">
      <div className="flex flex-col items-center gap-3 px-6 py-12 text-center">
        <div className="grid size-11 place-items-center rounded-xl border border-caution/30 bg-caution-soft text-caution-ink">
          <ShieldAlert aria-hidden className="size-5" />
        </div>
        <div className="grid gap-1">
          <h1 className="text-base font-semibold text-ink">Área restrita a administradores</h1>
          <p className="text-sm text-ink-muted">
            Seu perfil é <strong className="text-ink-soft">Vendedor</strong>. A API recusa este
            recurso para papéis que não sejam Admin.
          </p>
        </div>
        <LinkBotao to="/" variante="contorno" tamanho="pequeno">
          Voltar ao painel
        </LinkBotao>
      </div>
    </Cartao>
  )
}
