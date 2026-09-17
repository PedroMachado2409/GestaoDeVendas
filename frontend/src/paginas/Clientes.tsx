import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { zodResolver } from '@hookform/resolvers/zod'
import { Ban, CheckCircle2, PencilLine, Plus, UserPlus, Users } from 'lucide-react'
import { useMemo, useState } from 'react'
import { useForm } from 'react-hook-form'
import { toast } from 'sonner'
import { z } from 'zod'

import { clientesApi } from '@/api/recursos'
import type { Cliente } from '@/api/tipos'
import { useSessao } from '@/auth/useSessao'
import { CabecalhoDaPagina } from '@/componentes/layout/CabecalhoDaPagina'
import { Botao } from '@/componentes/ui/Botao'
import { Cartao } from '@/componentes/ui/Cartao'
import { Campo, Entrada } from '@/componentes/ui/Campo'
import { Confirmacao } from '@/componentes/ui/Confirmacao'
import { EstadoDeErro, EstadoVazio, EsqueletoDeTabela } from '@/componentes/ui/Estados'
import { Etiqueta } from '@/componentes/ui/Etiqueta'
import { CampoDeBusca, Segmentos } from '@/componentes/ui/Filtros'
import { ItemDeMenu, Menu } from '@/componentes/ui/Menu'
import { Modal } from '@/componentes/ui/Modal'
import { Tabela, type Coluna } from '@/componentes/ui/Tabela'
import { tratarErroDeFormulario } from '@/lib/formulario'
import { notificarErro } from '@/lib/notificacoes'
import { formatarCpf, formatarData, normalizarTexto, somenteDigitos } from '@/lib/utils'
import { esquemaCpf, esquemaEmail } from '@/lib/validacoes'

const esquema = z.object({
  nome: z.string().trim().min(1, 'Informe o nome.'),
  email: esquemaEmail,
  cpf: esquemaCpf,
})

type Formulario = z.infer<typeof esquema>
type Filtro = 'todos' | 'ativos' | 'inativos'

