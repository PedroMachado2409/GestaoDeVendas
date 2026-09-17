import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Ban, CheckCircle2, Eye, Plus, RefreshCw, ShoppingCart } from 'lucide-react'
import { useMemo, useState } from 'react'
import { toast } from 'sonner'

import { clientesApi, pedidosApi, produtosApi } from '@/api/recursos'
import type { Pedido, StatusPedido } from '@/api/tipos'
import { useSessao } from '@/auth/useSessao'
import { CabecalhoDaPagina } from '@/componentes/layout/CabecalhoDaPagina'
import { Botao } from '@/componentes/ui/Botao'
import { Cartao } from '@/componentes/ui/Cartao'
import { Confirmacao } from '@/componentes/ui/Confirmacao'
import { EsqueletoDeTabela, EstadoDeErro, EstadoVazio } from '@/componentes/ui/Estados'
import { Etiqueta, type TomEtiqueta } from '@/componentes/ui/Etiqueta'
import { CampoDeBusca, Segmentos } from '@/componentes/ui/Filtros'
import { ItemDeMenu, Menu } from '@/componentes/ui/Menu'
import { Modal } from '@/componentes/ui/Modal'
import { Tabela, type Coluna } from '@/componentes/ui/Tabela'
import { notificarErro } from '@/lib/notificacoes'
import {
  formatarDataHora,
  formatarMoeda,
  formatarNumero,
  normalizarTexto,
  pluralizar,
} from '@/lib/utils'
import { NovoPedido } from '@/pedidos/NovoPedido'

export const tonsDeStatus: Record<StatusPedido, TomEtiqueta> = {
  Aberto: 'marca',
  Finalizado: 'positivo',
  Cancelado: 'neutro',
}

type Filtro = 'todos' | StatusPedido

// Referência estável: um [] novo a cada render invalidaria os useMemo abaixo.
const SEM_PEDIDOS: Pedido[] = []

