import { useQuery } from '@tanstack/react-query'
import {
  ArrowDownRight,
  ArrowUpRight,
  History,
  PackageSearch,
  SlidersHorizontal,
  Wrench,
} from 'lucide-react'
import { useMemo, useState } from 'react'
import { useSearchParams } from 'react-router-dom'

import { movimentacoesApi, produtosApi } from '@/api/recursos'
import type { MovimentacaoEstoque, OrigemMovimentacao, Produto } from '@/api/tipos'
import { CabecalhoDaPagina } from '@/componentes/layout/CabecalhoDaPagina'
import { Botao } from '@/componentes/ui/Botao'
import { Campo } from '@/componentes/ui/Campo'
import { CabecalhoDoCartao, Cartao } from '@/componentes/ui/Cartao'
import { EsqueletoDeTabela, EstadoDeErro, EstadoVazio } from '@/componentes/ui/Estados'
import { Etiqueta, type TomEtiqueta } from '@/componentes/ui/Etiqueta'
import { Segmentos } from '@/componentes/ui/Filtros'
import { Indicador } from '@/componentes/ui/Indicador'
import { SeletorComPesquisa } from '@/componentes/ui/SeletorComPesquisa'
import { Tabela, type Coluna } from '@/componentes/ui/Tabela'
import { formatarDataHora, formatarNumero, formatarTempoRelativo } from '@/lib/utils'
import { AjusteDeEstoque } from '@/produtos/AjusteDeEstoque'

const rotulosDeOrigem: Record<OrigemMovimentacao, string> = {
  PedidoDeVenda: 'Pedido de venda',
  PedidoDeCompra: 'Pedido de compra',
  AjusteManual: 'Ajuste manual',
}

const tonsDeOrigem: Record<OrigemMovimentacao, TomEtiqueta> = {
  PedidoDeVenda: 'marca',
  PedidoDeCompra: 'neutro',
  AjusteManual: 'atencao',
}

type Filtro = 'todas' | OrigemMovimentacao

// Referência estável: um [] novo a cada render invalidaria os useMemo abaixo.
const SEM_MOVIMENTACOES: MovimentacaoEstoque[] = []

