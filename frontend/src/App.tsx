import { Navigate, Route, Routes } from 'react-router-dom'

import { RotaProtegida } from '@/auth/RotaProtegida'
import { Shell } from '@/componentes/layout/Shell'
import { Acesso } from '@/paginas/Acesso'
import { Pessoas } from '@/paginas/Pessoas'
import { Estoque } from '@/paginas/Estoque'
import { Financeiro } from '@/paginas/Financeiro'
import { NaoEncontrado } from '@/paginas/NaoEncontrado'
import { Painel } from '@/paginas/Painel'
import { Pedidos } from '@/paginas/Pedidos'
import { Perfil } from '@/paginas/Perfil'
import { Produtos } from '@/paginas/Produtos'
import { Usuarios } from '@/paginas/Usuarios'

export function App() {
  return (
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
  )
}
