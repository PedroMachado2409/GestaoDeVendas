import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Ban, CheckCircle2, ShieldCheck, TriangleAlert, UserCog, UserRound } from 'lucide-react'
import { useMemo, useState } from 'react'
import { toast } from 'sonner'

import { usuariosApi } from '@/api/recursos'
import type { Papel, Usuario } from '@/api/tipos'
import { useSessao } from '@/auth/useSessao'
import { CabecalhoDaPagina } from '@/componentes/layout/CabecalhoDaPagina'
import { Cartao } from '@/componentes/ui/Cartao'
import { Confirmacao } from '@/componentes/ui/Confirmacao'
import { EsqueletoDeTabela, EstadoDeErro, EstadoVazio } from '@/componentes/ui/Estados'
import { Etiqueta } from '@/componentes/ui/Etiqueta'
import { CampoDeBusca, Segmentos } from '@/componentes/ui/Filtros'
import { ItemDeMenu, Menu, SeparadorDeMenu } from '@/componentes/ui/Menu'
import { notificarErro } from '@/lib/notificacoes'
import { formatarData, iniciais, normalizarTexto } from '@/lib/utils'
import { Tabela, type Coluna } from '@/componentes/ui/Tabela'

type Filtro = 'todos' | 'admins' | 'vendedores' | 'inativos'

interface AcaoPendente {
  usuario: Usuario
  tipo: 'papel' | 'status'
  novoPapel?: Papel
}

