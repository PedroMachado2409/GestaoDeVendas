import { zodResolver } from '@hookform/resolvers/zod'
import { ArrowRight, Eye, EyeOff, Lock, Mail } from 'lucide-react'
import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { Navigate, useLocation, useNavigate } from 'react-router-dom'
import { z } from 'zod'

import { useSessao } from '@/auth/useSessao'
import { SeletorDeTema } from '@/componentes/layout/SeletorDeTema'
import { Botao } from '@/componentes/ui/Botao'
import { Campo, Entrada } from '@/componentes/ui/Campo'
import { ErroApi } from '@/lib/erros'
import { aplicarErrosDaApi } from '@/lib/formulario'
import { notificarErro } from '@/lib/notificacoes'
import { esquemaEmail } from '@/lib/validacoes'

const esquemaEntrada = z.object({
  email: esquemaEmail,
  senha: z.string().min(1, 'Informe a senha.').max(128, 'A senha deve ter no máximo 128 caracteres.'),
})

export function Acesso() {
  const { autenticado, token, carregando, motivoDaUltimaSaida, limparMotivoDeSaida } = useSessao()
  const local = useLocation()

  useEffect(() => {
    if (motivoDaUltimaSaida === 'expirada') {
      notificarErro(new ErroApi(401), 'Sessão encerrada')
      limparMotivoDeSaida()
    }
  }, [motivoDaUltimaSaida, limparMotivoDeSaida])

  if (token && carregando) return null
  if (autenticado) {
    const destino = (local.state as { de?: string } | null)?.de ?? '/'
    return <Navigate to={destino} replace />
  }

  return (
    <div className="grid min-h-dvh lg:grid-cols-[1.05fr_1fr]">
      <PainelDeMarca />
      <div className="relative flex items-center justify-center px-5 py-10 sm:px-10">
        <div className="absolute top-5 right-5"><SeletorDeTema /></div>
        <div className="w-full max-w-sm">
          <div className="mb-7 lg:hidden">
            <span aria-hidden className="grid size-10 place-items-center rounded-xl bg-brand text-sm font-bold text-on-brand">GP</span>
          </div>
          <h1 className="text-2xl font-semibold tracking-tight text-ink">Entrar na plataforma</h1>
          <p className="mt-1.5 text-sm text-ink-muted">Use seu e-mail e senha para acessar o sistema.</p>
          <div className="mt-6"><FormularioDeEntrada /></div>
        </div>
      </div>
    </div>
  )
}

function FormularioDeEntrada() {
  const { entrar } = useSessao()
  const navegar = useNavigate()
  const local = useLocation()
  const [visivel, definirVisivel] = useState(false)

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<z.infer<typeof esquemaEntrada>>({
    resolver: zodResolver(esquemaEntrada),
    defaultValues: { email: '', senha: '' },
  })

  async function enviar(dados: z.infer<typeof esquemaEntrada>) {
    try {
      await entrar(dados)
      const destino = (local.state as { de?: string } | null)?.de ?? '/'
      navegar(destino, { replace: true })
    } catch (erro) {
      aplicarErrosDaApi(erro, setError, ['email', 'senha'])

      if (erro instanceof ErroApi && erro.status === 401) {
        setError('senha', { type: 'server', message: 'E-mail ou senha incorretos.' })
        notificarErro(new ErroApi(401, {
          detail: 'E-mail ou senha incorretos.', traceId: erro.traceId,
        }), 'Não foi possível entrar')
        return
      }
      notificarErro(erro, 'Não foi possível entrar')
    }
  }

  return (
    <form onSubmit={handleSubmit(enviar)} className="grid gap-4" noValidate>
      <Campo rotulo="E-mail" erro={errors.email?.message} obrigatorio>
        {(propriedades) => (
          <Entrada
            {...propriedades}
            {...register('email')}
            type="email"
            autoComplete="username"
            autoFocus
            placeholder="voce@empresa.com"
            prefixo={<Mail aria-hidden className="size-4" />}
          />
        )}
      </Campo>

      <Campo rotulo="Senha" erro={errors.senha?.message} obrigatorio>
        {(propriedades) => (
          <Entrada
            {...propriedades}
            {...register('senha')}
            type={visivel ? 'text' : 'password'}
            autoComplete="current-password"
            placeholder="••••••••••"
            prefixo={<Lock aria-hidden className="size-4" />}
            sufixo={
              <button
                type="button"
                onClick={() => definirVisivel((v) => !v)}
                aria-label={visivel ? 'Ocultar senha' : 'Mostrar senha'}
                className="grid size-9 place-items-center rounded-lg text-ink-muted transition-colors hover:text-ink"
              >
                {visivel ? (
                  <EyeOff aria-hidden className="size-4" />
                ) : (
                  <Eye aria-hidden className="size-4" />
                )}
              </button>
            }
          />
        )}
      </Campo>

      <Botao
        type="submit"
        variante="primario"
        tamanho="grande"
        carregando={isSubmitting}
        className="mt-1 w-full justify-center"
        iconeFinal={<ArrowRight aria-hidden className="size-4" />}
      >
        Entrar
      </Botao>
    </form>
  )
}

function PainelDeMarca() {
  return (
    <div className="relative hidden overflow-hidden bg-brand p-12 text-on-brand lg:flex lg:flex-col">
      <div
        aria-hidden
        className="pointer-events-none absolute inset-0 opacity-[0.14]"
        style={{ backgroundImage: 'radial-gradient(circle at 1px 1px, currentColor 1px, transparent 0)', backgroundSize: '22px 22px' }}
      />
      <div aria-hidden className="pointer-events-none absolute -top-24 -right-24 size-[28rem] rounded-full bg-white/10 blur-3xl" />
      <div className="relative flex items-center gap-3">
        <span className="grid size-10 place-items-center rounded-xl bg-white/15 text-sm font-bold backdrop-blur">GP</span>
        <span className="text-[0.9375rem] font-semibold tracking-tight">Gestão de Pedidos</span>
      </div>
      <div className="relative my-auto max-w-md py-12">
        <h2 className="text-[2rem] leading-tight font-semibold tracking-tight">Pessoas, produtos e pedidos em um só lugar.</h2>
      </div>
    </div>
  )
}
