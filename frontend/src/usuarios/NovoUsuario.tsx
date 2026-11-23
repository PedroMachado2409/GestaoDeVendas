import { zodResolver } from '@hookform/resolvers/zod'
import { useQueryClient } from '@tanstack/react-query'
import { Eye, EyeOff } from 'lucide-react'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { toast } from 'sonner'
import { z } from 'zod'

import { usuariosApi } from '@/api/recursos'
import { useSessao } from '@/auth/useSessao'
import { Botao } from '@/componentes/ui/Botao'
import { Campo, Entrada } from '@/componentes/ui/Campo'
import { Modal } from '@/componentes/ui/Modal'
import { ErroApi } from '@/lib/erros'
import { tratarErroDeFormulario } from '@/lib/formulario'
import { esquemaEmail, esquemaSenhaNova } from '@/lib/validacoes'

const esquema = z.object({
  nome: z.string().trim().min(1, 'Informe o nome.').max(120, 'O nome deve ter no máximo 120 caracteres.'),
  email: esquemaEmail,
  senha: esquemaSenhaNova,
})

export function NovoUsuario({ aoFechar }: { aoFechar: () => void }) {
  const { ehAdmin } = useSessao()
  const consultas = useQueryClient()
  const [senhaVisivel, definirSenhaVisivel] = useState(false)
  const { register, handleSubmit, setError, formState: { errors, isSubmitting } } = useForm<z.infer<typeof esquema>>({
    resolver: zodResolver(esquema),
    defaultValues: { nome: '', email: '', senha: '' },
  })

  function fechar() {
    if (!isSubmitting) aoFechar()
  }

  async function enviar(dados: z.infer<typeof esquema>) {
    if (!ehAdmin) return
    try {
      const usuario = await usuariosApi.registrar(dados)
      toast.success('Usuário cadastrado', { description: usuario.nome })
      await consultas.invalidateQueries({ queryKey: ['usuarios'] })
      aoFechar()
    } catch (erro) {
      if (erro instanceof ErroApi && erro.status === 409) {
        setError('email', { type: 'server', message: 'Já existe um usuário com este e-mail.' })
      }
      tratarErroDeFormulario(erro, setError, ['nome', 'email', 'senha'], 'Não foi possível cadastrar o usuário')
    }
  }

  if (!ehAdmin) return null

  return (
    <Modal
      aberto
      aoFechar={fechar}
      titulo="Novo usuário"
      descricao="Cadastre os dados de acesso ao sistema."
      largura="estreita"
      rodape={
        <>
          <Botao variante="sutil" onClick={fechar} disabled={isSubmitting}>Cancelar</Botao>
          <Botao variante="primario" type="submit" form="novo-usuario" carregando={isSubmitting}>Cadastrar usuário</Botao>
        </>
      }
    >
      <form id="novo-usuario" onSubmit={handleSubmit(enviar)} className="grid gap-4" noValidate>
        <Campo rotulo="Nome" erro={errors.nome?.message} obrigatorio>
          {(propriedades) => <Entrada {...propriedades} {...register('nome')} maxLength={120} autoComplete="off" autoFocus />}
        </Campo>
        <Campo rotulo="E-mail" erro={errors.email?.message} obrigatorio>
          {(propriedades) => <Entrada {...propriedades} {...register('email')} type="email" maxLength={254} autoComplete="off" />}
        </Campo>
        <Campo rotulo="Senha" erro={errors.senha?.message} dica="Mínimo de 10 caracteres, com ao menos uma letra e um número." obrigatorio>
          {(propriedades) => (
            <Entrada
              {...propriedades}
              {...register('senha')}
              type={senhaVisivel ? 'text' : 'password'}
              autoComplete="new-password"
              maxLength={128}
              sufixo={
                <button type="button" onClick={() => definirSenhaVisivel((visivel) => !visivel)} aria-label={senhaVisivel ? 'Ocultar senha' : 'Mostrar senha'} className="grid size-9 place-items-center rounded-lg text-ink-muted hover:text-ink">
                  {senhaVisivel ? <EyeOff aria-hidden className="size-4" /> : <Eye aria-hidden className="size-4" />}
                </button>
              }
            />
          )}
        </Campo>
        <p className="rounded-lg border border-line bg-surface-2 px-3 py-2.5 text-xs text-ink-muted">
          Novos usuários recebem o perfil Vendedor. Você pode alterar o perfil após o cadastro.
        </p>
      </form>
    </Modal>
  )
}