export function Pedidos() {
  const { usuario } = useSessao()
  const clienteDeConsultas = useQueryClient()

  const [busca, definirBusca] = useState('')
  const [filtro, definirFiltro] = useState<Filtro>('todos')
  const [criacaoAberta, definirCriacaoAberta] = useState(false)
  // Guarda o id, não o objeto: o detalhe acompanha a lista quando ela é
  // atualizada por uma finalização ou cancelamento.
  const [idDoDetalhe, definirIdDoDetalhe] = useState<number | null>(null)
  const [alvoDeCancelamento, definirAlvoDeCancelamento] = useState<Pedido | null>(null)

  const consulta = useQuery({
    queryKey: ['pedidos'],
    queryFn: ({ signal }) => pedidosApi.listar(signal),
  })

  const consultaClientes = useQuery({
    queryKey: ['clientes'],
    queryFn: ({ signal }) => clientesApi.listar(signal),
  })

  const pedidos = consulta.data ?? SEM_PEDIDOS

  const nomesDeClientes = useMemo(
    () => new Map((consultaClientes.data ?? []).map((cliente) => [cliente.id, cliente.nome])),
    [consultaClientes.data],
  )

  // A listagem já traz os nomes. Os fallbacks cobrem o intervalo entre criar
  // um pedido (a resposta da criação não carrega cliente nem autor) e a lista
  // ser recarregada.
  const nomeDoCliente = (pedido: Pedido) =>
    pedido.clienteNome || nomesDeClientes.get(pedido.clienteId)

  const nomeDoVendedor = (pedido: Pedido) =>
    pedido.usuarioNome || (pedido.usuarioId === usuario?.id ? usuario.nome : undefined)

  const contagens = useMemo(
    () => ({
      todos: pedidos.length,
      Aberto: pedidos.filter((p) => p.status === 'Aberto').length,
      Finalizado: pedidos.filter((p) => p.status === 'Finalizado').length,
      Cancelado: pedidos.filter((p) => p.status === 'Cancelado').length,
    }),
    [pedidos],
  )

  const termo = normalizarTexto(busca).replace(/^#/, '')

  const visiveis = pedidos.filter((pedido) => {
    if (filtro !== 'todos' && pedido.status !== filtro) {
      return false
    }
    if (!termo) {
      return true
    }
    return (
      String(pedido.id) === termo ||
      normalizarTexto(nomeDoCliente(pedido) ?? '').includes(termo) ||
      normalizarTexto(nomeDoVendedor(pedido) ?? '').includes(termo)
    )
  })

  const detalhe = pedidos.find((pedido) => pedido.id === idDoDetalhe) ?? null

  function aoMudarPedido(atualizado: Pedido) {
    clienteDeConsultas.setQueryData<Pedido[]>(['pedidos'], (atuais) =>
      atuais?.map((pedido) =>
        pedido.id === atualizado.id
          ? // A resposta da transição pode vir sem os nomes; preserva os da lista.
            {
              ...atualizado,
              clienteNome: atualizado.clienteNome || pedido.clienteNome,
              usuarioNome: atualizado.usuarioNome || pedido.usuarioNome,
            }
          : pedido,
      ),
    )
    void clienteDeConsultas.invalidateQueries({ queryKey: ['pedidos'] })
    // Finalizar movimenta estoque e grava trilha; cancelar desfaz reserva.
    void clienteDeConsultas.invalidateQueries({ queryKey: ['produtos'] })
    void clienteDeConsultas.invalidateQueries({ queryKey: ['movimentacoes'] })
  }

  const finalizar = useMutation({
    mutationFn: (pedido: Pedido) => pedidosApi.finalizar(pedido.id),
    onSuccess: (atualizado) => {
      toast.success(`Pedido #${atualizado.id} finalizado`, {
        description: 'O estoque foi atualizado e a movimentação ficou registrada.',
      })
      aoMudarPedido(atualizado)
    },
    onError: (erro) => notificarErro(erro, 'Não foi possível finalizar'),
  })

  const cancelar = useMutation({
    mutationFn: (pedido: Pedido) => pedidosApi.cancelar(pedido.id),
    onSuccess: (atualizado) => {
      toast.success(`Pedido #${atualizado.id} cancelado`, {
        description: 'Reservas e pendências do pedido foram desfeitas.',
      })
      definirAlvoDeCancelamento(null)
      aoMudarPedido(atualizado)
    },
    onError: (erro) => notificarErro(erro, 'Não foi possível cancelar'),
  })

  const colunas: Array<Coluna<Pedido>> = [
    {
      chave: 'id',
      cabecalho: 'Pedido',
      valor: (pedido) => pedido.id,
      celula: (pedido) => (
        <button
          type="button"
          onClick={() => definirIdDoDetalhe(pedido.id)}
          className="numerico rounded font-medium text-ink hover:text-brand-ink hover:underline"
        >
          #{pedido.id}
        </button>
      ),
    },
    {
      chave: 'cliente',
      cabecalho: 'Cliente',
      larguraMinima: '11rem',
      valor: (pedido) => nomeDoCliente(pedido) ?? String(pedido.clienteId),
      celula: (pedido) => (
        <span className="text-pretty text-ink-soft">
          {nomeDoCliente(pedido) ?? (
            <span className="numerico text-ink-muted">cliente #{pedido.clienteId}</span>
          )}
        </span>
      ),
    },
    {
      chave: 'vendedor',
      cabecalho: 'Vendedor',
      ocultarEmTelaPequena: true,
      valor: (pedido) => nomeDoVendedor(pedido) ?? '',
      celula: (pedido) => (
        <span className="text-ink-muted">{nomeDoVendedor(pedido) ?? '—'}</span>
      ),
    },
    {
      chave: 'data',
      cabecalho: 'Criado em',
      ocultarEmTelaPequena: true,
      valor: (pedido) => pedido.dataCadastro,
      celula: (pedido) => (
        <span className="numerico whitespace-nowrap text-ink-soft">
          {formatarDataHora(pedido.dataCadastro)}
        </span>
      ),
    },
    {
      chave: 'itens',
      cabecalho: 'Itens',
      alinhamento: 'fim',
      ocultarEmTelaPequena: true,
      valor: (pedido) => pedido.itens.length,
      celula: (pedido) => (
        <span className="numerico text-ink-muted">{formatarNumero(pedido.itens.length)}</span>
      ),
    },
    {
      chave: 'total',
      cabecalho: 'Total',
      alinhamento: 'fim',
      valor: (pedido) => pedido.valorTotal,
      celula: (pedido) => (
        <span className="numerico font-semibold whitespace-nowrap text-ink">
          {formatarMoeda(pedido.valorTotal)}
        </span>
      ),
    },
    {
      chave: 'status',
      cabecalho: 'Status',
      valor: (pedido) => pedido.status,
      celula: (pedido) => (
        <Etiqueta tom={tonsDeStatus[pedido.status]} comPonto>
          {pedido.status}
        </Etiqueta>
      ),
    },
    {
      chave: 'acoes',
      cabecalho: <span className="sr-only">Ações</span>,
      alinhamento: 'fim',
      celula: (pedido) => (
        <Menu rotulo={`Ações do pedido ${pedido.id}`}>
          {(fechar) => (
            <>
              <ItemDeMenu
                icone={<Eye aria-hidden className="size-4" />}
                onClick={() => {
                  fechar()
                  definirIdDoDetalhe(pedido.id)
                }}
              >
                Ver detalhes
              </ItemDeMenu>
              <ItemDeMenu
                disabled={pedido.status !== 'Aberto' || finalizar.isPending}
                icone={<CheckCircle2 aria-hidden className="size-4" />}
                onClick={() => {
                  fechar()
                  finalizar.mutate(pedido)
                }}
              >
                Finalizar
              </ItemDeMenu>
              <ItemDeMenu
                tom="perigo"
                disabled={pedido.status !== 'Aberto'}
                icone={<Ban aria-hidden className="size-4" />}
                onClick={() => {
                  fechar()
                  definirAlvoDeCancelamento(pedido)
                }}
              >
                Cancelar pedido
              </ItemDeMenu>
            </>
          )}
        </Menu>
      ),
    },
  ]

  const unidadesDoCancelamento =
    alvoDeCancelamento?.itens.reduce((soma, item) => soma + item.quantidade, 0) ?? 0

  return (
    <>
      <CabecalhoDaPagina
        titulo="Pedidos"
        descricao="Criação, finalização e cancelamento. Cada operação roda em transação e é idempotente do lado da API."
        acoes={
          <>
            <Botao
              variante="secundario"
              onClick={() => void consulta.refetch()}
              carregando={consulta.isFetching && !consulta.isPending}
              iconeInicial={<RefreshCw aria-hidden className="size-4" />}
            >
              Atualizar
            </Botao>
            <Botao
              variante="primario"
              onClick={() => definirCriacaoAberta(true)}
              iconeInicial={<Plus aria-hidden className="size-4" />}
            >
              Novo pedido
            </Botao>
          </>
        }
      />

      <Cartao>
        <div className="flex flex-wrap items-center justify-between gap-3 border-b border-line px-4 py-3">
          <CampoDeBusca
            rotulo="Buscar pedidos"
            placeholder="Nº, cliente ou vendedor"
            valor={busca}
            aoMudar={definirBusca}
          />
          <Segmentos
            rotulo="Filtrar por status"
            valor={filtro}
            aoMudar={definirFiltro}
            opcoes={[
              { valor: 'todos', rotulo: 'Todos', contagem: contagens.todos },
              { valor: 'Aberto', rotulo: 'Abertos', contagem: contagens.Aberto },
              { valor: 'Finalizado', rotulo: 'Finalizados', contagem: contagens.Finalizado },
              { valor: 'Cancelado', rotulo: 'Cancelados', contagem: contagens.Cancelado },
            ]}
          />
        </div>

        {consulta.isPending ? (
          <EsqueletoDeTabela colunas={6} />
        ) : consulta.isError ? (
          <EstadoDeErro erro={consulta.error} aoTentarNovamente={() => void consulta.refetch()} />
        ) : (
          <Tabela
            legenda="Lista de pedidos"
            itens={visiveis}
            colunas={colunas}
            chaveDoItem={(pedido) => pedido.id}
            ordenacaoInicial={{ chave: 'id', direcao: 'decrescente' }}
            vazio={
              pedidos.length === 0 ? (
                <EstadoVazio
                  icone={<ShoppingCart aria-hidden className="size-5" />}
                  titulo="Nenhum pedido registrado"
                  descricao="Crie o primeiro pedido para ele aparecer aqui."
                  acao={
                    <Botao
                      variante="primario"
                      tamanho="pequeno"
                      onClick={() => definirCriacaoAberta(true)}
                      iconeInicial={<Plus aria-hidden className="size-3.5" />}
                    >
                      Criar pedido
                    </Botao>
                  }
                />
              ) : (
                <EstadoVazio
                  icone={<ShoppingCart aria-hidden className="size-5" />}
                  titulo="Nenhum pedido corresponde ao filtro"
                  descricao="Ajuste a busca ou volte para o filtro “Todos”."
                />
              )
            }
          />
        )}
      </Cartao>

      <NovoPedido
        aberto={criacaoAberta}
        aoFechar={() => definirCriacaoAberta(false)}
        aoCriar={(pedido) => {
          clienteDeConsultas.setQueryData<Pedido[]>(['pedidos'], (atuais) =>
            atuais ? [pedido, ...atuais] : atuais,
          )
          // Relê para trazer os nomes de cliente e vendedor, que a criação não devolve.
          void clienteDeConsultas.invalidateQueries({ queryKey: ['pedidos'] })
        }}
      />

      <DetalheDoPedido
        pedido={detalhe}
        nomeDoCliente={detalhe ? nomeDoCliente(detalhe) : undefined}
        nomeDoVendedor={detalhe ? nomeDoVendedor(detalhe) : undefined}
        finalizando={finalizar.isPending}
        aoFinalizar={(pedido) => finalizar.mutate(pedido)}
        aoCancelar={(pedido) => definirAlvoDeCancelamento(pedido)}
        aoFechar={() => definirIdDoDetalhe(null)}
      />

      <Confirmacao
        aberto={alvoDeCancelamento !== null}
        titulo={`Cancelar o pedido #${alvoDeCancelamento?.id}?`}
        descricao={
          <>
            As{' '}
            <strong className="text-ink">
              {unidadesDoCancelamento} {pluralizar(unidadesDoCancelamento, 'unidade', 'unidades')}
            </strong>{' '}
            do pedido deixam de ficar reservadas (venda) ou pendentes (compra). Só pedidos abertos
            podem ser cancelados.
          </>
        }
        rotuloConfirmar="Cancelar pedido"
        varianteConfirmar="perigo"
        processando={cancelar.isPending}
        aoConfirmar={() => alvoDeCancelamento && cancelar.mutate(alvoDeCancelamento)}
        aoCancelar={() => definirAlvoDeCancelamento(null)}
      />
    </>
  )
}

function DetalheDoPedido({
  pedido,
  nomeDoCliente,
  nomeDoVendedor,
  finalizando,
  aoFinalizar,
  aoCancelar,
  aoFechar,
}: {
  pedido: Pedido | null
  nomeDoCliente?: string
  nomeDoVendedor?: string
  finalizando: boolean
  aoFinalizar: (pedido: Pedido) => void
  aoCancelar: (pedido: Pedido) => void
  aoFechar: () => void
}) {
  const consultaProdutos = useQuery({
    queryKey: ['produtos'],
    queryFn: ({ signal }) => produtosApi.listar(signal),
    enabled: pedido !== null,
  })

  const nomesDeProdutos = useMemo(
    () => new Map((consultaProdutos.data ?? []).map((produto) => [produto.id, produto.nome])),
    [consultaProdutos.data],
  )

  if (!pedido) {
    return null
  }

  const aberto = pedido.status === 'Aberto'

  return (
    <Modal
      aberto={pedido !== null}
      aoFechar={aoFechar}
      titulo={`Pedido #${pedido.id}`}
      descricao={`Criado em ${formatarDataHora(pedido.dataCadastro)}`}
      rodape={
        <>
          <div className="mr-auto flex items-baseline gap-2 max-sm:w-full max-sm:justify-between">
            <span className="text-[0.8125rem] text-ink-muted">Valor total</span>
            <span className="numerico text-lg font-semibold tracking-tight text-ink">
              {formatarMoeda(pedido.valorTotal)}
            </span>
          </div>
          {aberto ? (
            <>
              <Botao
                variante="sutil"
                onClick={() => aoCancelar(pedido)}
                disabled={finalizando}
                iconeInicial={<Ban aria-hidden className="size-4" />}
                className="text-negative-ink hover:bg-negative-soft hover:text-negative-ink"
              >
                Cancelar
              </Botao>
              <Botao
                variante="primario"
                onClick={() => aoFinalizar(pedido)}
                carregando={finalizando}
                iconeInicial={<CheckCircle2 aria-hidden className="size-4" />}
              >
                Finalizar
              </Botao>
            </>
          ) : (
            <Botao variante="secundario" onClick={aoFechar}>
              Fechar
            </Botao>
          )}
        </>
      }
    >
      <div className="grid gap-5">
        <dl className="grid grid-cols-2 gap-4 sm:grid-cols-4">
          <div className="min-w-0">
            <dt className="text-xs text-ink-muted">Cliente</dt>
            <dd className="mt-0.5 text-sm font-medium text-pretty text-ink">
              {nomeDoCliente ?? `#${pedido.clienteId}`}
            </dd>
          </div>
          <div className="min-w-0">
            <dt className="text-xs text-ink-muted">Vendedor</dt>
            <dd className="mt-0.5 text-sm font-medium text-pretty text-ink">
              {nomeDoVendedor ?? '—'}
            </dd>
          </div>
          <div>
            <dt className="text-xs text-ink-muted">Status</dt>
            <dd className="mt-1">
              <Etiqueta tom={tonsDeStatus[pedido.status]} comPonto>
                {pedido.status}
              </Etiqueta>
            </dd>
          </div>
          <div>
            <dt className="text-xs text-ink-muted">Itens</dt>
            <dd className="numerico mt-0.5 text-sm font-medium text-ink">{pedido.itens.length}</dd>
          </div>
        </dl>

        <div className="scrollbar-fina overflow-x-auto rounded-lg border border-line">
          <table className="w-full min-w-[26rem] text-sm">
            <caption className="sr-only">Itens do pedido {pedido.id}</caption>
            <thead>
              <tr className="border-b border-line bg-surface-2/60">
                <th scope="col" className="px-3 py-2 text-left text-xs font-semibold text-ink-muted uppercase">
                  Produto
                </th>
                <th scope="col" className="px-3 py-2 text-right text-xs font-semibold text-ink-muted uppercase">
                  Preço
                </th>
                <th scope="col" className="px-3 py-2 text-right text-xs font-semibold text-ink-muted uppercase">
                  Qtd.
                </th>
                <th scope="col" className="px-3 py-2 text-right text-xs font-semibold text-ink-muted uppercase">
                  Subtotal
                </th>
              </tr>
            </thead>
            <tbody>
              {pedido.itens.map((item) => (
                <tr key={item.id} className="border-b border-line/70 last:border-0">
                  <td className="px-3 py-2.5 text-ink">
                    {nomesDeProdutos.get(item.produtoId) ?? (
                      <span className="numerico text-ink-muted">produto #{item.produtoId}</span>
                    )}
                  </td>
                  <td className="numerico px-3 py-2.5 text-right text-ink-soft">
                    {formatarMoeda(item.preco)}
                  </td>
                  <td className="numerico px-3 py-2.5 text-right text-ink-soft">
                    {formatarNumero(item.quantidade)}
                  </td>
                  <td className="numerico px-3 py-2.5 text-right font-medium text-ink">
                    {formatarMoeda(item.subTotal)}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        <p className="text-xs text-ink-muted">
          O preço gravado no item é o preço vigente no momento da criação do pedido — alterações
          posteriores no catálogo não alteram pedidos já registrados.
        </p>
      </div>
    </Modal>
  )
}
