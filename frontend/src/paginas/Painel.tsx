import { useQuery } from '@tanstack/react-query'
import {
  ArrowRight,
  Boxes,
  PackageX,
  Plus,
  ShoppingCart,
  TrendingUp,
  Users,
  Warehouse,
} from 'lucide-react'
import { useMemo } from 'react'
import { Link } from 'react-router-dom'

import { clientesApi, pedidosApi, produtosApi } from '@/api/recursos'
import type { Pedido } from '@/api/tipos'
import { useSessao } from '@/auth/useSessao'
import { CabecalhoDaPagina } from '@/componentes/layout/CabecalhoDaPagina'
import { BarrasHorizontais, ColunasPorPeriodo, type PontoDeSerie } from '@/componentes/graficos/Barras'
import { LinkBotao } from '@/componentes/ui/Botao'
import { CabecalhoDoCartao, Cartao, CorpoDoCartao } from '@/componentes/ui/Cartao'
import { EstadoDeErro, EstadoVazio } from '@/componentes/ui/Estados'
import { Etiqueta } from '@/componentes/ui/Etiqueta'
import { SecaoFinanceiraDoPainel } from '@/financeiro/SecaoDoPainel'
import { Indicador } from '@/componentes/ui/Indicador'
import {
  formatarData,
  formatarMesAno,
  formatarMoeda,
  formatarMoedaCompacta,
  formatarNumero,
  pluralizar,
} from '@/lib/utils'
import { tonsDeStatus } from './Pedidos'

const MESES_NO_GRAFICO = 6
const ITENS_NO_RANQUE = 5

const SEM_PEDIDOS: Pedido[] = []

