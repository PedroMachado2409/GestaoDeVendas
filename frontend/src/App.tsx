import { lazy, Suspense } from 'react'
import { Navigate, Route, Routes } from 'react-router-dom'

import { RotaProtegida } from '@/auth/RotaProtegida'
import { Shell } from '@/componentes/layout/Shell'

const Acesso = lazy(() => import('@/paginas/Acesso').then(m => ({ default: m.Acesso })))
const Pessoas = lazy(() => import('@/paginas/Pessoas').then(m => ({ default: m.Pessoas })))
const Estoque = lazy(() => import('@/paginas/Estoque').then(m => ({ default: m.Estoque })))
const Financeiro = lazy(() => import('@/paginas/Financeiro').then(m => ({ default: m.Financeiro })))
const NaoEncontrado = lazy(() => import('@/paginas/NaoEncontrado').then(m => ({ default: m.NaoEncontrado })))
const Painel = lazy(() => import('@/paginas/Painel').then(m => ({ default: m.Painel })))
const Pedidos = lazy(() => import('@/paginas/Pedidos').then(m => ({ default: m.Pedidos })))
const Perfil = lazy(() => import('@/paginas/Perfil').then(m => ({ default: m.Perfil })))
const Produtos = lazy(() => import('@/paginas/Produtos').then(m => ({ default: m.Produtos })))
const Usuarios = lazy(() => import('@/paginas/Usuarios').then(m => ({ default: m.Usuarios })))

export function App() {
  return (
    <Suspense fallback={<p role="status" className="p-6 text-sm text-ink-muted">Carregando página…</p>}>
      <Routes>
        <Route path="/entrar" element={<Acesso />} />

        <Route
          element={
            <RotaProtegida>
              <Shell />
            </RotaProtegida>
          }
        >
          <Route index element={<Painel />} />
          <Route path="pedidos" element={<Pedidos key="vendas" tipo="Saida" />} />
          <Route
            path="compras"
            element={
              <RotaProtegida somenteAdmin>
                <Pedidos key="compras" tipo="Entrada" />
              </RotaProtegida>
            }
          />
          <Route path="pessoas" element={<Pessoas />} />
          <Route path="clientes" element={<Navigate to="/pessoas" replace />} />
          <Route path="produtos" element={<Produtos />} />
          <Route
            path="estoque"
            element={
              <RotaProtegida somenteAdmin>
                <Estoque />
              </RotaProtegida>
            }
          />
          <Route
            path="financeiro"
            element={
              <RotaProtegida somenteAdmin>
                <Financeiro />
              </RotaProtegida>
            }
          />
          <Route
            path="usuarios"
            element={
              <RotaProtegida somenteAdmin>
                <Usuarios />
              </RotaProtegida>
            }
          />
          <Route path="perfil" element={<Perfil />} />
          <Route path="*" element={<NaoEncontrado />} />
        </Route>

        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </Suspense>
  )
}