export function Usuarios() {
  const { usuario: eu } = useSessao()
  const clienteDeConsultas = useQueryClient()

  const [busca, definirBusca] = useState('')
  const [filtro, definirFiltro] = useState<Filtro>('todos')
  const [pendente, definirPendente] = useState<AcaoPendente | null>(null)

  const consulta = useQuery({
    queryKey: ['usuarios'],
    queryFn: ({ signal }) => usuariosApi.listar(signal),
  })

  const executar = useMutation({
    mutationFn: async (acao: AcaoPendente) => {
      if (acao.tipo === 'papel' && acao.novoPapel) {
        return usuariosApi.alterarPapel(acao.usuario.id, acao.novoPapel)
      }
      return acao.usuario.ativo
        ? usuariosApi.inativar(acao.usuario.id)
        : usuariosApi.ativar(acao.usuario.id)
    },
    onSuccess: (_, acao) => {
      const mensagem =
        acao.tipo === 'papel'
          ? `${acao.usuario.nome} agora é ${acao.novoPapel}`
          : acao.usuario.ativo
            ? `${acao.usuario.nome} foi inativado`
            : `${acao.usuario.nome} foi ativado`

      toast.success(mensagem, {
        description: 'Os tokens anteriores desse usuário foram revogados pela API.',
      })
      definirPendente(null)
      void clienteDeConsultas.invalidateQueries({ queryKey: ['usuarios'] })
    },
    onError: (erro) => notificarErro(erro, 'Não foi possível concluir a alteração'),
  })

  const usuarios = consulta.data ?? []

  const contagens = useMemo(
    () => ({
      todos: usuarios.length,
      admins: usuarios.filter((u) => u.role === 'Admin').length,
      vendedores: usuarios.filter((u) => u.role === 'Vendedor').length,
      inativos: usuarios.filter((u) => !u.ativo).length,
    }),
    [usuarios],
  )

  const adminsAtivos = usuarios.filter((u) => u.role === 'Admin' && u.ativo).length

  const visiveis = useMemo(() => {
    const termo = normalizarTexto(busca)

    return usuarios.filter((usuario) => {
      if (filtro === 'admins' && usuario.role !== 'Admin') return false
      if (filtro === 'vendedores' && usuario.role !== 'Vendedor') return false
      if (filtro === 'inativos' && usuario.ativo) return false
      if (!termo) return true

      return (
        normalizarTexto(usuario.nome).includes(termo) ||
        normalizarTexto(usuario.email).includes(termo)
      )
    })
  }, [usuarios, busca, filtro])

  const colunas: Array<Coluna<Usuario>> = [
    {
      chave: 'nome',
      cabecalho: 'Usuário',
      valor: (usuario) => usuario.nome,
      larguraMinima: '13rem',
      celula: (usuario) => (
        // Largura máxima para o truncate do e-mail funcionar dentro da tabela.
        <div className="flex max-w-[22rem] min-w-0 items-center gap-3">
          <span
            aria-hidden
            className="grid size-8 shrink-0 place-items-center rounded-full bg-surface-3 text-[0.6875rem] font-semibold text-ink-soft"
          >
            {iniciais(usuario.nome)}
          </span>
          <div className="min-w-0">
            <p className="truncate font-medium text-ink">
              {usuario.nome}
              {usuario.id === eu?.id && (
                <span className="ml-1.5 text-[0.6875rem] font-normal text-ink-muted">(você)</span>
              )}
            </p>
            <p className="truncate text-[0.8125rem] text-ink-muted">{usuario.email}</p>
          </div>
        </div>
      ),
    },
    {
      chave: 'papel',
      cabecalho: 'Papel',
      valor: (usuario) => usuario.role,
      celula: (usuario) => (
        <Etiqueta tom={usuario.role === 'Admin' ? 'marca' : 'neutro'}>
          {usuario.role === 'Admin' ? (
            <ShieldCheck aria-hidden className="size-3" />
          ) : (
            <UserRound aria-hidden className="size-3" />
          )}
          {usuario.role}
        </Etiqueta>
      ),
    },
    {
      chave: 'cadastro',
      cabecalho: 'Desde',
      ocultarEmTelaPequena: true,
      valor: (usuario) => usuario.dataCadastro,
      celula: (usuario) => (
        <span className="numerico text-ink-soft">{formatarData(usuario.dataCadastro)}</span>
      ),
    },
    {
      chave: 'status',
      cabecalho: 'Status',
      valor: (usuario) => usuario.ativo,
      celula: (usuario) => (
        <Etiqueta tom={usuario.ativo ? 'positivo' : 'neutro'} comPonto>
          {usuario.ativo ? 'Ativo' : 'Inativo'}
        </Etiqueta>
      ),
    },
    {
      chave: 'acoes',
      cabecalho: <span className="sr-only">Ações</span>,
      alinhamento: 'fim',
      celula: (usuario) => {
        const ultimoAdmin = usuario.role === 'Admin' && usuario.ativo && adminsAtivos <= 1

        return (
          <Menu rotulo={`Ações de ${usuario.nome}`}>
            {(fechar) => (
              <>
                <ItemDeMenu
                  disabled={usuario.role === 'Admin' || !usuario.ativo}
                  icone={<ShieldCheck aria-hidden className="size-4" />}
                  onClick={() => {
                    fechar()
                    definirPendente({ usuario, tipo: 'papel', novoPapel: 'Admin' })
                  }}
                >
                  Promover a Admin
                </ItemDeMenu>
                <ItemDeMenu
                  disabled={usuario.role === 'Vendedor' || ultimoAdmin}
                  icone={<UserRound aria-hidden className="size-4" />}
                  onClick={() => {
                    fechar()
                    definirPendente({ usuario, tipo: 'papel', novoPapel: 'Vendedor' })
                  }}
                >
                  Tornar Vendedor
                </ItemDeMenu>
                <SeparadorDeMenu />
                <ItemDeMenu
                  tom={usuario.ativo ? 'perigo' : 'neutro'}
                  disabled={ultimoAdmin}
                  icone={
                    usuario.ativo ? (
                      <Ban aria-hidden className="size-4" />
                    ) : (
                      <CheckCircle2 aria-hidden className="size-4" />
                    )
                  }
                  onClick={() => {
                    fechar()
                    definirPendente({ usuario, tipo: 'status' })
                  }}
                >
                  {usuario.ativo ? 'Inativar acesso' : 'Reativar acesso'}
                </ItemDeMenu>
              </>
            )}
          </Menu>
        )
      },
    },
  ]

  const souEuMesmo = pendente?.usuario.id === eu?.id

  return (
    <>
      <CabecalhoDaPagina
        titulo="Usuários"
        descricao="Papéis e acesso à API. Alterar papel ou status revoga imediatamente os tokens já emitidos para o usuário."
      />

      <Cartao>
        <div className="flex flex-wrap items-center justify-between gap-3 border-b border-line px-4 py-3">
          <CampoDeBusca
            rotulo="Buscar usuários"
            placeholder="Nome ou e-mail"
            valor={busca}
            aoMudar={definirBusca}
          />
          <Segmentos
            rotulo="Filtrar usuários"
            valor={filtro}
            aoMudar={definirFiltro}
            opcoes={[
              { valor: 'todos', rotulo: 'Todos', contagem: contagens.todos },
              { valor: 'admins', rotulo: 'Admins', contagem: contagens.admins },
              { valor: 'vendedores', rotulo: 'Vendedores', contagem: contagens.vendedores },
              { valor: 'inativos', rotulo: 'Inativos', contagem: contagens.inativos },
            ]}
          />
        </div>

        {consulta.isPending ? (
          <EsqueletoDeTabela colunas={5} />
        ) : consulta.isError ? (
          <EstadoDeErro erro={consulta.error} aoTentarNovamente={() => void consulta.refetch()} />
        ) : (
          <Tabela
            legenda="Usuários da plataforma"
            itens={visiveis}
            colunas={colunas}
            chaveDoItem={(usuario) => usuario.id}
            ordenacaoInicial={{ chave: 'nome', direcao: 'crescente' }}
            vazio={
              <EstadoVazio
                icone={<UserCog aria-hidden className="size-5" />}
                titulo="Nenhum usuário corresponde ao filtro"
                descricao="Ajuste a busca ou volte para o filtro “Todos”."
              />
            }
          />
        )}
      </Cartao>

      <Confirmacao
        aberto={pendente !== null}
        titulo={
          pendente?.tipo === 'papel'
            ? `Alterar papel de ${pendente.usuario.nome}?`
            : pendente?.usuario.ativo
              ? `Inativar ${pendente.usuario.nome}?`
              : `Reativar ${pendente?.usuario.nome}?`
        }
        descricao={
          <div className="grid gap-3">
            <p>
              {pendente?.tipo === 'papel' ? (
                <>
                  O papel passa de <strong className="text-ink">{pendente.usuario.role}</strong> para{' '}
                  <strong className="text-ink">{pendente.novoPapel}</strong>.
                </>
              ) : pendente?.usuario.ativo ? (
                <>O usuário perde o acesso à API imediatamente.</>
              ) : (
                <>O usuário volta a poder autenticar.</>
              )}{' '}
              A API gera uma nova versão de token, invalidando as sessões abertas dessa conta.
            </p>

            {souEuMesmo && (
              <p className="flex items-start gap-2 rounded-lg border border-caution/30 bg-caution-soft px-3 py-2.5 text-xs text-caution-ink">
                <TriangleAlert aria-hidden className="mt-px size-3.5 shrink-0" />
                Esta é a sua própria conta. A alteração revoga o seu token e você será desconectado
                na próxima requisição.
              </p>
            )}
          </div>
        }
        rotuloConfirmar={pendente?.tipo === 'papel' ? 'Alterar papel' : 'Confirmar'}
        varianteConfirmar={
          pendente?.tipo === 'status' && pendente.usuario.ativo ? 'perigo' : 'primario'
        }
        processando={executar.isPending}
        aoConfirmar={() => pendente && executar.mutate(pendente)}
        aoCancelar={() => definirPendente(null)}
      />
    </>
  )
}