export function Painel() {
  const { usuario, ehAdmin } = useSessao()

  const consultaClientes = useQuery({
    queryKey: ['clientes'],
    queryFn: ({ signal }) => clientesApi.listar(signal),
  })

  const consultaProdutos = useQuery({
    queryKey: ['produtos'],
    queryFn: ({ signal }) => produtosApi.listar(signal),
  })

  const consultaPedidos = useQuery({
    queryKey: ['pedidos'],
    queryFn: ({ signal }) => pedidosApi.listar(signal),
  })

  const clientes = consultaClientes.data ?? []
  const produtos = consultaProdutos.data ?? []
  const pedidos = consultaPedidos.data ?? SEM_PEDIDOS

  const recentes = useMemo(
    () =>
      [...pedidos]
        .sort((a, b) => b.dataCadastro.localeCompare(a.dataCadastro) || b.id - a.id)
        .slice(0, ITENS_NO_RANQUE),
    [pedidos],
  )

  const clientesAtivos = clientes.filter((cliente) => cliente.ativo).length
  const produtosAtivos = produtos.filter((produto) => produto.ativo)
  const semEstoque = produtosAtivos.filter((produto) => produto.estoque === 0)
  const valorEmEstoque = produtos.reduce(
    (soma, produto) => soma + produto.preco * produto.estoque,
    0,
  )
  const unidadesReservadas = produtos.reduce(
    (soma, produto) => soma + produto.quantidadeReservada,
    0,
  )
  const pedidosAbertos = pedidos.filter((pedido) => pedido.status === 'Aberto')
  const valorEmAberto = pedidosAbertos.reduce((soma, pedido) => soma + pedido.valorTotal, 0)

  const cadastrosPorMes = useMemo<PontoDeSerie[]>(() => {
    const hoje = new Date()
    const baldes: PontoDeSerie[] = []

    for (let recuo = MESES_NO_GRAFICO - 1; recuo >= 0; recuo -= 1) {
      const referencia = new Date(hoje.getFullYear(), hoje.getMonth() - recuo, 1)
      const total = clientes.filter((cliente) => {
        const data = new Date(cliente.dataCadastro)
        return (
          data.getFullYear() === referencia.getFullYear() &&
          data.getMonth() === referencia.getMonth()
        )
      }).length

      baldes.push({
        rotulo: formatarMesAno(referencia),
        rotuloLongo: referencia.toLocaleDateString('pt-BR', { month: 'long', year: 'numeric' }),
        valor: total,
      })
    }

    return baldes
  }, [clientes])

  const ranqueDeEstoque = useMemo<PontoDeSerie[]>(
    () =>
      [...produtos]
        .map((produto) => ({
          rotulo: produto.nome,
          rotuloLongo: `${produto.nome} (${produto.marca})`,
          valor: produto.preco * produto.estoque,
        }))
        .filter((ponto) => ponto.valor > 0)
        .sort((a, b) => b.valor - a.valor)
        .slice(0, ITENS_NO_RANQUE),
    [produtos],
  )

  const reservados = useMemo(
    () =>
      produtos
        .filter((produto) => produto.quantidadeReservada > 0)
        .sort((a, b) => b.quantidadeReservada - a.quantidadeReservada)
        .slice(0, 5),
    [produtos],
  )

  const erro = consultaClientes.error ?? consultaProdutos.error
  const carregando = consultaClientes.isPending || consultaProdutos.isPending

  if (erro && !carregando) {
    return (
      <>
        <CabecalhoDaPagina titulo={`Olá, ${primeiroNome(usuario?.nome)}`} />
        <Cartao>
          <EstadoDeErro
            erro={erro}
            aoTentarNovamente={() => {
              void consultaClientes.refetch()
              void consultaProdutos.refetch()
            }}
          />
        </Cartao>
      </>
    )
  }

  return (
    <>
      <CabecalhoDaPagina
        titulo={`Olá, ${primeiroNome(usuario?.nome)}`}
        descricao={
          ehAdmin
            ? 'Visão geral do catálogo, da carteira de clientes e dos pedidos.'
            : 'Visão geral do catálogo e da carteira de clientes. Alterações de catálogo exigem perfil Admin.'
        }
        acoes={
          <LinkBotao
            to="/pedidos"
            variante="primario"
            iconeInicial={<Plus aria-hidden className="size-4" />}
          >
            Novo pedido
          </LinkBotao>
        }
      />

      <div className="grid grid-cols-1 gap-3 min-[420px]:grid-cols-2 sm:gap-4 xl:grid-cols-4">
        <Indicador
          rotulo="Clientes ativos"
          Icone={Users}
          carregando={carregando}
          valor={formatarNumero(clientesAtivos)}
          apoio={
            clientes.length > 0
              ? `de ${formatarNumero(clientes.length)} ${pluralizar(clientes.length, 'cadastrado', 'cadastrados')}`
              : 'Nenhum cliente cadastrado'
          }
        />
        <Indicador
          rotulo="Produtos ativos"
          Icone={Boxes}
          carregando={carregando}
          valor={formatarNumero(produtosAtivos.length)}
          apoio={
            semEstoque.length > 0 ? (
              <span className="text-negative-ink">
                {formatarNumero(semEstoque.length)} sem estoque
              </span>
            ) : (
              `de ${formatarNumero(produtos.length)} no catálogo`
            )
          }
        />
        <Indicador
          rotulo="Valor em estoque"
          Icone={Warehouse}
          carregando={carregando}
          destaque
          valor={formatarMoedaCompacta(valorEmEstoque)}
          apoio={`${formatarNumero(unidadesReservadas)} ${pluralizar(unidadesReservadas, 'unidade reservada', 'unidades reservadas')}`}
        />
        <Indicador
          rotulo="Pedidos em aberto"
          Icone={ShoppingCart}
          carregando={consultaPedidos.isPending}
          valor={consultaPedidos.isError ? '—' : formatarNumero(pedidosAbertos.length)}
          apoio={
            consultaPedidos.isError
              ? 'Não foi possível carregar os pedidos'
              : pedidos.length === 0
                ? 'Nenhum pedido registrado'
                : `${formatarMoeda(valorEmAberto)} a finalizar`
          }
        />
      </div>

      {/* O Financeiro é restrito a Admin; o resumo dele no Painel também. */}
      {ehAdmin && <SecaoFinanceiraDoPainel />}

      <div className="grid grid-cols-1 gap-5 sm:gap-6 lg:grid-cols-2">
        <Cartao>
          <CabecalhoDoCartao
            titulo="Novos clientes por mês"
            descricao={`Últimos ${MESES_NO_GRAFICO} meses, pela data de cadastro`}
          />
          <CorpoDoCartao>
            {carregando ? (
              <div className="h-52 animar-cintilar rounded-lg bg-surface-2" />
            ) : clientes.length === 0 ? (
              <EstadoVazio
                icone={<Users aria-hidden className="size-5" />}
                titulo="Sem cadastros para exibir"
                descricao="O gráfico aparece assim que houver clientes cadastrados."
              />
            ) : (
              <ColunasPorPeriodo
                titulo={`Novos clientes por mês nos últimos ${MESES_NO_GRAFICO} meses`}
                nomeDaMedida="Clientes cadastrados"
                dados={cadastrosPorMes}
                formatarValor={formatarNumero}
              />
            )}
          </CorpoDoCartao>
        </Cartao>

        <Cartao>
          <CabecalhoDoCartao
            titulo="Maior valor imobilizado"
            descricao="Preço × quantidade disponível, por produto"
          />
          <CorpoDoCartao>
            {carregando ? (
              <div className="h-52 animar-cintilar rounded-lg bg-surface-2" />
            ) : ranqueDeEstoque.length === 0 ? (
              <EstadoVazio
                icone={<Boxes aria-hidden className="size-5" />}
                titulo="Sem estoque valorado"
                descricao="Nenhum produto ativo com quantidade disponível."
              />
            ) : (
              <BarrasHorizontais
                titulo="Produtos com maior valor imobilizado em estoque"
                nomeDaMedida="Valor em estoque"
                dados={ranqueDeEstoque}
                formatarValor={formatarMoeda}
              />
            )}
          </CorpoDoCartao>
        </Cartao>
      </div>

      <div className="grid grid-cols-1 gap-5 sm:gap-6 lg:grid-cols-2">
        <Cartao>
          <CabecalhoDoCartao
            titulo="Estoque reservado"
            descricao="Unidades presas a pedidos ainda abertos"
            acao={
              <Link
                to="/produtos"
                className="inline-flex items-center gap-1 rounded text-[0.8125rem] font-medium text-brand-ink hover:underline"
              >
                Ver catálogo
                <ArrowRight aria-hidden className="size-3.5" />
              </Link>
            }
          />
          {reservados.length === 0 ? (
            <EstadoVazio
              icone={<TrendingUp aria-hidden className="size-5" />}
              titulo="Nada reservado"
              descricao="Todo o estoque está disponível para novos pedidos."
            />
          ) : (
            <ul className="divide-y divide-line">
              {reservados.map((produto) => (
                <li key={produto.id} className="flex items-center gap-3 px-5 py-3">
                  <div className="min-w-0 flex-1">
                    <p className="truncate text-sm font-medium text-ink">{produto.nome}</p>
                    <p className="truncate text-xs text-ink-muted">{produto.marca}</p>
                  </div>
                  <div className="text-right">
                    <p className="numerico text-sm font-semibold text-ink">
                      {formatarNumero(produto.quantidadeReservada)}
                    </p>
                    <p className="numerico text-xs text-ink-muted">
                      {formatarNumero(produto.estoque)} livres
                    </p>
                  </div>
                </li>
              ))}
            </ul>
          )}
        </Cartao>

        <Cartao>
          <CabecalhoDoCartao
            titulo="Pedidos recentes"
            descricao={`Os ${ITENS_NO_RANQUE} mais recentes`}
            acao={
              <Link
                to="/pedidos"
                className="inline-flex items-center gap-1 rounded text-[0.8125rem] font-medium text-brand-ink hover:underline"
              >
                Abrir pedidos
                <ArrowRight aria-hidden className="size-3.5" />
              </Link>
            }
          />
          {consultaPedidos.isPending ? (
            <div className="grid gap-3 p-5">
              {Array.from({ length: 4 }).map((_, indice) => (
                <div key={indice} className="h-9 animar-cintilar rounded-lg bg-surface-2" />
              ))}
            </div>
          ) : consultaPedidos.isError ? (
            <EstadoDeErro
              erro={consultaPedidos.error}
              aoTentarNovamente={() => void consultaPedidos.refetch()}
            />
          ) : recentes.length === 0 ? (
            <EstadoVazio
              icone={<ShoppingCart aria-hidden className="size-5" />}
              titulo="Nenhum pedido registrado"
              descricao="Os pedidos aparecem aqui assim que forem criados."
              acao={
                <LinkBotao to="/pedidos" variante="contorno" tamanho="pequeno">
                  Ir para pedidos
                </LinkBotao>
              }
            />
          ) : (
            <ul className="divide-y divide-line">
              {recentes.map((pedido) => (
                <li key={pedido.id} className="flex items-center gap-3 px-5 py-3">
                  <span className="numerico w-12 shrink-0 text-sm font-medium text-ink">
                    #{pedido.id}
                  </span>
                  <div className="min-w-0 flex-1">
                    <p className="truncate text-sm text-ink">
                      {pedido.clienteNome || `Cliente #${pedido.clienteId}`}
                    </p>
                    <p className="numerico truncate text-xs text-ink-muted">
                      {formatarData(pedido.dataCadastro)} · {pedido.itens.length}{' '}
                      {pluralizar(pedido.itens.length, 'item', 'itens')}
                    </p>
                  </div>
                  <div className="flex shrink-0 flex-col items-end gap-1">
                    <span className="numerico text-sm font-semibold text-ink">
                      {formatarMoeda(pedido.valorTotal)}
                    </span>
                    <Etiqueta tom={tonsDeStatus[pedido.status]} comPonto>
                      {pedido.status}
                    </Etiqueta>
                  </div>
                </li>
              ))}
            </ul>
          )}
        </Cartao>
      </div>

      {semEstoque.length > 0 && (
        <Cartao className="border-caution/30 bg-caution-soft/40">
          <div className="flex flex-wrap items-center gap-3 px-5 py-4">
            <span
              aria-hidden
              className="grid size-9 shrink-0 place-items-center rounded-lg bg-caution-soft text-caution-ink"
            >
              <PackageX className="size-4" />
            </span>
            <div className="min-w-0 flex-1">
              <p className="text-sm font-medium text-ink">
                {formatarNumero(semEstoque.length)}{' '}
                {pluralizar(semEstoque.length, 'produto ativo sem estoque', 'produtos ativos sem estoque')}
              </p>
              <p className="truncate text-[0.8125rem] text-ink-muted">
                {semEstoque
                  .slice(0, 4)
                  .map((produto) => produto.nome)
                  .join(', ')}
                {semEstoque.length > 4 && ` e mais ${semEstoque.length - 4}`}
              </p>
            </div>
            <LinkBotao to="/produtos" variante="contorno" tamanho="pequeno">
              Repor estoque
            </LinkBotao>
          </div>
        </Cartao>
      )}
    </>
  )
}

function primeiroNome(nome: string | undefined) {
  return nome?.trim().split(/\s+/)[0] ?? 'bem-vindo'
}