export function Estoque() {
  const [parametros, definirParametros] = useSearchParams()
  const produtoId = Number(parametros.get('produto')) || 0
  const [filtro, definirFiltro] = useState<Filtro>('todas')
  const [emAjuste, definirEmAjuste] = useState<Produto | null>(null)

  const consultaProdutos = useQuery({
    queryKey: ['produtos'],
    queryFn: ({ signal }) => produtosApi.listar(signal),
  })

  const consultaMovimentacoes = useQuery({
    queryKey: ['movimentacoes', produtoId],
    queryFn: ({ signal }) => movimentacoesApi.listarPorProduto(produtoId, signal),
    enabled: produtoId > 0,
  })

  const produtos = useMemo(
    () =>
      [...(consultaProdutos.data ?? [])].sort((a, b) =>
        a.nome.localeCompare(b.nome, 'pt-BR', { sensitivity: 'base' }),
      ),
    [consultaProdutos.data],
  )
  const produto = produtos.find((item) => item.id === produtoId) ?? null
  const movimentacoes = consultaMovimentacoes.data ?? SEM_MOVIMENTACOES

  const resumo = useMemo(() => {
    let entradas = 0
    let saidas = 0
    for (const movimentacao of movimentacoes) {
      if (movimentacao.tipoMovimentacao === 'Entrada') entradas += movimentacao.quantidade
      else saidas += movimentacao.quantidade
    }
    return { entradas, saidas, ultima: movimentacoes[0]?.dataMovimentacao }
  }, [movimentacoes])

  const contagens = useMemo(
    () => ({
      todas: movimentacoes.length,
      PedidoDeVenda: movimentacoes.filter((m) => m.origemMovimentacao === 'PedidoDeVenda').length,
      PedidoDeCompra: movimentacoes.filter((m) => m.origemMovimentacao === 'PedidoDeCompra').length,
      AjusteManual: movimentacoes.filter((m) => m.origemMovimentacao === 'AjusteManual').length,
    }),
    [movimentacoes],
  )

  const visiveis =
    filtro === 'todas'
      ? movimentacoes
      : movimentacoes.filter((movimentacao) => movimentacao.origemMovimentacao === filtro)

  function escolherProduto(id: string) {
    definirFiltro('todas')
    definirParametros(id ? { produto: id } : {}, { replace: true })
  }

  const colunas: Array<Coluna<MovimentacaoEstoque>> = [
    {
      chave: 'data',
      cabecalho: 'Quando',
      valor: (m) => m.dataMovimentacao,
      larguraMinima: '11rem',
      celula: (m) => (
        <div>
          <p className="numerico text-ink">{formatarDataHora(m.dataMovimentacao)}</p>
          <p className="text-xs text-ink-muted">{formatarTempoRelativo(m.dataMovimentacao)}</p>
        </div>
      ),
    },
    {
      chave: 'tipo',
      cabecalho: 'Movimento',
      valor: (m) => m.tipoMovimentacao,
      celula: (m) => (
        <span className="inline-flex items-center gap-2">
          <span
            aria-hidden
            className={
              m.tipoMovimentacao === 'Entrada'
                ? 'grid size-6 place-items-center rounded-full bg-positive-soft text-positive-ink'
                : 'grid size-6 place-items-center rounded-full bg-negative-soft text-negative-ink'
            }
          >
            {m.tipoMovimentacao === 'Entrada' ? (
              <ArrowUpRight className="size-3.5" />
            ) : (
              <ArrowDownRight className="size-3.5" />
            )}
          </span>
          <span className="text-ink-soft">
            {m.tipoMovimentacao === 'Entrada' ? 'Entrada' : 'Saída'}
          </span>
        </span>
      ),
    },
    {
      chave: 'quantidade',
      cabecalho: 'Qtd.',
      alinhamento: 'fim',
      valor: (m) => (m.tipoMovimentacao === 'Entrada' ? m.quantidade : -m.quantidade),
      celula: (m) => (
        <span
          className={
            m.tipoMovimentacao === 'Entrada'
              ? 'numerico font-semibold text-positive-ink'
              : 'numerico font-semibold text-negative-ink'
          }
        >
          {m.tipoMovimentacao === 'Entrada' ? '+' : '−'}
          {formatarNumero(m.quantidade)}
        </span>
      ),
    },
    {
      chave: 'origem',
      cabecalho: 'Origem',
      valor: (m) => m.origemMovimentacao,
      celula: (m) => (
        <div className="flex flex-wrap items-center gap-2">
          <Etiqueta tom={tonsDeOrigem[m.origemMovimentacao]}>
            {rotulosDeOrigem[m.origemMovimentacao]}
          </Etiqueta>
          {m.origemMovimentacao !== 'AjusteManual' && (
            <span className="numerico text-xs text-ink-muted">#{m.idOrigem}</span>
          )}
        </div>
      ),
    },
    {
      chave: 'observacao',
      cabecalho: 'Observação',
      ocultarEmTelaPequena: true,
      larguraMinima: '14rem',
      celula: (m) => (
        <span className="text-ink-muted">
          {m.observacao || '—'}
          {m.produtoNome && produto && m.produtoNome !== produto.nome && (
            <span className="block text-xs">registrado como “{m.produtoNome}”</span>
          )}
        </span>
      ),
    },
  ]

  return (
    <>
      <CabecalhoDaPagina
        titulo="Estoque"
        descricao="Trilha de auditoria por produto: vendas e compras finalizadas e ajustes manuais. Movimentações não são editadas nem apagadas."
        acoes={
          produto && (
            <Botao
              variante="primario"
              onClick={() => definirEmAjuste(produto)}
              iconeInicial={<SlidersHorizontal aria-hidden className="size-4" />}
            >
              Ajustar estoque
            </Botao>
          )
        }
      />

      <Cartao className="p-4">
        <div className="max-w-xl">
          <Campo
            rotulo="Produto"
            dica={
              consultaProdutos.isError
                ? 'Não foi possível carregar o catálogo.'
                : 'Digite o código e tecle Enter, ou pesquise pelo nome ou marca.'
            }
          >
            {(propriedades) => (
              <SeletorComPesquisa
                {...propriedades}
                itens={produtos}
                valor={produtoId || null}
                aoEscolher={(item) => escolherProduto(String(item.id))}
                aoLimpar={() => escolherProduto('')}
                obterId={(item) => item.id}
                obterTitulo={(item) => item.nome}
                obterDescricao={(item) => (item.ativo ? item.marca : `${item.marca} · inativo`)}
                obterComplemento={(item) => (
                  <span className="numerico">{formatarNumero(item.estoque)} disp.</span>
                )}
                nomeDaEntidade="produto"
                placeholder="Pesquisar produto"
                tituloDoModal="Pesquisar produto"
                descricaoDoModal="O histórico inclui produtos inativos."
                carregando={consultaProdutos.isPending}
              />
            )}
          </Campo>
        </div>
      </Cartao>

      {!produtoId ? (
        <Cartao>
          <EstadoVazio
            icone={<PackageSearch aria-hidden className="size-5" />}
            titulo="Escolha um produto"
            descricao="Selecione um item do catálogo para ver o histórico de entradas e saídas e registrar ajustes."
          />
        </Cartao>
      ) : (
        <>
          <div className="grid grid-cols-2 gap-3 sm:gap-4 xl:grid-cols-4">
            <Indicador
              rotulo="Disponível"
              Icone={PackageSearch}
              destaque
              carregando={consultaProdutos.isPending}
              valor={produto ? formatarNumero(produto.estoque) : '—'}
              apoio={
                produto
                  ? `${formatarNumero(produto.quantidadeReservada)} em reserva`
                  : 'Produto não encontrado no catálogo'
              }
            />
            <Indicador
              rotulo="Entradas registradas"
              Icone={ArrowUpRight}
              carregando={consultaMovimentacoes.isPending}
              valor={formatarNumero(resumo.entradas)}
              apoio="unidades somadas na trilha"
            />
            <Indicador
              rotulo="Saídas registradas"
              Icone={ArrowDownRight}
              carregando={consultaMovimentacoes.isPending}
              valor={formatarNumero(resumo.saidas)}
              apoio="unidades retiradas na trilha"
            />
            <Indicador
              rotulo="Último movimento"
              Icone={History}
              carregando={consultaMovimentacoes.isPending}
              valor={resumo.ultima ? formatarTempoRelativo(resumo.ultima) : '—'}
              apoio={resumo.ultima ? formatarDataHora(resumo.ultima) : 'Sem movimentações'}
            />
          </div>

          <Cartao>
            <CabecalhoDoCartao
              titulo="Movimentações"
              descricao={produto ? `${produto.nome} · ${produto.marca}` : `Produto #${produtoId}`}
              acao={
                <Segmentos
                  rotulo="Filtrar por origem"
                  valor={filtro}
                  aoMudar={definirFiltro}
                  opcoes={[
                    { valor: 'todas', rotulo: 'Todas', contagem: contagens.todas },
                    { valor: 'PedidoDeVenda', rotulo: 'Vendas', contagem: contagens.PedidoDeVenda },
                    { valor: 'PedidoDeCompra', rotulo: 'Compras', contagem: contagens.PedidoDeCompra },
                    { valor: 'AjusteManual', rotulo: 'Ajustes', contagem: contagens.AjusteManual },
                  ]}
                />
              }
            />

            {consultaMovimentacoes.isPending ? (
              <EsqueletoDeTabela colunas={5} />
            ) : consultaMovimentacoes.isError ? (
              <EstadoDeErro
                erro={consultaMovimentacoes.error}
                aoTentarNovamente={() => void consultaMovimentacoes.refetch()}
              />
            ) : (
              <Tabela
                legenda="Movimentações de estoque"
                itens={visiveis}
                colunas={colunas}
                chaveDoItem={(m) => m.id}
                ordenacaoInicial={{ chave: 'data', direcao: 'decrescente' }}
                vazio={
                  <EstadoVazio
                    icone={<Wrench aria-hidden className="size-5" />}
                    titulo={
                      movimentacoes.length === 0
                        ? 'Nenhuma movimentação ainda'
                        : 'Nada com esta origem'
                    }
                    descricao={
                      movimentacoes.length === 0
                        ? 'A trilha recebe um registro quando um pedido é finalizado ou quando um ajuste manual é feito.'
                        : 'Troque o filtro para ver as demais movimentações.'
                    }
                  />
                }
              />
            )}
          </Cartao>
        </>
      )}

      <AjusteDeEstoque produto={emAjuste} aoFechar={() => definirEmAjuste(null)} />
    </>
  )
}
