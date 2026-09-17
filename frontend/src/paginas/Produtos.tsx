import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  Ban,
  CheckCircle2,
  History,
  Package,
  PackagePlus,
  PencilLine,
  Plus,
  ShieldAlert,
  SlidersHorizontal,
} from 'lucide-react'
import { useMemo, useState } from 'react'
import { useForm } from 'react-hook-form'
import { useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { z } from 'zod'

import { produtosApi } from '@/api/recursos'
import type { Produto } from '@/api/tipos'
import { useSessao } from '@/auth/useSessao'
import { CabecalhoDaPagina } from '@/componentes/layout/CabecalhoDaPagina'
import { Botao } from '@/componentes/ui/Botao'
import { Campo, Entrada } from '@/componentes/ui/Campo'
import { Cartao } from '@/componentes/ui/Cartao'
import { Confirmacao } from '@/componentes/ui/Confirmacao'
import { EsqueletoDeTabela, EstadoDeErro, EstadoVazio } from '@/componentes/ui/Estados'
import { Etiqueta } from '@/componentes/ui/Etiqueta'
import { CampoDeBusca, Segmentos } from '@/componentes/ui/Filtros'
import { ItemDeMenu, Menu, SeparadorDeMenu } from '@/componentes/ui/Menu'
import { Modal } from '@/componentes/ui/Modal'
import { Tabela, type Coluna } from '@/componentes/ui/Tabela'
import { tratarErroDeFormulario } from '@/lib/formulario'
import { notificarErro } from '@/lib/notificacoes'
import { formatarMoeda, formatarNumero, normalizarTexto } from '@/lib/utils'
import { AjusteDeEstoque } from '@/produtos/AjusteDeEstoque'

// Os campos numéricos ficam como texto no formulário (é o que <input> entrega)
// e são convertidos só na hora de montar a carga — assim o campo vazio vira
// "informe o valor" em vez de NaN.
const esquema = z.object({
  nome: z.string().trim().min(1, 'Informe o nome do produto.'),
  marca: z.string().trim().min(1, 'Informe a marca.'),
  preco: z
    .string()
    .trim()
    .min(1, 'Informe o preço.')
    .refine((valor) => Number.isFinite(Number(valor)), 'Preço inválido.')
    .refine((valor) => Number(valor) > 0, 'O preço deve ser maior que zero.'),
  estoque: z
    .string()
    .trim()
    .min(1, 'Informe o estoque.')
    .refine((valor) => Number.isInteger(Number(valor)), 'O estoque deve ser um número inteiro.')
    .refine((valor) => Number(valor) >= 0, 'O estoque não pode ser negativo.'),
})

type Formulario = z.infer<typeof esquema>
type Filtro = 'todos' | 'ativos' | 'inativos' | 'semEstoque'

export function Produtos() {
  const { ehAdmin } = useSessao()
  const clienteDeConsultas = useQueryClient()

  const [busca, definirBusca] = useState('')
  const [filtro, definirFiltro] = useState<Filtro>('todos')
  const [emEdicao, definirEmEdicao] = useState<Produto | null>(null)
  const [formularioAberto, definirFormularioAberto] = useState(false)
  const [alvoDeStatus, definirAlvoDeStatus] = useState<Produto | null>(null)
  const [emAjuste, definirEmAjuste] = useState<Produto | null>(null)
  const navegar = useNavigate()

  const consulta = useQuery({
    queryKey: ['produtos'],
    queryFn: ({ signal }) => produtosApi.listar(signal),
  })

  const mudarStatus = useMutation({
    mutationFn: (produto: Produto) =>
      produto.ativo ? produtosApi.inativar(produto.id) : produtosApi.ativar(produto.id),
    onSuccess: (_, produto) => {
      toast.success(produto.ativo ? 'Produto inativado' : 'Produto ativado', {
        description: produto.nome,
      })
      definirAlvoDeStatus(null)
      void clienteDeConsultas.invalidateQueries({ queryKey: ['produtos'] })
    },
    onError: (erro) => notificarErro(erro, 'Não foi possível alterar o status'),
  })

  const produtos = consulta.data ?? []

  const contagens = useMemo(
    () => ({
      todos: produtos.length,
      ativos: produtos.filter((p) => p.ativo).length,
      inativos: produtos.filter((p) => !p.ativo).length,
      semEstoque: produtos.filter((p) => p.ativo && p.estoque === 0).length,
    }),
    [produtos],
  )

  const visiveis = useMemo(() => {
    const termo = normalizarTexto(busca)

    return produtos.filter((produto) => {
      if (filtro === 'ativos' && !produto.ativo) return false
      if (filtro === 'inativos' && produto.ativo) return false
      if (filtro === 'semEstoque' && !(produto.ativo && produto.estoque === 0)) return false
      if (!termo) return true

      return (
        normalizarTexto(produto.nome).includes(termo) ||
        normalizarTexto(produto.marca).includes(termo)
      )
    })
  }, [produtos, busca, filtro])

  const colunas: Array<Coluna<Produto>> = [
    {
      chave: 'nome',
      cabecalho: 'Produto',
      valor: (produto) => produto.nome,
      larguraMinima: '12rem',
      celula: (produto) => (
        // Quebra em vez de truncate: numa tabela, truncate não encolhe a
        // coluna, e o nome longo alargava a tabela inteira.
        <div className="max-w-[22rem] min-w-0">
          <p className="font-medium text-pretty text-ink">{produto.nome}</p>
          <p className="text-[0.8125rem] text-ink-muted">{produto.marca}</p>
        </div>
      ),
    },
    {
      chave: 'preco',
      cabecalho: 'Preço',
      alinhamento: 'fim',
      valor: (produto) => produto.preco,
      celula: (produto) => (
        <span className="numerico font-medium text-ink">{formatarMoeda(produto.preco)}</span>
      ),
    },
    {
      chave: 'estoque',
      cabecalho: 'Disponível',
      alinhamento: 'fim',
      valor: (produto) => produto.estoque,
      celula: (produto) => (
        <span
          className={
            produto.estoque === 0
              ? 'numerico font-medium text-negative-ink'
              : 'numerico text-ink-soft'
          }
        >
          {formatarNumero(produto.estoque)}
        </span>
      ),
    },
    {
      chave: 'reservado',
      cabecalho: 'Reservado',
      alinhamento: 'fim',
      ocultarEmTelaPequena: true,
      valor: (produto) => produto.quantidadeReservada,
      celula: (produto) => (
        <span className="numerico text-ink-muted">
          {formatarNumero(produto.quantidadeReservada)}
        </span>
      ),
    },
    {
      chave: 'status',
      cabecalho: 'Status',
      valor: (produto) => produto.ativo,
      celula: (produto) =>
        !produto.ativo ? (
          <Etiqueta tom="neutro" comPonto>
            Inativo
          </Etiqueta>
        ) : produto.estoque === 0 ? (
          <Etiqueta tom="negativo" comPonto>
            Sem estoque
          </Etiqueta>
        ) : (
          <Etiqueta tom="positivo" comPonto>
            Ativo
          </Etiqueta>
        ),
    },
    {
      chave: 'acoes',
      cabecalho: <span className="sr-only">Ações</span>,
      alinhamento: 'fim',
      celula: (produto) => (
        <Menu rotulo={`Ações de ${produto.nome}`}>
          {(fechar) => (
            <>
              <ItemDeMenu
                disabled={!ehAdmin}
                icone={<PencilLine aria-hidden className="size-4" />}
                onClick={() => {
                  fechar()
                  definirEmEdicao(produto)
                  definirFormularioAberto(true)
                }}
              >
                Editar
              </ItemDeMenu>
              <ItemDeMenu
                disabled={!ehAdmin}
                icone={<SlidersHorizontal aria-hidden className="size-4" />}
                onClick={() => {
                  fechar()
                  definirEmAjuste(produto)
                }}
              >
                Ajustar estoque
              </ItemDeMenu>
              <ItemDeMenu
                disabled={!ehAdmin}
                icone={<History aria-hidden className="size-4" />}
                onClick={() => {
                  fechar()
                  navegar(`/estoque?produto=${produto.id}`)
                }}
              >
                Ver movimentações
              </ItemDeMenu>
              <SeparadorDeMenu />
              <ItemDeMenu
                disabled={!ehAdmin}
                tom={produto.ativo ? 'perigo' : 'neutro'}
                icone={
                  produto.ativo ? (
                    <Ban aria-hidden className="size-4" />
                  ) : (
                    <CheckCircle2 aria-hidden className="size-4" />
                  )
                }
                onClick={() => {
                  fechar()
                  definirAlvoDeStatus(produto)
                }}
              >
                {produto.ativo ? 'Inativar' : 'Ativar'}
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
        titulo="Produtos"
        descricao="Catálogo e estoque. A quantidade reservada pertence a pedidos abertos e volta ao disponível se o pedido for cancelado."
        acoes={
          ehAdmin && (
            <Botao
              variante="primario"
              onClick={() => {
                definirEmEdicao(null)
                definirFormularioAberto(true)
              }}
              iconeInicial={<Plus aria-hidden className="size-4" />}
            >
              Novo produto
            </Botao>
          )
        }
      />

      {!ehAdmin && (
        <div className="flex items-start gap-2.5 rounded-(--radius-card) border border-caution/30 bg-caution-soft px-4 py-3 text-[0.8125rem] text-caution-ink">
          <ShieldAlert aria-hidden className="mt-px size-4 shrink-0" />
          <p>
            Seu perfil é <strong>Vendedor</strong>: o catálogo é somente leitura. Cadastro, edição,
            ajuste de estoque e mudança de status exigem papel Admin na API.
          </p>
        </div>
      )}

      <Cartao>
        <div className="flex flex-wrap items-center justify-between gap-3 border-b border-line px-4 py-3">
          <CampoDeBusca
            rotulo="Buscar produtos"
            placeholder="Nome ou marca"
            valor={busca}
            aoMudar={definirBusca}
          />
          <Segmentos
            rotulo="Filtrar produtos"
            valor={filtro}
            aoMudar={definirFiltro}
            opcoes={[
              { valor: 'todos', rotulo: 'Todos', contagem: contagens.todos },
              { valor: 'ativos', rotulo: 'Ativos', contagem: contagens.ativos },
              { valor: 'semEstoque', rotulo: 'Sem estoque', contagem: contagens.semEstoque },
              { valor: 'inativos', rotulo: 'Inativos', contagem: contagens.inativos },
            ]}
          />
        </div>

        {consulta.isPending ? (
          <EsqueletoDeTabela colunas={6} />
        ) : consulta.isError ? (
          <EstadoDeErro erro={consulta.error} aoTentarNovamente={() => void consulta.refetch()} />
        ) : (
          <Tabela
            legenda="Catálogo de produtos"
            itens={visiveis}
            colunas={colunas}
            chaveDoItem={(produto) => produto.id}
            ordenacaoInicial={{ chave: 'nome', direcao: 'crescente' }}
            vazio={
              produtos.length === 0 ? (
                <EstadoVazio
                  icone={<Package aria-hidden className="size-5" />}
                  titulo="Catálogo vazio"
                  descricao={
                    ehAdmin
                      ? 'Cadastre o primeiro produto para poder montar pedidos.'
                      : 'Nenhum produto cadastrado. Peça a um administrador para incluir itens.'
                  }
                  acao={
                    ehAdmin && (
                      <Botao
                        variante="primario"
                        tamanho="pequeno"
                        onClick={() => {
                          definirEmEdicao(null)
                          definirFormularioAberto(true)
                        }}
                        iconeInicial={<PackagePlus aria-hidden className="size-3.5" />}
                      >
                        Cadastrar produto
                      </Botao>
                    )
                  }
                />
              ) : (
                <EstadoVazio
                  icone={<Package aria-hidden className="size-5" />}
                  titulo="Nenhum produto corresponde ao filtro"
                  descricao="Ajuste a busca ou volte para o filtro “Todos”."
                />
              )
            }
          />
        )}
      </Cartao>

      <FormularioDeProduto
        aberto={formularioAberto}
        produto={emEdicao}
        aoFechar={() => definirFormularioAberto(false)}
      />

      <AjusteDeEstoque produto={emAjuste} aoFechar={() => definirEmAjuste(null)} />

      <Confirmacao
        aberto={alvoDeStatus !== null}
        titulo={alvoDeStatus?.ativo ? 'Inativar produto?' : 'Ativar produto?'}
        descricao={
          alvoDeStatus?.ativo ? (
            <>
              <strong className="text-ink">{alvoDeStatus.nome}</strong> não poderá ser incluído em
              pedidos novos. As reservas de pedidos abertos continuam válidas.
            </>
          ) : (
            <>
              <strong className="text-ink">{alvoDeStatus?.nome}</strong> volta a ficar disponível
              para novos pedidos.
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

function FormularioDeProduto({
  aberto,
  produto,
  aoFechar,
}: {
  aberto: boolean
  produto: Produto | null
  aoFechar: () => void
}) {
  const clienteDeConsultas = useQueryClient()
  const editando = produto !== null

  const {
    register,
    handleSubmit,
    setError,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<Formulario>({
    resolver: zodResolver(esquema),
    values: {
      nome: produto?.nome ?? '',
      marca: produto?.marca ?? '',
      preco: produto ? String(produto.preco) : '',
      estoque: produto ? String(produto.estoque) : '',
    },
  })

  async function enviar(dados: Formulario) {
    const carga = {
      nome: dados.nome.trim(),
      marca: dados.marca.trim(),
      preco: Number(dados.preco),
      estoque: Number(dados.estoque),
    }

    try {
      if (editando) {
        await produtosApi.atualizar({ id: produto.id, ...carga })
        toast.success('Produto atualizado', { description: carga.nome })
      } else {
        await produtosApi.criar(carga)
        toast.success('Produto cadastrado', { description: carga.nome })
      }

      await clienteDeConsultas.invalidateQueries({ queryKey: ['produtos'] })
      reset()
      aoFechar()
    } catch (erro) {
      tratarErroDeFormulario(erro, setError, ['nome', 'marca', 'preco', 'estoque'])
    }
  }

  return (
    <Modal
      aberto={aberto}
      aoFechar={aoFechar}
      titulo={editando ? 'Editar produto' : 'Novo produto'}
      descricao={
        editando
          ? 'O valor informado em estoque substitui o disponível atual; a quantidade já reservada não é alterada.'
          : 'Preço maior que zero e estoque não negativo, como exige o validador da API.'
      }
      largura="estreita"
      rodape={
        <>
          <Botao variante="sutil" onClick={aoFechar} disabled={isSubmitting}>
            Cancelar
          </Botao>
          <Botao variante="primario" carregando={isSubmitting} onClick={handleSubmit(enviar)}>
            {editando ? 'Salvar alterações' : 'Cadastrar'}
          </Botao>
        </>
      }
    >
      <form onSubmit={handleSubmit(enviar)} className="grid gap-4" noValidate>
        <Campo rotulo="Nome" erro={errors.nome?.message} obrigatorio>
          {(propriedades) => (
            <Entrada {...propriedades} {...register('nome')} placeholder="Ex.: Teclado sem fio" autoFocus />
          )}
        </Campo>

        <Campo rotulo="Marca" erro={errors.marca?.message} obrigatorio>
          {(propriedades) => (
            <Entrada {...propriedades} {...register('marca')} placeholder="Ex.: Logitech" />
          )}
        </Campo>

        <div className="grid gap-4 sm:grid-cols-2">
          <Campo rotulo="Preço (R$)" erro={errors.preco?.message} obrigatorio>
            {(propriedades) => (
              <Entrada
                {...propriedades}
                {...register('preco')}
                type="number"
                step="0.01"
                min="0.01"
                inputMode="decimal"
                placeholder="0,00"
                className="numerico"
              />
            )}
          </Campo>

          <Campo
            rotulo="Estoque"
            erro={errors.estoque?.message}
            dica={
              editando && produto.quantidadeReservada > 0
                ? `${formatarNumero(produto.quantidadeReservada)} em reserva`
                : undefined
            }
            obrigatorio
          >
            {(propriedades) => (
              <Entrada
                {...propriedades}
                {...register('estoque')}
                type="number"
                step="1"
                min="0"
                inputMode="numeric"
                placeholder="0"
                className="numerico"
              />
            )}
          </Campo>
        </div>
      </form>
    </Modal>
  )
}
