import { zodResolver } from '@hookform/resolvers/zod'
import { useQueryClient } from '@tanstack/react-query'
import { Eye, EyeOff, KeyRound, ShieldCheck, TriangleAlert, UserRound } from 'lucide-react'
import { useEffect, useRef, useState } from 'react'
import { useForm } from 'react-hook-form'
import { useSearchParams } from 'react-router-dom'
import { toast } from 'sonner'
import { z } from 'zod'

import { usuariosApi } from '@/api/recursos'
import { useSessao } from '@/auth/useSessao'
import { CabecalhoDaPagina } from '@/componentes/layout/CabecalhoDaPagina'
import { Botao } from '@/componentes/ui/Botao'
import { Campo, Entrada } from '@/componentes/ui/Campo'
import { CabecalhoDoCartao, Cartao, CorpoDoCartao } from '@/componentes/ui/Cartao'
import { Etiqueta } from '@/componentes/ui/Etiqueta'
import { tratarErroDeFormulario } from '@/lib/formulario'
import { formatarData, iniciais } from '@/lib/utils'
import { esquemaEmail, esquemaSenhaNova, forcaDaSenha } from '@/lib/validacoes'
import { cn } from '@/lib/utils'

const esquemaPerfil = z.object({
  nome: z.string().trim().min(1, 'Informe o nome.').max(120, 'Máximo de 120 caracteres.'),
  email: esquemaEmail,
})

const esquemaSenha = z
  .object({
    senhaAntiga: z.string().min(1, 'Informe a senha atual.').max(128),
    novaSenha: esquemaSenhaNova,
    confirmacao: z.string().min(1, 'Repita a nova senha.'),
  })
  .refine((dados) => dados.novaSenha !== dados.senhaAntiga, {
    path: ['novaSenha'],
    message: 'A nova senha deve ser diferente da senha atual.',
  })
  .refine((dados) => dados.novaSenha === dados.confirmacao, {
    path: ['confirmacao'],
    message: 'A confirmação não confere com a nova senha.',
  })

export function Perfil() {
  const { usuario, ehAdmin } = useSessao()
  const [parametros] = useSearchParams()
  const secaoSenha = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (parametros.get('secao') === 'senha') {
      secaoSenha.current?.scrollIntoView({ behavior: 'smooth', block: 'center' })
    }
  }, [parametros])

  if (!usuario) return null

  return (
    <>
      <CabecalhoDaPagina
        titulo="Meu perfil"
        descricao="Dados da sua conta na API. O papel só pode ser alterado por um administrador."
      />

      <div className="grid grid-cols-1 gap-5 sm:gap-6 lg:grid-cols-[20rem_minmax(0,1fr)] lg:items-start">
        <Cartao>
          <CorpoDoCartao className="flex flex-col items-center gap-3 text-center">
            <span
              aria-hidden
              className="grid size-16 place-items-center rounded-full bg-brand-soft text-lg font-semibold text-brand-ink"
            >
              {iniciais(usuario.nome)}
            </span>
            <div>
              <p className="font-semibold text-ink">{usuario.nome}</p>
              <p className="text-[0.8125rem] break-all text-ink-muted">{usuario.email}</p>
            </div>
            <Etiqueta tom={ehAdmin ? 'marca' : 'neutro'}>
              {ehAdmin ? (
                <ShieldCheck aria-hidden className="size-3" />
              ) : (
                <UserRound aria-hidden className="size-3" />
              )}
              {usuario.role}
            </Etiqueta>

            <dl className="mt-2 grid w-full gap-2 border-t border-line pt-4 text-left text-[0.8125rem]">
              <div className="flex items-center justify-between gap-3">
                <dt className="text-ink-muted">Identificador</dt>
                <dd className="numerico text-ink-soft">#{usuario.id}</dd>
              </div>
              <div className="flex items-center justify-between gap-3">
                <dt className="text-ink-muted">Cadastro</dt>
                <dd className="numerico text-ink-soft">{formatarData(usuario.dataCadastro)}</dd>
              </div>
              <div className="flex items-center justify-between gap-3">
                <dt className="text-ink-muted">Situação</dt>
                <dd>
                  <Etiqueta tom={usuario.ativo ? 'positivo' : 'negativo'} comPonto>
                    {usuario.ativo ? 'Ativo' : 'Inativo'}
                  </Etiqueta>
                </dd>
              </div>
            </dl>
          </CorpoDoCartao>
        </Cartao>

        <div className="grid grid-cols-1 gap-5 sm:gap-6">
          <FormularioDeDados nome={usuario.nome} email={usuario.email} />
          <div ref={secaoSenha}>
            <FormularioDeSenha />
          </div>
        </div>
      </div>
    </>
  )
}

