import { useQuery, useQueryClient } from '@tanstack/react-query'
import { AlertTriangle, Minus, Plus, ShoppingCart, Trash2 } from 'lucide-react'
import { useEffect, useMemo, useState } from 'react'
import { toast } from 'sonner'

import { clientesApi, pedidosApi, produtosApi } from '@/api/recursos'
import type { Cliente, Pedido, Produto, TipoMovimentacao } from '@/api/tipos'
import { Botao } from '@/componentes/ui/Botao'
import { Campo } from '@/componentes/ui/Campo'
import { Etiqueta } from '@/componentes/ui/Etiqueta'
import { Segmentos } from '@/componentes/ui/Filtros'
import { Modal } from '@/componentes/ui/Modal'
import { SeletorComPesquisa } from '@/componentes/ui/SeletorComPesquisa'
import { notificarErro } from '@/lib/notificacoes'
import { cn, formatarMoeda, formatarNumero } from '@/lib/utils'

// Referências estáveis: um [] novo a cada render invalidaria o useMemo.
const SEM_CLIENTES: Cliente[] = []
const SEM_PRODUTOS: Produto[] = []

interface ItemEmMontagem {
  produtoId: number
  quantidade: number
}

interface Propriedades {
  aberto: boolean
  aoFechar: () => void
  aoCriar: (pedido: Pedido) => void
}

