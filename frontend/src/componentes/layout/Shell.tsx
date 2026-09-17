import { ChevronDown, KeyRound, LogOut, Menu as MenuIcone, UserRound, X } from 'lucide-react'
import { useEffect, useState } from 'react'
import { Link, NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom'

import { useSessao } from '@/auth/useSessao'
import { Botao } from '@/componentes/ui/Botao'
import { Etiqueta } from '@/componentes/ui/Etiqueta'
import { ItemDeMenu, Menu, SeparadorDeMenu } from '@/componentes/ui/Menu'
import { ProvedorFinanceiro } from '@/financeiro/contexto'
import { cn, iniciais } from '@/lib/utils'

import { IndicadorDaApi } from './IndicadorDaApi'
import { itensDeNavegacao } from './navegacao'
import { SeletorDeTema } from './SeletorDeTema'

export function Shell() {
  const { usuario, ehAdmin, sair } = useSessao()
  const [gavetaAberta, definirGaveta] = useState(false)
  const local = useLocation()
  const navegar = useNavigate()

  // Navegar fecha a gaveta no celular.
  useEffect(() => {
    definirGaveta(false)
  }, [local.pathname])

  // Gaveta aberta: o fundo não rola e o Escape fecha.
  useEffect(() => {
    if (!gavetaAberta) {
      return
    }
    const anterior = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    function aoTeclar(evento: KeyboardEvent) {
      if (evento.key === 'Escape') {
        definirGaveta(false)
      }
    }
    document.addEventListener('keydown', aoTeclar)
    return () => {
      document.body.style.overflow = anterior
      document.removeEventListener('keydown', aoTeclar)
    }
  }, [gavetaAberta])

  const itens = itensDeNavegacao.filter((item) => !item.somenteAdmin || ehAdmin)

  return (
    <div className="min-h-dvh bg-canvas">
      <a
        href="#conteudo"
        className="sr-only focus:not-sr-only focus:absolute focus:top-3 focus:left-3 focus:z-50 focus:rounded-lg focus:bg-surface focus:px-3 focus:py-2 focus:text-sm focus:shadow-e3"
      >
        Pular para o conteúdo
      </a>

      {/* Gaveta em telas estreitas */}
      {gavetaAberta && (
        <button
          type="button"
          aria-label="Fechar menu"
          onClick={() => definirGaveta(false)}
          className="fixed inset-0 z-40 bg-black/40 backdrop-blur-[1px] lg:hidden"
        />
      )}

      <aside
        className={cn(
          'fixed inset-y-0 left-0 z-50 flex w-[min(17rem,85vw)] flex-col shadow-e3 lg:w-64 lg:shadow-none border-r border-line bg-surface transition-transform duration-200 lg:translate-x-0',
          gavetaAberta ? 'translate-x-0' : '-translate-x-full',
        )}
      >
        <div className="flex h-14 items-center justify-between gap-2 border-b border-line px-4">
          <Link to="/" className="flex items-center gap-2.5 rounded-lg">
            <span
              aria-hidden
              className="grid size-8 shrink-0 place-items-center rounded-lg bg-brand text-[0.8125rem] font-bold text-on-brand"
            >
              GP
            </span>
            <span className="grid leading-tight">
              <span className="text-[0.8125rem] font-semibold tracking-tight text-ink">
                Gestão de Pedidos
              </span>
              <span className="text-[0.6875rem] text-ink-muted">Painel administrativo</span>
            </span>
          </Link>
          <Botao
            variante="sutil"
            tamanho="icone"
            aria-label="Fechar menu"
            onClick={() => definirGaveta(false)}
            className="lg:hidden"
          >
            <X aria-hidden className="size-4" />
          </Botao>
        </div>

        <nav aria-label="Navegação principal" className="flex-1 overflow-y-auto p-3">
          <ul className="grid gap-0.5">
            {itens.map(({ para, rotulo, icone: Icone, fim, somenteAdmin }) => (
              <li key={para}>
                <NavLink
                  to={para}
                  end={fim}
                  className={({ isActive }) =>
                    cn(
                      'group flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors',
                      isActive
                        ? 'bg-brand-soft text-brand-ink'
                        : 'text-ink-soft hover:bg-surface-2 hover:text-ink',
                    )
                  }
                >
                  {({ isActive }) => (
                    <>
                      <Icone
                        aria-hidden
                        className={cn(
                          'size-4.5 shrink-0',
                          isActive ? 'text-brand' : 'text-ink-muted group-hover:text-ink-soft',
                        )}
                      />
                      {rotulo}
                      {somenteAdmin && (
                        <span className="ml-auto text-[0.625rem] font-semibold tracking-wide text-ink-muted uppercase">
                          admin
                        </span>
                      )}
                    </>
                  )}
                </NavLink>
              </li>
            ))}
          </ul>
        </nav>

        <div className="border-t border-line p-3">
          <Menu
            alinhamento="inicio"
            posicao="acima"
            rotulo="Menu da conta"
            className="w-full"
            classeGatilho="size-auto w-full justify-start gap-2.5 p-2 text-left hover:bg-surface-2"
            gatilho={
              <span className="flex w-full items-center gap-2.5 text-left">
                <span
                  aria-hidden
                  className="grid size-8 shrink-0 place-items-center rounded-full bg-surface-3 text-[0.6875rem] font-semibold text-ink-soft"
                >
                  {iniciais(usuario?.nome ?? '')}
                </span>
                <span className="grid min-w-0 flex-1 leading-tight">
                  <span className="truncate text-[0.8125rem] font-medium text-ink">
                    {usuario?.nome}
                  </span>
                  <span className="truncate text-[0.6875rem] text-ink-muted">{usuario?.email}</span>
                </span>
                <ChevronDown aria-hidden className="size-4 shrink-0 text-ink-muted" />
              </span>
            }
          >
            {(fechar) => (
              <>
                <div className="px-2.5 py-2">
                  <p className="text-[0.8125rem] font-medium text-ink">{usuario?.nome}</p>
                  <p className="mt-0.5 text-[0.6875rem] text-ink-muted">{usuario?.email}</p>
                  <Etiqueta tom={ehAdmin ? 'marca' : 'neutro'} className="mt-2">
                    {usuario?.role}
                  </Etiqueta>
                </div>
                <SeparadorDeMenu />
                <ItemDeMenu
                  icone={<UserRound aria-hidden className="size-4" />}
                  onClick={() => {
                    fechar()
                    navegar('/perfil')
                  }}
                >
                  Meu perfil
                </ItemDeMenu>
                <ItemDeMenu
                  icone={<KeyRound aria-hidden className="size-4" />}
                  onClick={() => {
                    fechar()
                    navegar('/perfil?secao=senha')
                  }}
                >
                  Alterar senha
                </ItemDeMenu>
                <SeparadorDeMenu />
                <ItemDeMenu
                  tom="perigo"
                  icone={<LogOut aria-hidden className="size-4" />}
                  onClick={() => {
                    fechar()
                    sair('manual')
                  }}
                >
                  Sair
                </ItemDeMenu>
              </>
            )}
          </Menu>
        </div>
      </aside>

      <div className="lg:pl-64">
        <header className="sticky top-0 z-30 flex h-14 items-center gap-2 border-b border-line bg-canvas/85 px-3 backdrop-blur-md sm:gap-3 sm:px-6 lg:px-8">
          <Botao
            variante="sutil"
            tamanho="icone"
            aria-label="Abrir menu"
            aria-expanded={gavetaAberta}
            onClick={() => definirGaveta(true)}
            className="lg:hidden"
          >
            <MenuIcone aria-hidden className="size-5" />
          </Botao>

          <p className="truncate text-sm font-medium text-ink-soft">
            {itens.find((item) =>
              item.fim ? local.pathname === item.para : local.pathname.startsWith(item.para),
            )?.rotulo ?? 'Conta'}
          </p>

          <div className="ml-auto flex shrink-0 items-center gap-2.5 sm:gap-3">
            <IndicadorDaApi />
            <SeletorDeTema />
          </div>
        </header>

        {/* grid-cols-1 = minmax(0, 1fr): sem isso a coluna implícita cresce até
            a largura da tabela mais larga e a página inteira rola de lado. */}
        <main
          id="conteudo"
          className="mx-auto grid max-w-[1400px] grid-cols-1 gap-5 px-4 pt-5 pb-10 sm:gap-6 sm:p-6 lg:px-8"
        >
          {/* Aqui, e não por tela: o Painel e o Financeiro leem o mesmo estado. */}
          <ProvedorFinanceiro usuarioNome={usuario?.nome ?? 'Você'}>
            <Outlet />
          </ProvedorFinanceiro>
        </main>
      </div>
    </div>
  )
}