export function Clientes() {
  const { ehAdmin } = useSessao()
  const clienteDeConsultas = useQueryClient()

  const [busca, definirBusca] = useState('')
  const [filtro, definirFiltro] = useState<Filtro>('todos')
  const [emEdicao, definirEmEdicao] = useState<Cliente | null>(null)
  const [formularioAberto, definirFormularioAberto] = useState(false)
  const [alvoDeStatus, definirAlvoDeStatus] = useState<Cliente | null>(null)

  const consulta = useQuery({
    queryKey: ['clientes'],
    queryFn: ({ signal }) => clientesApi.listar(signal),
  })

  const mudarStatus = useMutation({
    mutationFn: (cliente: Cliente) =>
      cliente.ativo ? clientesApi.inativar(cliente.id) : clientesApi.ativar(cliente.id),
    onSuccess: (_, cliente) => {
      toast.success(cliente.ativo ? 'Cliente inativado' : 'Cliente ativado', {
        description: cliente.nome,
      })
      definirAlvoDeStatus(null)
      void clienteDeConsultas.invalidateQueries({ queryKey: ['clientes'] })
    },
    onError: (erro) => notificarErro(erro, 'Não foi possível alterar o status'),
  })

  const clientes = consulta.data ?? []

  const contagens = useMemo(
    () => ({
      todos: clientes.length,
      ativos: clientes.filter((c) => c.ativo).length,
      inativos: clientes.filter((c) => !c.ativo).length,
    }),
    [clientes],
  )

  const visiveis = useMemo(() => {
    const termo = normalizarTexto(busca)
    const digitos = somenteDigitos(busca)

    return clientes.filter((cliente) => {
      if (filtro === 'ativos' && !cliente.ativo) return false
      if (filtro === 'inativos' && cliente.ativo) return false
      if (!termo) return true

      return (
        normalizarTexto(cliente.nome).includes(termo) ||
        normalizarTexto(cliente.email).includes(termo) ||
        (digitos.length > 0 && cliente.cpf.includes(digitos))
      )
    })
  }, [clientes, busca, filtro])

  function abrirCriacao() {
    definirEmEdicao(null)
    definirFormularioAberto(true)
  }

  function abrirEdicao(cliente: Cliente) {
    definirEmEdicao(cliente)
    definirFormularioAberto(true)
  }

  const colunas: Array<Coluna<Cliente>> = [
    {
      chave: 'nome',
      cabecalho: 'Cliente',
      valor: (cliente) => cliente.nome,
      larguraMinima: '12rem',
      celula: (cliente) => (
        // Largura máxima para o truncate do e-mail funcionar dentro da tabela.
        <div className="max-w-[20rem] min-w-0">
          <p className="font-medium text-pretty text-ink">{cliente.nome}</p>
          <p className="truncate text-[0.8125rem] text-ink-muted">{cliente.email}</p>
        </div>
      ),
    },
    {
      chave: 'cpf',
      cabecalho: 'CPF',
      valor: (cliente) => cliente.cpf,
      ocultarEmTelaPequena: true,
      celula: (cliente) => (
        <span className="numerico text-ink-soft">{formatarCpf(cliente.cpf)}</span>
      ),
    },
    {
      chave: 'cadastro',
      cabecalho: 'Cadastro',
      valor: (cliente) => cliente.dataCadastro,
      ocultarEmTelaPequena: true,
      celula: (cliente) => (
        <span className="numerico text-ink-soft">{formatarData(cliente.dataCadastro)}</span>
      ),
    },
    {
      chave: 'status',
      cabecalho: 'Status',
      valor: (cliente) => cliente.ativo,
      celula: (cliente) => (
        <Etiqueta tom={cliente.ativo ? 'positivo' : 'neutro'} comPonto>
          {cliente.ativo ? 'Ativo' : 'Inativo'}
        </Etiqueta>
      ),
    },
    {
      chave: 'acoes',
      cabecalho: <span className="sr-only">Ações</span>,
      alinhamento: 'fim',
      celula: (cliente) => (
        <Menu rotulo={`Ações de ${cliente.nome}`}>
          {(fechar) => (
            <>
              <ItemDeMenu
                icone={<PencilLine aria-hidden className="size-4" />}
                onClick={() => {
                  fechar()
                  abrirEdicao(cliente)
                }}
              >
                Editar dados
              </ItemDeMenu>
              <ItemDeMenu
                tom={cliente.ativo ? 'perigo' : 'neutro'}
                disabled={!ehAdmin}
                icone={
                  cliente.ativo ? (
                    <Ban aria-hidden className="size-4" />
                  ) : (
                    <CheckCircle2 aria-hidden className="size-4" />
                  )
                }
                onClick={() => {
                  fechar()
                  definirAlvoDeStatus(cliente)
                }}
              >
                {cliente.ativo ? 'Inativar' : 'Ativar'}
                {!ehAdmin && <span className="ml-auto text-[0.625rem] uppercase">admin</span>}
              </ItemDeMenu>
            </>
          )}
        </Menu>
      ),
    },
  ]

  return (
    <>
      <CabecalhoDaPagina
        titulo="Clientes"
        descricao="Cadastro de clientes elegíveis a pedidos. Um cliente inativo não pode receber pedidos novos."
        acoes={
          <Botao
            variante="primario"
            onClick={abrirCriacao}
            iconeInicial={<Plus aria-hidden className="size-4" />}
          >
            Novo cliente
          </Botao>
        }
      />

      <Cartao>
        <div className="flex flex-wrap items-center justify-between gap-3 border-b border-line px-4 py-3">
          <CampoDeBusca
            rotulo="Buscar clientes"
            placeholder="Nome, e-mail ou CPF"
            valor={busca}
            aoMudar={definirBusca}
          />
          <Segmentos
            rotulo="Filtrar por status"
            valor={filtro}
            aoMudar={definirFiltro}
            opcoes={[
              { valor: 'todos', rotulo: 'Todos', contagem: contagens.todos },
              { valor: 'ativos', rotulo: 'Ativos', contagem: contagens.ativos },
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
            legenda="Lista de clientes"
            itens={visiveis}
            colunas={colunas}
            chaveDoItem={(cliente) => cliente.id}
            ordenacaoInicial={{ chave: 'nome', direcao: 'crescente' }}
            vazio={
              clientes.length === 0 ? (
                <EstadoVazio
                  icone={<Users aria-hidden className="size-5" />}
                  titulo="Nenhum cliente cadastrado"
                  descricao="Cadastre o primeiro cliente para começar a registrar pedidos."
                  acao={
                    <Botao
                      variante="primario"
                      tamanho="pequeno"
                      onClick={abrirCriacao}
                      iconeInicial={<UserPlus aria-hidden className="size-3.5" />}
                    >
                      Cadastrar cliente
                    </Botao>
                  }
                />
              ) : (
                <EstadoVazio
                  icone={<Users aria-hidden className="size-5" />}
                  titulo="Nenhum cliente corresponde ao filtro"
                  descricao="Ajuste a busca ou volte para o filtro “Todos”."
                />
              )
            }
          />
        )}
      </Cartao>

      <FormularioDeCliente
        aberto={formularioAberto}
        cliente={emEdicao}
        aoFechar={() => definirFormularioAberto(false)}
      />

      <Confirmacao
        aberto={alvoDeStatus !== null}
        titulo={alvoDeStatus?.ativo ? 'Inativar cliente?' : 'Ativar cliente?'}
        descricao={
          alvoDeStatus?.ativo ? (
            <>
              <strong className="text-ink">{alvoDeStatus.nome}</strong> deixará de aceitar pedidos
              novos. Os pedidos já existentes não são afetados.
            </>
          ) : (
            <>
              <strong className="text-ink">{alvoDeStatus?.nome}</strong> volta a aceitar pedidos.
            </>
          )
        }
        rotuloConfirmar={alvoDeStatus?.ativo ? 'Inativar' : 'Ativar'}
        varianteConfirmar={alvoDeStatus?.ativo ? 'perigo' : 'primario'}
        processando={mudarStatus.isPending}
        aoConfirmar={() => alvoDeStatus && mudarStatus.mutate(alvoDeStatus)}
        aoCancelar={() => definirAlvoDeStatus(null)}
      />
    </>
  )
}