export function NovoPedido({ aberto, aoFechar, aoCriar }: Propriedades) {
  const clienteDeConsultas = useQueryClient()
  const [clienteId, definirClienteId] = useState<number | null>(null)
  const [tipo, definirTipo] = useState<TipoMovimentacao>('Saida')
  const [itens, definirItens] = useState<ItemEmMontagem[]>([])
  // Venda retira do disponível; compra só anota a pendência e não depende dele.
  const ehVenda = tipo === 'Saida'
  const [enviando, definirEnviando] = useState(false)

  const consultaClientes = useQuery({
    queryKey: ['clientes'],
    queryFn: ({ signal }) => clientesApi.listar(signal),
    enabled: aberto,
  })

  const consultaProdutos = useQuery({
    queryKey: ['produtos'],
    queryFn: ({ signal }) => produtosApi.listar(signal),
    enabled: aberto,
  })

  const clientes = consultaClientes.data ?? SEM_CLIENTES
  const produtos = consultaProdutos.data ?? SEM_PRODUTOS
  const clientesAtivos = clientes.filter((cliente) => cliente.ativo).length

  const porId = useMemo(() => new Map(produtos.map((produto) => [produto.id, produto])), [produtos])

  useEffect(() => {
    if (!aberto) {
      definirClienteId(null)
      definirTipo('Saida')
      definirItens([])
    }
  }, [aberto])

  // Inativos e sem estoque continuam na pesquisa, marcados: some da lista e a
  // pessoa não sabe se digitou errado ou se o registro existe. A API recusaria
  // os dois com 409.
  function motivoDoProduto(produto: Produto) {
    if (!produto.ativo) {
      return 'Inativo'
    }
    const quantidadeNoPedido = itens.find((item) => item.produtoId === produto.id)?.quantidade ?? 0
    if (ehVenda && produto.estoque <= quantidadeNoPedido) {
      return quantidadeNoPedido > 0 ? 'No limite do estoque' : 'Sem estoque'
    }
    return null
  }

  // Escolher um produto que já está no pedido soma uma unidade, em vez de
  // criar uma segunda linha do mesmo item.
  function adicionar(produto: Produto) {
    const existente = itens.find((item) => item.produtoId === produto.id)
    if (existente) {
      alterarQuantidade(produto.id, existente.quantidade + 1)
      return
    }
    definirItens((atuais) => [...atuais, { produtoId: produto.id, quantidade: 1 }])
  }

  const total = itens.reduce((soma, item) => {
    const produto = porId.get(item.produtoId)
    return soma + (produto ? produto.preco * item.quantidade : 0)
  }, 0)

  function mudarTipo(novo: TipoMovimentacao) {
    definirTipo(novo)
    if (novo !== 'Saida') return
    // Ao voltar para venda, o que foi montado como compra pode exceder o
    // disponível: ajusta ao limite e tira o que não tem estoque.
    definirItens((atuais) =>
      atuais
        .map((item) => ({
          ...item,
          quantidade: Math.min(item.quantidade, porId.get(item.produtoId)?.estoque ?? 0),
        }))
        .filter((item) => item.quantidade > 0),
    )
  }

  function alterarQuantidade(produtoId: number, quantidade: number) {
    const produto = porId.get(produtoId)
    const limite = ehVenda ? (produto?.estoque ?? 1) : Number.MAX_SAFE_INTEGER
    const ajustada = Math.min(Math.max(1, quantidade), Math.max(1, limite))
    definirItens((atuais) =>
      atuais.map((item) => (item.produtoId === produtoId ? { ...item, quantidade: ajustada } : item)),
    )
  }

  function remover(produtoId: number) {
    definirItens((atuais) => atuais.filter((item) => item.produtoId !== produtoId))
  }

  const podeEnviar = clienteId !== null && itens.length > 0 && !enviando

  async function enviar() {
    if (!podeEnviar) {
      return
    }
    definirEnviando(true)

    try {
      const pedido = await pedidosApi.criar({
        clienteId,
        tipoMovimentacao: tipo,
        itens: itens.map(({ produtoId, quantidade }) => ({ produtoId, quantidade })),
      })

      toast.success(`${ehVenda ? 'Venda' : 'Compra'} #${pedido.id} criada`, {
        description: `${formatarMoeda(pedido.valorTotal)} em ${itens.length} ${
          itens.length === 1 ? 'item' : 'itens'
        }. ${
          ehVenda
            ? 'O estoque já foi reservado.'
            : 'As unidades entram no estoque quando o pedido for finalizado.'
        }`,
      })

      // A criação reserva estoque: a lista de produtos ficou desatualizada.
      await clienteDeConsultas.invalidateQueries({ queryKey: ['produtos'] })
      aoCriar(pedido)
      aoFechar()
    } catch (erro) {
      notificarErro(erro, 'Não foi possível criar o pedido')
    } finally {
      definirEnviando(false)
    }
  }

  return (
    <Modal
      aberto={aberto}
      aoFechar={aoFechar}
      titulo="Novo pedido"
      descricao={
        ehVenda
          ? 'A venda reserva o estoque dos itens em uma única transação. Se qualquer item falhar, nada é reservado.'
          : 'A compra registra as unidades como pendentes; elas entram no estoque quando o pedido for finalizado.'
      }
      largura="larga"
      rodape={
        <>
          <div className="mr-auto flex items-baseline gap-2 max-sm:w-full max-sm:justify-between">
            <span className="text-[0.8125rem] text-ink-muted">Total</span>
            <span className="numerico text-lg font-semibold tracking-tight text-ink">
              {formatarMoeda(total)}
            </span>
          </div>
          <Botao variante="sutil" onClick={aoFechar} disabled={enviando}>
            Cancelar
          </Botao>
          <Botao
            variante="primario"
            onClick={enviar}
            disabled={!podeEnviar}
            carregando={enviando}
            iconeInicial={<ShoppingCart aria-hidden className="size-4" />}
          >
            Criar pedido
          </Botao>
        </>
      }
    >
      <div className="grid gap-5">
        <div className="grid gap-1.5">
          <span className="text-[0.8125rem] font-medium text-ink-soft">Tipo de pedido</span>
          <Segmentos
            rotulo="Tipo de pedido"
            valor={tipo}
            aoMudar={mudarTipo}
            opcoes={[
              { valor: 'Saida', rotulo: 'Venda' },
              { valor: 'Entrada', rotulo: 'Compra' },
            ]}
          />
        </div>

        <Campo
          rotulo="Cliente"
          obrigatorio
          dica={
            clientesAtivos === 0 && !consultaClientes.isPending
              ? 'Nenhum cliente ativo. Ative ou cadastre um cliente antes de montar o pedido.'
              : 'Digite o código e tecle Enter, ou pesquise pelo nome.'
          }
        >
          {(propriedades) => (
            <SeletorComPesquisa
              {...propriedades}
              itens={clientes}
              valor={clienteId}
              aoEscolher={(cliente) => definirClienteId(cliente.id)}
              aoLimpar={() => definirClienteId(null)}
              obterId={(cliente) => cliente.id}
              obterTitulo={(cliente) => cliente.nome}
              obterDescricao={(cliente) => cliente.email}
              obterTextoDeBusca={(cliente) => cliente.cpf}
              obterMotivoIndisponivel={(cliente) => (cliente.ativo ? null : 'Inativo')}
              nomeDaEntidade="cliente"
              placeholder="Pesquisar cliente"
              tituloDoModal="Pesquisar cliente"
              descricaoDoModal="Busque por nome, e-mail, CPF ou código."
              carregando={consultaClientes.isPending}
            />
          )}
        </Campo>

        <div className="grid gap-2">
          <Campo
            rotulo={ehVenda ? 'Adicionar produto' : 'Adicionar produto à compra'}
            dica="Cada escolha adiciona uma unidade; escolher de novo soma mais uma."
          >
            {(propriedades) => (
              <SeletorComPesquisa
                {...propriedades}
                itens={produtos}
                valor={null}
                limparAoEscolher
                aoEscolher={adicionar}
                obterId={(produto) => produto.id}
                obterTitulo={(produto) => produto.nome}
                obterDescricao={(produto) => produto.marca}
                obterComplemento={(produto) => (
                  <>
                    <span className="numerico block font-medium text-ink">
                      {formatarMoeda(produto.preco)}
                    </span>
                    <span className="numerico block text-ink-muted">
                      {formatarNumero(produto.estoque)} disp.
                    </span>
                  </>
                )}
                obterMotivoIndisponivel={motivoDoProduto}
                nomeDaEntidade="produto"
                placeholder="Pesquisar produto"
                tituloDoModal="Adicionar produto"
                descricaoDoModal={
                  ehVenda
                    ? 'Busque por nome, marca ou código. Produtos sem estoque não entram em venda.'
                    : 'Busque por nome, marca ou código.'
                }
                carregando={consultaProdutos.isPending}
              />
            )}
          </Campo>

          {itens.length === 0 ? (
            <p className="rounded-lg border border-dashed border-line-strong px-4 py-8 text-center text-sm text-ink-muted">
              Nenhum item no pedido. A API recusa pedidos sem itens.
            </p>
          ) : (
            <ul className="grid gap-2">
              {itens.map((item) => {
                const produto = porId.get(item.produtoId)
                if (!produto) return null
                return (
                  <LinhaDeItem
                    key={item.produtoId}
                    produto={produto}
                    limitarAoEstoque={ehVenda}
                    quantidade={item.quantidade}
                    aoAlterar={(valor) => alterarQuantidade(item.produtoId, valor)}
                    aoRemover={() => remover(item.produtoId)}
                  />
                )
              })}
            </ul>
          )}
        </div>
      </div>
    </Modal>
  )
}