function FormularioDeDados({ nome, email }: { nome: string; email: string }) {
  const clienteDeConsultas = useQueryClient()

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting, isDirty },
  } = useForm<z.infer<typeof esquemaPerfil>>({
    resolver: zodResolver(esquemaPerfil),
    values: { nome, email },
  })

  async function enviar(dados: z.infer<typeof esquemaPerfil>) {
    try {
      await usuariosApi.atualizarPerfil(dados)
      toast.success('Perfil atualizado')
      await clienteDeConsultas.invalidateQueries({ queryKey: ['usuario-autenticado'] })
      await clienteDeConsultas.invalidateQueries({ queryKey: ['usuarios'] })
    } catch (erro) {
      tratarErroDeFormulario(erro, setError, ['nome', 'email'], 'Não foi possível salvar o perfil')
    }
  }

  return (
    <Cartao>
      <CabecalhoDoCartao
        titulo="Dados da conta"
        descricao="Nome e e-mail usados para autenticar na API."
      />
      <form onSubmit={handleSubmit(enviar)} noValidate>
        <CorpoDoCartao className="grid gap-4 sm:grid-cols-2">
          <Campo rotulo="Nome" erro={errors.nome?.message} obrigatorio>
            {(propriedades) => <Entrada {...propriedades} {...register('nome')} />}
          </Campo>
          <Campo rotulo="E-mail" erro={errors.email?.message} obrigatorio>
            {(propriedades) => <Entrada {...propriedades} {...register('email')} type="email" />}
          </Campo>
        </CorpoDoCartao>
        <div className="flex justify-end border-t border-line bg-surface-2/50 px-5 py-3.5">
          <Botao
            type="submit"
            variante="primario"
            carregando={isSubmitting}
            disabled={!isDirty}
          >
            Salvar alterações
          </Botao>
        </div>
      </form>
    </Cartao>
  )
}

function FormularioDeSenha() {
  const { sair } = useSessao()
  const [visivel, definirVisivel] = useState(false)

  const {
    register,
    handleSubmit,
    setError,
    reset,
    watch,
    formState: { errors, isSubmitting },
  } = useForm<z.infer<typeof esquemaSenha>>({
    resolver: zodResolver(esquemaSenha),
    defaultValues: { senhaAntiga: '', novaSenha: '', confirmacao: '' },
  })

  const novaSenha = watch('novaSenha')
  const forca = forcaDaSenha(novaSenha)

  async function enviar({ senhaAntiga, novaSenha: nova }: z.infer<typeof esquemaSenha>) {
    try {
      await usuariosApi.atualizarSenha({ senhaAntiga, novaSenha: nova })
      reset()
      toast.success('Senha alterada', {
        description: 'A API revogou os tokens anteriores. Entre novamente com a nova senha.',
      })
      // A troca de senha invalida o token em uso; permanecer na tela só
      // levaria ao próximo 401.
      sair('manual')
    } catch (erro) {
      tratarErroDeFormulario(
        erro,
        setError,
        ['senhaAntiga', 'novaSenha'],
        'Não foi possível alterar a senha',
      )
    }
  }

  const alternador = (
    <button
      type="button"
      onClick={() => definirVisivel((v) => !v)}
      aria-label={visivel ? 'Ocultar senhas' : 'Mostrar senhas'}
      className="grid size-9 place-items-center rounded-lg text-ink-muted transition-colors hover:text-ink"
    >
      {visivel ? <EyeOff aria-hidden className="size-4" /> : <Eye aria-hidden className="size-4" />}
    </button>
  )

  return (
    <Cartao>
      <CabecalhoDoCartao
        titulo="Alterar senha"
        descricao="Mínimo de 10 caracteres, com ao menos uma letra e um número."
      />
      <form onSubmit={handleSubmit(enviar)} noValidate>
        <CorpoDoCartao className="grid gap-4">
          <div className="flex items-start gap-2.5 rounded-lg border border-caution/30 bg-caution-soft px-3 py-2.5 text-xs text-caution-ink">
            <TriangleAlert aria-hidden className="mt-px size-3.5 shrink-0" />
            <p>
              Trocar a senha revoga todos os tokens ativos desta conta. Você será desconectado e
              precisará entrar de novo.
            </p>
          </div>

          <Campo rotulo="Senha atual" erro={errors.senhaAntiga?.message} obrigatorio>
            {(propriedades) => (
              <Entrada
                {...propriedades}
                {...register('senhaAntiga')}
                type={visivel ? 'text' : 'password'}
                autoComplete="current-password"
                prefixo={<KeyRound aria-hidden className="size-4" />}
                sufixo={alternador}
              />
            )}
          </Campo>

          <div className="grid gap-4 sm:grid-cols-2">
            <Campo rotulo="Nova senha" erro={errors.novaSenha?.message} obrigatorio>
              {(propriedades) => (
                <Entrada
                  {...propriedades}
                  {...register('novaSenha')}
                  type={visivel ? 'text' : 'password'}
                  autoComplete="new-password"
                />
              )}
            </Campo>
            <Campo rotulo="Repetir nova senha" erro={errors.confirmacao?.message} obrigatorio>
              {(propriedades) => (
                <Entrada
                  {...propriedades}
                  {...register('confirmacao')}
                  type={visivel ? 'text' : 'password'}
                  autoComplete="new-password"
                />
              )}
            </Campo>
          </div>

          {novaSenha.length > 0 && (
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
              <span className="w-16 text-right text-[0.6875rem] text-ink-muted">
                {forca.rotulo}
              </span>
            </div>
          )}
        </CorpoDoCartao>

        <div className="flex justify-end border-t border-line bg-surface-2/50 px-5 py-3.5">
          <Botao type="submit" variante="primario" carregando={isSubmitting}>
            Alterar senha
          </Botao>
        </div>
      </form>
    </Cartao>
  )
}