function FormularioDeCliente({
  aberto,
  cliente,
  aoFechar,
}: {
  aberto: boolean
  cliente: Cliente | null
  aoFechar: () => void
}) {
  const clienteDeConsultas = useQueryClient()
  const editando = cliente !== null

  const {
    register,
    handleSubmit,
    setError,
    reset,
    setValue,
    watch,
    formState: { errors, isSubmitting },
  } = useForm<Formulario>({
    resolver: zodResolver(esquema),
    values: {
      nome: cliente?.nome ?? '',
      email: cliente?.email ?? '',
      cpf: cliente ? formatarCpf(cliente.cpf) : '',
    },
  })

  const cpf = watch('cpf')

  async function enviar(dados: Formulario) {
    // A API normaliza o CPF para dígitos; enviar já sem máscara evita
    // divergência entre o que o usuário vê e o que fica gravado.
    const carga = { ...dados, cpf: somenteDigitos(dados.cpf) }

    try {
      if (editando) {
        await clientesApi.atualizar({ id: cliente.id, ...carga })
        toast.success('Cliente atualizado', { description: carga.nome })
      } else {
        await clientesApi.criar(carga)
        toast.success('Cliente cadastrado', { description: carga.nome })
      }

      await clienteDeConsultas.invalidateQueries({ queryKey: ['clientes'] })
      reset()
      aoFechar()
    } catch (erro) {
      tratarErroDeFormulario(erro, setError, ['nome', 'email', 'cpf'], 'Não foi possível salvar')
    }
  }

  return (
    <Modal
      aberto={aberto}
      aoFechar={aoFechar}
      titulo={editando ? 'Editar cliente' : 'Novo cliente'}
      descricao={
        editando
          ? 'As alterações valem para pedidos futuros; o histórico permanece.'
          : 'O CPF é validado no dispositivo e novamente pela API.'
      }
      largura="estreita"
      rodape={
        <>
          <Botao variante="sutil" onClick={aoFechar} disabled={isSubmitting}>
            Cancelar
          </Botao>
          <Botao
            variante="primario"
            carregando={isSubmitting}
            onClick={handleSubmit(enviar)}
            type="submit"
          >
            {editando ? 'Salvar alterações' : 'Cadastrar'}
          </Botao>
        </>
      }
    >
      <form onSubmit={handleSubmit(enviar)} className="grid gap-4" noValidate>
        <Campo rotulo="Nome" erro={errors.nome?.message} obrigatorio>
          {(propriedades) => (
            <Entrada {...propriedades} {...register('nome')} placeholder="Nome completo" autoFocus />
          )}
        </Campo>

        <Campo rotulo="E-mail" erro={errors.email?.message} obrigatorio>
          {(propriedades) => (
            <Entrada
              {...propriedades}
              {...register('email')}
              type="email"
              placeholder="cliente@email.com"
            />
          )}
        </Campo>

        <Campo rotulo="CPF" erro={errors.cpf?.message} obrigatorio>
          {(propriedades) => (
            <Entrada
              {...propriedades}
              {...register('cpf')}
              value={cpf}
              onChange={(evento) =>
                setValue('cpf', formatarCpf(evento.target.value), { shouldValidate: false })
              }
              inputMode="numeric"
              placeholder="000.000.000-00"
              className="numerico"
            />
          )}
        </Campo>
      </form>
    </Modal>
  )
}