function LinhaDeItem({
  produto,
  limitarAoEstoque,
  quantidade,
  aoAlterar,
  aoRemover,
}: {
  produto: Produto
  limitarAoEstoque: boolean
  quantidade: number
  aoAlterar: (quantidade: number) => void
  aoRemover: () => void
}) {
  const noLimite = limitarAoEstoque && quantidade >= produto.estoque

  return (
    <li className="flex flex-wrap items-center gap-x-3 gap-y-2 rounded-lg border border-line bg-surface-2/50 px-3 py-2.5">
      {/* No celular o nome ocupa a linha inteira e os controles descem juntos. */}
      <div className="min-w-0 flex-1 max-sm:basis-full">
        <p className="truncate text-sm font-medium text-ink">{produto.nome}</p>
        <p className="numerico truncate text-xs text-ink-muted">
          {produto.marca} · {formatarMoeda(produto.preco)} · {formatarNumero(produto.estoque)} em
          estoque
        </p>
      </div>

      <div className="flex items-center gap-1 rounded-lg border border-line bg-surface p-0.5">
        <button
          type="button"
          onClick={() => aoAlterar(quantidade - 1)}
          disabled={quantidade <= 1}
          aria-label={`Diminuir quantidade de ${produto.nome}`}
          className="grid size-7 place-items-center rounded-md text-ink-muted transition-colors hover:bg-surface-2 hover:text-ink disabled:opacity-40"
        >
          <Minus aria-hidden className="size-3.5" />
        </button>
        <input
          type="number"
          value={quantidade}
          min={1}
          max={limitarAoEstoque ? produto.estoque : undefined}
          aria-label={`Quantidade de ${produto.nome}`}
          onChange={(evento) => aoAlterar(Number(evento.target.value))}
          className="numerico w-12 border-0 bg-transparent text-center text-sm font-medium text-ink outline-none"
        />
        <button
          type="button"
          onClick={() => aoAlterar(quantidade + 1)}
          disabled={noLimite}
          aria-label={`Aumentar quantidade de ${produto.nome}`}
          className="grid size-7 place-items-center rounded-md text-ink-muted transition-colors hover:bg-surface-2 hover:text-ink disabled:opacity-40"
        >
          <Plus aria-hidden className="size-3.5" />
        </button>
      </div>

      <span className={cn('numerico min-w-24 text-right text-sm font-semibold text-ink max-sm:ml-auto')}>
        {formatarMoeda(produto.preco * quantidade)}
      </span>

      <button
        type="button"
        onClick={aoRemover}
        aria-label={`Remover ${produto.nome}`}
        className="grid size-8 place-items-center rounded-lg text-ink-muted transition-colors hover:bg-negative-soft hover:text-negative-ink"
      >
        <Trash2 aria-hidden className="size-4" />
      </button>

      {noLimite && (
        <Etiqueta tom="atencao" className="w-full sm:w-auto">
          <AlertTriangle aria-hidden className="size-3" />
          No limite do estoque
        </Etiqueta>
      )}
    </li>
  )
}
