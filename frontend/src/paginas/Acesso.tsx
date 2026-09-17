import { zodResolver } from '@hookform/resolvers/zod'
import { ArrowRight, CheckCircle2, Eye, EyeOff, Info, Lock, Mail, UserRound } from 'lucide-react'
import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { Navigate, useLocation, useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { z } from 'zod'

import { usuariosApi } from '@/api/recursos'
import { useSessao } from '@/auth/useSessao'
import { SeletorDeTema } from '@/componentes/layout/SeletorDeTema'
import { Botao } from '@/componentes/ui/Botao'
import { Campo, Entrada } from '@/componentes/ui/Campo'
import { ErroApi } from '@/lib/erros'
import { aplicarErrosDaApi } from '@/lib/formulario'
import { URL_API } from '@/lib/http'
import { cn } from '@/lib/utils'
import { esquemaEmail, esquemaSenhaNova, forcaDaSenha } from '@/lib/validacoes'

const esquemaEntrada = z.object({
  email: esquemaEmail,
  senha: z.string().min(1, 'Informe a senha.').max(128, 'A senha deve ter no máximo 128 caracteres.'),
})

const esquemaCadastro = z.object({
  nome: z
    .string()
    .trim()
    .min(1, 'Informe o nome.')
    .max(120, 'O nome deve ter no máximo 120 caracteres.'),
  email: esquemaEmail,
  senha: esquemaSenhaNova,
})

type Aba = 'entrar' | 'criar'

export function Acesso() {
  const { autenticado, token, carregando, motivoDaUltimaSaida, limparMotivoDeSaida } = useSessao()
  const local = useLocation()
  const [aba, definirAba] = useState<Aba>('entrar')

  useEffect(() => {
    if (motivoDaUltimaSaida === 'expirada') {
      toast.warning('Sessão encerrada', {
        description: 'O token expirou ou foi revogado pela API. Entre novamente.',
      })
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
        <div className="absolute top-5 right-5">
          <SeletorDeTema />
        </div>

        <div className="w-full max-w-sm">
          <div className="mb-7 lg:hidden">
            <span
              aria-hidden
              className="grid size-10 place-items-center rounded-xl bg-brand text-sm font-bold text-on-brand"
            >
              GP
            </span>
          </div>

          <h1 className="text-2xl font-semibold tracking-tight text-ink">
            {aba === 'entrar' ? 'Entrar na plataforma' : 'Criar sua conta'}
          </h1>
          <p className="mt-1.5 text-sm text-ink-muted">
            {aba === 'entrar'
              ? 'Use as credenciais cadastradas na API de Gestão de Pedidos.'
              : 'O cadastro público sempre cria um usuário com papel Vendedor.'}
          </p>

          <div
            role="tablist"
            aria-label="Formas de acesso"
            className="mt-6 inline-flex w-full items-center gap-0.5 rounded-lg border border-line bg-surface-2 p-0.5"
          >
            {(
              [
                ['entrar', 'Entrar'],
                ['criar', 'Criar conta'],
              ] as const
            ).map(([valor, rotulo]) => (
              <button
                key={valor}
                role="tab"
                type="button"
                aria-selected={aba === valor}
                onClick={() => definirAba(valor)}
                className={cn(
                  'h-8 flex-1 rounded-md text-[0.8125rem] font-medium transition-colors',
                  aba === valor
                    ? 'bg-surface text-ink shadow-e1'
                    : 'text-ink-muted hover:text-ink',
                )}
              >
                {rotulo}
              </button>
            ))}
          </div>

          <div className="mt-6">
            {aba === 'entrar' ? (
              <FormularioDeEntrada />
            ) : (
              <FormularioDeCadastro aoConcluir={() => definirAba('entrar')} />
            )}
          </div>

          <p className="mt-8 text-center font-mono text-[0.6875rem] text-ink-muted">{URL_API}</p>
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
      const orfaos = aplicarErrosDaApi(erro, setError, ['email', 'senha'])

      if (erro instanceof ErroApi && erro.status === 401) {
        setError('senha', { type: 'server', message: 'E-mail ou senha incorretos.' })
        return
      }
      if (erro instanceof ErroApi && erro.status === 429) {
        toast.error('Muitas tentativas', {
          description:
            'A API limita 10 tentativas por minuto neste endpoint. Aguarde e tente de novo.',
        })
        return
      }
      toast.error('Não foi possível entrar', {
        description: orfaos.join(' ') || (erro instanceof ErroApi ? erro.mensagem : undefined),
      })
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

function FormularioDeCadastro({ aoConcluir }: { aoConcluir: () => void }) {
  const [visivel, definirVisivel] = useState(false)

  const {
    register,
    handleSubmit,
    setError,
    watch,
    formState: { errors, isSubmitting },
  } = useForm<z.infer<typeof esquemaCadastro>>({
    resolver: zodResolver(esquemaCadastro),
    defaultValues: { nome: '', email: '', senha: '' },
  })

  const senha = watch('senha')
  const forca = forcaDaSenha(senha)

  async function enviar(dados: z.infer<typeof esquemaCadastro>) {
    try {
      await usuariosApi.registrar(dados)
      toast.success('Conta criada', {
        description: 'Agora entre com o e-mail e a senha que você acabou de cadastrar.',
      })
      aoConcluir()
    } catch (erro) {
      const orfaos = aplicarErrosDaApi(erro, setError, ['nome', 'email', 'senha'])

      if (erro instanceof ErroApi && erro.status === 409) {
        setError('email', { type: 'server', message: 'Já existe um usuário com este e-mail.' })
        return
      }
      if (erro instanceof ErroApi && erro.status === 429) {
        toast.error('Muitas tentativas', {
          description: 'A API limita 10 requisições por minuto neste endpoint.',
        })
        return
      }
      if (orfaos.length > 0 || !(erro instanceof ErroApi) || !erro.temErrosDeCampo) {
        toast.error('Não foi possível criar a conta', {
          description: orfaos.join(' ') || (erro instanceof ErroApi ? erro.mensagem : undefined),
        })
      }
    }
  }

  return (
    <form onSubmit={handleSubmit(enviar)} className="grid gap-4" noValidate>
      <Campo rotulo="Nome" erro={errors.nome?.message} obrigatorio>
        {(propriedades) => (
          <Entrada
            {...propriedades}
            {...register('nome')}
            autoComplete="name"
            placeholder="Seu nome completo"
            prefixo={<UserRound aria-hidden className="size-4" />}
          />
        )}
      </Campo>

      <Campo rotulo="E-mail" erro={errors.email?.message} obrigatorio>
        {(propriedades) => (
          <Entrada
            {...propriedades}
            {...register('email')}
            type="email"
            autoComplete="email"
            placeholder="voce@empresa.com"
            prefixo={<Mail aria-hidden className="size-4" />}
          />
        )}
      </Campo>

      <Campo
        rotulo="Senha"
        erro={errors.senha?.message}
        dica="Mínimo de 10 caracteres, com ao menos uma letra e um número."
        obrigatorio
      >
        {(propriedades) => (
          <Entrada
            {...propriedades}
            {...register('senha')}
            type={visivel ? 'text' : 'password'}
            autoComplete="new-password"
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

      {senha.length > 0 && (
        <div className="flex items-center gap-2" aria-live="polite">
          <div className="flex h-1 flex-1 gap-1">
            {[0, 1, 2, 3].map((indice) => (
              <span
                key={indice}
                className={cn(
                  'flex-1 rounded-full transition-colors',
                  indice < forca.nivel
                    ? forca.nivel <= 1
                      ? 'bg-negative'
                      : forca.nivel === 2
                        ? 'bg-caution'
                        : 'bg-positive'
                    : 'bg-surface-3',
                )}
              />
            ))}
          </div>
          <span className="w-16 text-right text-[0.6875rem] text-ink-muted">{forca.rotulo}</span>
        </div>
      )}

      <p className="flex items-start gap-2 rounded-lg border border-line bg-surface-2 px-3 py-2.5 text-xs text-ink-muted">
        <Info aria-hidden className="mt-px size-3.5 shrink-0" />
        Contas criadas por aqui recebem o papel <strong className="text-ink-soft">Vendedor</strong>.
        Somente um administrador pode promover alguém a Admin.
      </p>

      <Botao
        type="submit"
        variante="primario"
        tamanho="grande"
        carregando={isSubmitting}
        className="w-full justify-center"
      >
        Criar conta
      </Botao>
    </form>
  )
}

const destaques = [
  'Reservas de estoque com controle otimista de concorrência',
  'Cancelamento e finalização idempotentes, em transação',
  'JWT com versão de token — troca de senha revoga o acesso',
]

function PainelDeMarca() {
  return (
    <div className="relative hidden overflow-hidden bg-brand p-12 text-on-brand lg:flex lg:flex-col lg:justify-between">
      {/* Textura discreta; nenhuma imagem externa é carregada. */}
      <div
        aria-hidden
        className="pointer-events-none absolute inset-0 opacity-[0.14]"
        style={{
          backgroundImage:
            'radial-gradient(circle at 1px 1px, currentColor 1px, transparent 0)',
          backgroundSize: '22px 22px',
        }}
      />
      <div
        aria-hidden
        className="pointer-events-none absolute -top-24 -right-24 size-[28rem] rounded-full bg-white/10 blur-3xl"
      />

      <div className="relative flex items-center gap-3">
        <span className="grid size-10 place-items-center rounded-xl bg-white/15 text-sm font-bold backdrop-blur">
          GP
        </span>
        <span className="text-[0.9375rem] font-semibold tracking-tight">Gestão de Pedidos</span>
      </div>

      <div className="relative max-w-md">
        <h2 className="text-[2rem] leading-tight font-semibold tracking-tight">
          Clientes, produtos e pedidos em um só lugar.
        </h2>
        <p className="mt-4 text-[0.9375rem] leading-relaxed opacity-85">
          Interface para a API REST em .NET com autorização por papéis, controle de estoque e
          trilha de erros rastreável.
        </p>

        <ul className="mt-8 grid gap-3">
          {destaques.map((texto) => (
            <li key={texto} className="flex items-start gap-2.5 text-sm opacity-90">
              <CheckCircle2 aria-hidden className="mt-0.5 size-4 shrink-0" />
              {texto}
            </li>
          ))}
        </ul>
      </div>

      <p className="relative text-xs opacity-70">
        Front-end React + TypeScript consumindo ASP.NET Core, EF Core e PostgreSQL.
      </p>
    </div>
  )
}
