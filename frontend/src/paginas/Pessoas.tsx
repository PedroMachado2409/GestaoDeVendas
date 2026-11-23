import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { zodResolver } from '@hookform/resolvers/zod'
import { Ban, CheckCircle2, PencilLine, Plus, UserPlus, Users } from 'lucide-react'
import { useMemo, useState } from 'react'
import { useForm } from 'react-hook-form'
import { toast } from 'sonner'
import { z } from 'zod'

import { pessoasApi } from '@/api/recursos'
import type { Pessoa, TipoPessoa } from '@/api/tipos'
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
import { CATEGORIAS_PESSOA, obterCategoria, rotuloDaCategoria } from '@/pessoas/categorias'

const esquema = z.object({
  nome: z.string().trim().min(1, 'Informe o nome.').max(120, 'O nome deve ter no máximo 120 caracteres.'),
  email: esquemaEmail,
  cpf: esquemaCpf,
  tipoPessoa: z.enum(CATEGORIAS_PESSOA.map((categoria) => categoria.valor), {
    error: 'Selecione um tipo de pessoa válido.',
  }),
})

type Formulario = z.infer<typeof esquema>
type Filtro = 'todos' | 'ativos' | 'inativos'

const SEM_PESSOAS: Pessoa[] = []

export function Pessoas() {
  const { ehAdmin } = useSessao()
  const consultas = useQueryClient()

  const [busca, definirBusca] = useState('')
  const [filtro, definirFiltro] = useState<Filtro>('todos')
  const [categoria, definirCategoria] = useState<TipoPessoa | 'todas'>('todas')
  const [emEdicao, definirEmEdicao] = useState<Pessoa | null>(null)
  const [formularioAberto, definirFormularioAberto] = useState(false)
  const [alvoDeStatus, definirAlvoDeStatus] = useState<Pessoa | null>(null)

  const consulta = useQuery({
    queryKey: ['pessoas'],
    queryFn: ({ signal }) => pessoasApi.listar(signal),
  })

  const mudarStatus = useMutation({
    mutationFn: (pessoa: Pessoa) =>
      pessoa.ativo ? pessoasApi.inativar(pessoa.id) : pessoasApi.ativar(pessoa.id),
    onSuccess: (_, pessoa) => {
      toast.success(pessoa.ativo ? 'Pessoa inativada' : 'Pessoa ativada', {
        description: pessoa.nome,
      })
      definirAlvoDeStatus(null)
      void consultas.invalidateQueries({ queryKey: ['pessoas'] })
    },
    onError: (erro) => notificarErro(erro, 'Não foi possível alterar o status'),
  })

  const pessoas = consulta.data ?? SEM_PESSOAS

  const pessoasDaCategoria = useMemo(
    () => categoria === 'todas'
      ? pessoas
      : pessoas.filter((pessoa) => obterCategoria(pessoa.tipoPessoa) === categoria),
    [pessoas, categoria],
  )

  const contagens = useMemo(
    () => ({
      todos: pessoasDaCategoria.length,
      ativos: pessoasDaCategoria.filter((pessoa) => pessoa.ativo).length,
      inativos: pessoasDaCategoria.filter((pessoa) => !pessoa.ativo).length,
    }),
    [pessoasDaCategoria],
  )

  const visiveis = useMemo(() => {
    const termo = normalizarTexto(busca)
    const digitos = somenteDigitos(busca)

    return pessoasDaCategoria.filter((pessoa) => {
      if (filtro === 'ativos' && !pessoa.ativo) return false
      if (filtro === 'inativos' && pessoa.ativo) return false
      if (!termo) return true

      return (
        normalizarTexto(pessoa.nome).includes(termo) ||
        normalizarTexto(pessoa.email).includes(termo) ||
        (digitos.length > 0 && pessoa.cpf.includes(digitos))
      )
    })
  }, [pessoasDaCategoria, busca, filtro])

  function abrirCriacao() {
    definirEmEdicao(null)
    definirFormularioAberto(true)
  }

  function abrirEdicao(pessoa: Pessoa) {
    definirEmEdicao(pessoa)
    definirFormularioAberto(true)
  }

  const colunas: Array<Coluna<Pessoa>> = [
    {
      chave: 'nome',
      cabecalho: 'Pessoa',
      valor: (pessoa) => pessoa.nome,
      larguraMinima: '12rem',
      celula: (pessoa) => (
        // Largura máxima para o truncate do e-mail funcionar dentro da tabela.
        <div className="max-w-[20rem] min-w-0">
          <p className="font-medium text-pretty text-ink">{pessoa.nome}</p>
          <p className="truncate text-[0.8125rem] text-ink-muted">{pessoa.email}</p>
        </div>
      ),
    },
    {
      chave: 'categoria',
      cabecalho: 'Categoria',
      valor: (pessoa) => rotuloDaCategoria(pessoa.tipoPessoa),
      celula: (pessoa) => <Etiqueta>{rotuloDaCategoria(pessoa.tipoPessoa)}</Etiqueta>,
    },
    {
      chave: 'cpf',
      cabecalho: 'CPF',
      valor: (pessoa) => pessoa.cpf,
      ocultarEmTelaPequena: true,
      celula: (pessoa) => (
        <span className="numerico text-ink-soft">{formatarCpf(pessoa.cpf)}</span>
      ),
    },
    {
      chave: 'cadastro',
      cabecalho: 'Cadastro',
      valor: (pessoa) => pessoa.dataCadastro,
      ocultarEmTelaPequena: true,
      celula: (pessoa) => (
        <span className="numerico text-ink-soft">{formatarData(pessoa.dataCadastro)}</span>
      ),
    },
    {
      chave: 'status',
      cabecalho: 'Status',
      valor: (pessoa) => pessoa.ativo,
      celula: (pessoa) => (
        <Etiqueta tom={pessoa.ativo ? 'positivo' : 'neutro'} comPonto>
          {pessoa.ativo ? 'Ativa' : 'Inativa'}
        </Etiqueta>
      ),
    },
    {
      chave: 'acoes',
      cabecalho: <span className="sr-only">Ações</span>,
      alinhamento: 'fim',
      celula: (pessoa) => (
        <Menu rotulo={`Ações de ${pessoa.nome}`}>
          {(fechar) => (
            <>
              <ItemDeMenu
                icone={<PencilLine aria-hidden className="size-4" />}
                onClick={() => {
                  fechar()
                  abrirEdicao(pessoa)
                }}
              >
                Editar dados
              </ItemDeMenu>
              <ItemDeMenu
                tom={pessoa.ativo ? 'perigo' : 'neutro'}
                disabled={!ehAdmin}
                icone={
                  pessoa.ativo ? (
                    <Ban aria-hidden className="size-4" />
                  ) : (
                    <CheckCircle2 aria-hidden className="size-4" />
                  )
                }
                onClick={() => {
                  fechar()
                  definirAlvoDeStatus(pessoa)
                }}
              >
                {pessoa.ativo ? 'Inativar' : 'Ativar'}
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
        titulo="Pessoas"
        descricao="Cadastre pessoas e mantenha seus dados de identificação e contato atualizados."
        acoes={
          <Botao
            variante="primario"
            onClick={abrirCriacao}
            iconeInicial={<Plus aria-hidden className="size-4" />}
          >
            Nova pessoa
          </Botao>
        }
      />

      <Cartao>
        <div className="flex flex-wrap items-center justify-between gap-3 border-b border-line px-4 py-3">
          <CampoDeBusca
            rotulo="Buscar pessoas"
            placeholder="Nome, e-mail ou CPF"
            valor={busca}
            aoMudar={definirBusca}
          />
          <label className="flex items-center gap-2 text-[0.8125rem] font-medium text-ink-soft">
            Categoria
            <select
              value={categoria}
              onChange={(evento) => definirCategoria(evento.target.value as TipoPessoa | 'todas')}
              className="h-9.5 min-w-40 rounded-lg border border-line bg-surface px-3 text-sm text-ink outline-none hover:border-line-strong focus:border-brand"
            >
              <option value="todas">Todas as categorias</option>
              {CATEGORIAS_PESSOA.map((opcao) => (
                <option key={opcao.valor} value={opcao.valor}>{opcao.rotulo}</option>
              ))}
            </select>
          </label>
          <Segmentos
            rotulo="Filtrar por status"
            valor={filtro}
            aoMudar={definirFiltro}
            opcoes={[
              { valor: 'todos', rotulo: 'Todas', contagem: contagens.todos },
              { valor: 'ativos', rotulo: 'Ativas', contagem: contagens.ativos },
              { valor: 'inativos', rotulo: 'Inativas', contagem: contagens.inativos },
            ]}
          />
        </div>

        {consulta.isPending ? (
          <EsqueletoDeTabela colunas={6} />
        ) : consulta.isError ? (
          <EstadoDeErro erro={consulta.error} aoTentarNovamente={() => void consulta.refetch()} />
        ) : (
          <Tabela
            legenda="Lista de pessoas"
            itens={visiveis}
            colunas={colunas}
            chaveDoItem={(pessoa) => pessoa.id}
            ordenacaoInicial={{ chave: 'nome', direcao: 'crescente' }}
            vazio={
              pessoas.length === 0 ? (
                <EstadoVazio
                  icone={<Users aria-hidden className="size-5" />}
                  titulo="Nenhuma pessoa cadastrada"
                  descricao="Cadastre a primeira pessoa com nome, e-mail e CPF."
                  acao={
                    <Botao
                      variante="primario"
                      tamanho="pequeno"
                      onClick={abrirCriacao}
                      iconeInicial={<UserPlus aria-hidden className="size-3.5" />}
                    >
                      Cadastrar pessoa
                    </Botao>
                  }
                />
              ) : (
                <EstadoVazio
                  icone={<Users aria-hidden className="size-5" />}
                  titulo="Nenhuma pessoa corresponde ao filtro"
                  descricao="Ajuste a busca, a categoria ou o status para encontrar pessoas."
                />
              )
            }
          />
        )}
      </Cartao>

      {formularioAberto && (
        <FormularioDePessoa
          aberto={formularioAberto}
          pessoa={emEdicao}
          aoFechar={() => definirFormularioAberto(false)}
        />
      )}

      <Confirmacao
        aberto={alvoDeStatus !== null}
        titulo={alvoDeStatus?.ativo ? 'Inativar pessoa?' : 'Ativar pessoa?'}
        descricao={
          alvoDeStatus?.ativo ? (
            <>
              <strong className="text-ink">{alvoDeStatus.nome}</strong> ficará marcada como inativa.
              Os dados continuarão disponíveis para consulta.
            </>
          ) : (
            <>
              <strong className="text-ink">{alvoDeStatus?.nome}</strong> voltará a ficar ativa no cadastro.
            </>
          )
        }
        rotuloConfirmar={alvoDeStatus?.ativo ? 'Inativar' : 'Ativar'}
        varianteConfirmar={alvoDeStatus?.ativo ? 'perigo' : 'primario'}
        processando={mudarStatus.isPending}
        aoConfirmar={() => alvoDeStatus && mudarStatus.mutate(alvoDeStatus)}
        aoCancelar={() => {
          if (!mudarStatus.isPending) definirAlvoDeStatus(null)
        }}
      />
    </>
  )
}

function FormularioDePessoa({
  aberto,
  pessoa,
  aoFechar,
}: {
  aberto: boolean
  pessoa: Pessoa | null
  aoFechar: () => void
}) {
  const consultas = useQueryClient()
  const editando = pessoa !== null

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
    defaultValues: {
      nome: pessoa?.nome ?? '',
      email: pessoa?.email ?? '',
      cpf: pessoa ? formatarCpf(pessoa.cpf) : '',
      tipoPessoa: pessoa ? obterCategoria(pessoa.tipoPessoa) ?? undefined : 'Cliente',
    },
  })

  const cpf = watch('cpf')

  function fechar() {
    if (!isSubmitting) aoFechar()
  }

  async function enviar(dados: Formulario) {
    // A API normaliza o CPF para dígitos; enviar já sem máscara evita
    // divergência entre o que o usuário vê e o que fica gravado.
    const carga = { ...dados, cpf: somenteDigitos(dados.cpf) }

    try {
      if (editando) {
        await pessoasApi.atualizar({ id: pessoa.id, ...carga })
        toast.success('Pessoa atualizada', { description: carga.nome })
      } else {
        await pessoasApi.criar(carga)
        toast.success('Pessoa cadastrada', { description: carga.nome })
      }

      await consultas.invalidateQueries({ queryKey: ['pessoas'] })
      reset()
      aoFechar()
    } catch (erro) {
      tratarErroDeFormulario(erro, setError, ['nome', 'email', 'cpf', 'tipoPessoa'], 'Não foi possível salvar')
    }
  }

  return (
    <Modal
      aberto={aberto}
      aoFechar={fechar}
      titulo={editando ? 'Editar pessoa' : 'Nova pessoa'}
      descricao={
        editando
          ? 'Mantenha os dados de identificação e contato atualizados.'
          : 'Informe os dados da pessoa e selecione sua categoria.'
      }
      largura="estreita"
      rodape={
        <>
          <Botao variante="sutil" onClick={fechar} disabled={isSubmitting}>
            Cancelar
          </Botao>
          <Botao
            variante="primario"
            carregando={isSubmitting}
            type="submit"
            form="formulario-pessoa"
          >
            {editando ? 'Salvar alterações' : 'Cadastrar'}
          </Botao>
        </>
      }
    >
      <form id="formulario-pessoa" onSubmit={handleSubmit(enviar)} className="grid gap-4" noValidate>
        <Campo rotulo="Tipo de pessoa" erro={errors.tipoPessoa?.message} obrigatorio>
          {(propriedades) => (
            <select
              {...propriedades}
              {...register('tipoPessoa')}
              className="h-9.5 w-full rounded-lg border border-line bg-surface px-3 text-sm text-ink outline-none hover:border-line-strong focus:border-brand aria-invalid:border-negative"
            >
              <option value="" disabled>Selecione o tipo de pessoa</option>
              {CATEGORIAS_PESSOA.map((categoria) => (
                <option key={categoria.valor} value={categoria.valor}>{categoria.rotulo}</option>
              ))}
            </select>
          )}
        </Campo>

        <Campo rotulo="Nome" erro={errors.nome?.message} obrigatorio>
          {(propriedades) => (
            <Entrada {...propriedades} {...register('nome')} placeholder="Nome completo" maxLength={120} autoFocus />
          )}
        </Campo>

        <Campo rotulo="E-mail" erro={errors.email?.message} obrigatorio>
          {(propriedades) => (
            <Entrada
              {...propriedades}
              {...register('email')}
              type="email"
              maxLength={254}
              placeholder="pessoa@email.com"
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
