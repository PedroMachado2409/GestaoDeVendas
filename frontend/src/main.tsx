import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import { Toaster } from 'sonner'

import { App } from './App'
import { ProvedorDeSessao } from './auth/sessao'
import { ErroApi } from './lib/erros'
import { aplicarTema, lerTema } from './lib/tema'

import './index.css'

// Antes da primeira pintura, para não piscar o tema claro em quem usa escuro.
aplicarTema(lerTema())

const clienteDeConsultas = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      refetchOnWindowFocus: false,
      // 401/403/404 não melhoram com repetição; erro de rede e 5xx, sim.
      retry: (tentativas, erro) => {
        if (erro instanceof ErroApi && erro.status >= 400 && erro.status < 500) return false
        return tentativas < 2
      },
    },
    mutations: { retry: false },
  },
})

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={clienteDeConsultas}>
      <BrowserRouter>
        <ProvedorDeSessao>
          <App />
          <Toaster
            position="bottom-right"
            richColors
            closeButton
            toastOptions={{ style: { fontFamily: 'inherit' } }}
          />
        </ProvedorDeSessao>
      </BrowserRouter>
    </QueryClientProvider>
  </StrictMode>,
)
