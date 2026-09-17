import {
  AlarmClock,
  Ban,
  CalendarRange,
  CheckCircle2,
  Copy,
  Eye,
  FlaskConical,
  HandCoins,
  PencilLine,
  Plus,
  Receipt,
  TriangleAlert,
  Wallet,
} from 'lucide-react'
import { useMemo, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { toast } from 'sonner'

import { CabecalhoDaPagina } from '@/componentes/layout/CabecalhoDaPagina'
import { Botao } from '@/componentes/ui/Botao'
import { Cartao } from '@/componentes/ui/Cartao'
import { Confirmacao } from '@/componentes/ui/Confirmacao'
import { EstadoVazio } from '@/componentes/ui/Estados'
import { Etiqueta } from '@/componentes/ui/Etiqueta'
import { CampoDeBusca, Segmentos } from '@/componentes/ui/Filtros'
import { Indicador } from '@/componentes/ui/Indicador'
import { ItemDeMenu, Menu, SeparadorDeMenu } from '@/componentes/ui/Menu'
import { Tabela, type Coluna } from '@/componentes/ui/Tabela'
import { BaixaDeTitulo } from '@/financeiro/BaixaDeTitulo'
import type { DadosDaBaixa, NovoTitulo } from '@/financeiro/contexto'
import { clientesFicticios, fornecedoresFicticios } from '@/financeiro/dadosFicticios'
import { adicionarDias, descreverVencimento, diasEntre, formatarDataISO, hoje } from '@/financeiro/datas'
import { DetalheDoTitulo } from '@/financeiro/DetalheDoTitulo'
import { FormularioDePrazo } from '@/financeiro/FormularioDePrazo'
import { NovoLancamento } from '@/financeiro/NovoLancamento'
import { useFinanceiro } from '@/financeiro/useFinanceiro'
import { formatarPercentual, gerarParcelas, resumirDias } from '@/financeiro/prazos'
import {
  estaEmAberto,
  estaVencido,
  rotuloDeBaixado,
  rotulosDeSituacao,
  saldoDo,
  situacaoDo,
  tonsDeSituacao,
} from '@/financeiro/titulos'
import type { PrazoPagamento, TipoTitulo, TituloFinanceiro } from '@/financeiro/tipos'
import { cn, formatarMoeda, formatarMoedaCompacta, normalizarTexto, pluralizar } from '@/lib/utils'

type Aba = 'receber' | 'pagar' | 'prazos'
type FiltroDeTitulo = 'abertos' | 'vencidos' | 'semana' | 'baixados' | 'cancelados' | 'todos'

const tipoDaAba: Record<Exclude<Aba, 'prazos'>, TipoTitulo> = {
  receber: 'Receber',
  pagar: 'Pagar',
}

export function Financeiro() {
  const [parametros, definirParametros] = useSearchParams()
  const abaNaUrl = parametros.get('aba')
  const aba: Aba = abaNaUrl === 'pagar' || abaNaUrl === 'prazos' ? abaNaUrl : 'receber'

  // Estado compartilhado com o Painel (ver financeiro/contexto.tsx).
  const financeiro = useFinanceiro()
  const { titulos, prazos } = financeiro

  const [lancamentoAberto, definirLancamentoAberto] = useState(false)
  const [idDoDetalhe, definirIdDoDetalhe] = useState<number | null>(null)
  const [idEmBaixa, definirIdEmBaixa] = useState<number | null>(null)
  const [alvoDeCancelamento, definirAlvoDeCancelamento] = useState<TituloFinanceiro | null>(null)
  const [prazoEmEdicao, definirPrazoEmEdicao] = useState<PrazoPagamento | null>(null)
  const [editorDePrazoAberto, definirEditorDePrazoAberto] = useState(false)

  const detalhe = titulos.find((titulo) => titulo.id === idDoDetalhe) ?? null
  const emBaixa = titulos.find((titulo) => titulo.id === idEmBaixa) ?? null

  function mudarAba(nova: Aba) {
    definirParametros(nova === 'receber' ? {} : { aba: nova }, { replace: true })
  }

  const contagemAbertos = (tipo: TipoTitulo) =>
    titulos.filter((titulo) => titulo.tipo === tipo && estaEmAberto(titulo)).length

  function lancar(novos: NovoTitulo[]) {
    financeiro.lancar(novos)
    const total = novos.reduce((soma, novo) => soma + novo.valorOriginal, 0)
    toast.success('Lançamento registrado', {
      description: `${formatarMoeda(total)} em ${novos.length} ${pluralizar(novos.length, 'parcela', 'parcelas')}.`,
    })
    definirLancamentoAberto(false)
    if (novos[0]) {
      mudarAba(novos[0].tipo === 'Receber' ? 'receber' : 'pagar')
    }
  }

  function baixar(dados: DadosDaBaixa) {
    if (!emBaixa) {
      return
    }
    financeiro.baixar(emBaixa.id, dados)
    toast.success(emBaixa.tipo === 'Receber' ? 'Recebimento registrado' : 'Pagamento registrado', {
      description: `${formatarMoeda(dados.valor)} · ${emBaixa.contraparteNome}`,
    })
    definirIdEmBaixa(null)
  }

  function cancelar() {
    if (!alvoDeCancelamento) {
      return
    }
    financeiro.cancelar(alvoDeCancelamento.id)
    toast.success('Título cancelado', {
      description: `${alvoDeCancelamento.numeroDocumento} · parcela ${alvoDeCancelamento.numeroParcela}/${alvoDeCancelamento.totalParcelas}`,
    })
    definirAlvoDeCancelamento(null)
  }

  function salvarPrazo(dados: Pick<PrazoPagamento, 'nome' | 'parcelas'>) {
    // id 0 é o modelo de "Duplicar": salva como prazo novo, não como edição.
    const editando = prazoEmEdicao !== null && prazoEmEdicao.id !== 0
    financeiro.salvarPrazo(editando ? prazoEmEdicao.id : null, dados)
    toast.success(editando ? 'Prazo atualizado' : 'Prazo criado', { description: dados.nome })
    definirEditorDePrazoAberto(false)
  }

  const proximoNumeroDocumento = `LAN-${String(
    titulos.filter((titulo) => titulo.origem === 'LancamentoManual').reduce(
      (maior, titulo) => Math.max(maior, Number(titulo.numeroDocumento.replace(/\D/g, '')) || 0),
      0,
    ) + 1,
  ).padStart(4, '0')}`

  return (
    <>
      <CabecalhoDaPagina
        titulo="Financeiro"
        descricao="Contas a receber e a pagar geradas pelos pedidos ou lançadas à mão, e os prazos que definem as parcelas."
        acoes={
          aba === 'prazos' ? (
            <Botao
              variante="primario"
              onClick={() => {
                definirPrazoEmEdicao(null)
                definirEditorDePrazoAberto(true)
              }}
              iconeInicial={<Plus aria-hidden className="size-4" />}
            >
              Novo prazo
            </Botao>
          ) : (
            <Botao
              variante="primario"
              onClick={() => definirLancamentoAberto(true)}
              iconeInicial={<Plus aria-hidden className="size-4" />}
            >
              Novo lançamento
            </Botao>
          )
        }
      />

      <div className="flex items-start gap-2.5 rounded-(--radius-card) border border-caution/30 bg-caution-soft px-4 py-3 text-[0.8125rem] text-caution-ink">
        <FlaskConical aria-hidden className="mt-px size-4 shrink-0" />
        <p className="text-pretty">
          <strong>Dados fictícios.</strong> Esta tela ainda não está ligada à API: títulos, baixas e
          prazos ficam só na memória do navegador e voltam ao exemplo inicial ao recarregar a página.
        </p>
      </div>

      <div className="flex">
        <Segmentos
          rotulo="Seção do financeiro"
          valor={aba}
          aoMudar={mudarAba}
          opcoes={[
            { valor: 'receber', rotulo: 'Contas a receber', contagem: contagemAbertos('Receber') },
            { valor: 'pagar', rotulo: 'Contas a pagar', contagem: contagemAbertos('Pagar') },
            { valor: 'prazos', rotulo: 'Prazos de pagamento', contagem: prazos.length },
          ]}
        />
      </div>

      {aba === 'prazos' ? (
        <ListaDePrazos
          prazos={prazos}
          titulos={titulos}
          aoEditar={(prazo) => {
            definirPrazoEmEdicao(prazo)
            definirEditorDePrazoAberto(true)
          }}
          aoDuplicar={(prazo) => {
            // id 0 marca "modelo": abre a criação já preenchida com as parcelas do original.
            definirPrazoEmEdicao({ ...prazo, id: 0, nome: `${prazo.nome} (cópia)` })
            definirEditorDePrazoAberto(true)
          }}
          aoAlternarStatus={(prazo) => {
            financeiro.alternarPrazo(prazo.id)
            toast.success(prazo.ativo ? 'Prazo inativado' : 'Prazo ativado', { description: prazo.nome })
          }}
        />
      ) : (
        <PainelDeTitulos
          key={aba}
          tipo={tipoDaAba[aba]}
          titulos={titulos}
          aoVerDetalhe={(titulo) => definirIdDoDetalhe(titulo.id)}
          aoBaixar={(titulo) => definirIdEmBaixa(titulo.id)}
          aoCancelar={definirAlvoDeCancelamento}
        />
      )}

      <NovoLancamento
        aberto={lancamentoAberto}
        tipoInicial={aba === 'pagar' ? 'Pagar' : 'Receber'}
        prazos={prazos}
        clientes={clientesFicticios}
        fornecedores={fornecedoresFicticios}
        proximoNumeroDocumento={proximoNumeroDocumento}
        aoSalvar={lancar}
        aoFechar={() => definirLancamentoAberto(false)}
      />

      <DetalheDoTitulo
        titulo={detalhe}
        parcelasDoDocumento={
          detalhe
            ? titulos
                .filter((titulo) => titulo.tipo === detalhe.tipo && titulo.numeroDocumento === detalhe.numeroDocumento)
                .sort((a, b) => a.numeroParcela - b.numeroParcela)
            : []
        }
        aoBaixar={(titulo) => definirIdEmBaixa(titulo.id)}
        aoCancelar={definirAlvoDeCancelamento}
        aoEscolherParcela={(titulo) => definirIdDoDetalhe(titulo.id)}
        aoFechar={() => definirIdDoDetalhe(null)}
      />

      <BaixaDeTitulo titulo={emBaixa} aoConfirmar={baixar} aoFechar={() => definirIdEmBaixa(null)} />

      <FormularioDePrazo
        aberto={editorDePrazoAberto}
        prazo={prazoEmEdicao && prazoEmEdicao.id !== 0 ? prazoEmEdicao : null}
        modelo={prazoEmEdicao?.id === 0 ? prazoEmEdicao : null}
        nomesExistentes={prazos.map((prazo) => prazo.nome)}
        aoSalvar={salvarPrazo}
        aoFechar={() => definirEditorDePrazoAberto(false)}
      />

      <Confirmacao
        aberto={alvoDeCancelamento !== null}
        titulo="Cancelar este título?"
        descricao={
          alvoDeCancelamento && (
            <>
              <strong className="text-ink">
                {alvoDeCancelamento.numeroDocumento} · parcela {alvoDeCancelamento.numeroParcela}/
                {alvoDeCancelamento.totalParcelas}
              </strong>{' '}
              de {formatarMoeda(alvoDeCancelamento.valorOriginal)} deixa de ser cobrada. As demais
              parcelas do documento não mudam.
            </>
          )
        }
        rotuloConfirmar="Cancelar título"
        varianteConfirmar="perigo"
        aoConfirmar={cancelar}
        aoCancelar={() => definirAlvoDeCancelamento(null)}
      />
    </>
  )
}

function PainelDeTitulos({
  tipo,
  titulos: todos,
  aoVerDetalhe,
  aoBaixar,
  aoCancelar,
}: {
  tipo: TipoTitulo
  titulos: TituloFinanceiro[]
  aoVerDetalhe: (titulo: TituloFinanceiro) => void
  aoBaixar: (titulo: TituloFinanceiro) => void
  aoCancelar: (titulo: TituloFinanceiro) => void
}) {
  const [busca, definirBusca] = useState('')
  const [filtro, definirFiltro] = useState<FiltroDeTitulo>('abertos')
  const referencia = hoje()
  const ehReceber = tipo === 'Receber'
  const verbo = ehReceber ? 'Receber' : 'Pagar'

  const titulos = useMemo(() => todos.filter((titulo) => titulo.tipo === tipo), [todos, tipo])

  const venceNaSemana = (titulo: TituloFinanceiro) => {
    const dias = diasEntre(referencia, titulo.dataVencimento)
    return estaEmAberto(titulo) && dias >= 0 && dias <= 7
  }

  const resumo = (() => {
    const abertos = titulos.filter(estaEmAberto)
    const vencidos = abertos.filter((titulo) => estaVencido(titulo, referencia))
    const semana = abertos.filter(venceNaSemana)
    const inicioDoPeriodo = adicionarDias(referencia, -30)
    const baixasNoPeriodo = titulos.flatMap((titulo) =>
      titulo.baixas.filter((baixa) => diasEntre(inicioDoPeriodo, baixa.data) >= 0),
    )
    const soma = (lista: TituloFinanceiro[]) => lista.reduce((total, titulo) => total + saldoDo(titulo), 0)
    return {
      aberto: soma(abertos),
      quantidadeAberta: abertos.length,
      vencido: soma(vencidos),
      quantidadeVencida: vencidos.length,
      semana: soma(semana),
      quantidadeSemana: semana.length,
      baixadoNoPeriodo: baixasNoPeriodo.reduce((total, baixa) => total + baixa.valor, 0),
      quantidadeBaixas: baixasNoPeriodo.length,
    }
  })()

  const contagens: Record<FiltroDeTitulo, number> = {
    abertos: resumo.quantidadeAberta,
    vencidos: resumo.quantidadeVencida,
    semana: resumo.quantidadeSemana,
    baixados: titulos.filter((titulo) => titulo.status === 'Baixado').length,
    cancelados: titulos.filter((titulo) => titulo.status === 'Cancelado').length,
    todos: titulos.length,
  }

  const termo = normalizarTexto(busca)
  const visiveis = titulos.filter((titulo) => {
    if (filtro === 'abertos' && !estaEmAberto(titulo)) {
      return false
    }
    if (filtro === 'vencidos' && !estaVencido(titulo, referencia)) {
      return false
    }
    if (filtro === 'semana' && !venceNaSemana(titulo)) {
      return false
    }
    if (filtro === 'baixados' && titulo.status !== 'Baixado') {
      return false
    }
    if (filtro === 'cancelados' && titulo.status !== 'Cancelado') {
      return false
    }
    if (!termo) {
      return true
    }
    return normalizarTexto(
      `${titulo.numeroDocumento} ${titulo.descricao} ${titulo.contraparteNome} ${titulo.pedidoId ?? ''}`,
    ).includes(termo)
  })

  const colunas: Array<Coluna<TituloFinanceiro>> = [
    {
      chave: 'documento',
      cabecalho: 'Documento',
      larguraMinima: '11rem',
      valor: (titulo) => `${titulo.numeroDocumento}-${String(titulo.numeroParcela).padStart(3, '0')}`,
      celula: (titulo) => (
        <button type="button" onClick={() => aoVerDetalhe(titulo)} className="group grid max-w-[18rem] text-left">
          <span className="font-medium text-ink group-hover:text-brand-ink group-hover:underline">
            <span className="numerico">{titulo.numeroDocumento}</span>
            <span className="numerico ml-1.5 text-xs font-normal text-ink-muted">
              {titulo.numeroParcela}/{titulo.totalParcelas}
            </span>
          </span>
          <span className="truncate text-[0.8125rem] text-ink-muted">{titulo.descricao}</span>
        </button>
      ),
    },
    {
      chave: 'contraparte',
      cabecalho: ehReceber ? 'Cliente' : 'Fornecedor',
      larguraMinima: '10rem',
      valor: (titulo) => titulo.contraparteNome,
      celula: (titulo) => <span className="text-pretty text-ink-soft">{titulo.contraparteNome}</span>,
    },
    {
      chave: 'vencimento',
      cabecalho: 'Vencimento',
      valor: (titulo) => titulo.dataVencimento,
      celula: (titulo) => {
        const situacao = situacaoDo(titulo, referencia)
        return (
          <div className="whitespace-nowrap">
            <p className="numerico text-ink">{formatarDataISO(titulo.dataVencimento)}</p>
            {estaEmAberto(titulo) && (
              <p className={cn('text-xs', situacao === 'Vencido' ? 'font-medium text-negative-ink' : 'text-ink-muted')}>
                {descreverVencimento(titulo.dataVencimento, referencia)}
              </p>
            )}
          </div>
        )
      },
    },
    {
      chave: 'valor',
      cabecalho: 'Valor',
      alinhamento: 'fim',
      ocultarEmTelaPequena: true,
      valor: (titulo) => titulo.valorOriginal,
      celula: (titulo) => (
        <span className="numerico whitespace-nowrap text-ink-soft">{formatarMoeda(titulo.valorOriginal)}</span>
      ),
    },
    {
      chave: 'saldo',
      cabecalho: 'Saldo',
      alinhamento: 'fim',
      valor: (titulo) => saldoDo(titulo),
      celula: (titulo) => (
        <span
          className={cn(
            'numerico font-semibold whitespace-nowrap',
            saldoDo(titulo) === 0 ? 'text-ink-muted' : 'text-ink',
          )}
        >
          {titulo.status === 'Cancelado' ? '—' : formatarMoeda(saldoDo(titulo))}
        </span>
      ),
    },
    {
      chave: 'situacao',
      cabecalho: 'Situação',
      valor: (titulo) => situacaoDo(titulo, referencia),
      celula: (titulo) => {
        const situacao = situacaoDo(titulo, referencia)
        return (
          <Etiqueta tom={tonsDeSituacao[situacao]} comPonto>
            {situacao === 'Baixado' ? rotuloDeBaixado(tipo) : rotulosDeSituacao[situacao]}
          </Etiqueta>
        )
      },
    },
    {
      chave: 'acoes',
      cabecalho: <span className="sr-only">Ações</span>,
      alinhamento: 'fim',
      celula: (titulo) => {
        const aberto = estaEmAberto(titulo)
        return (
          <div className="flex items-center justify-end gap-1">
            {aberto && (
              <Botao
                variante="contorno"
                tamanho="pequeno"
                onClick={() => aoBaixar(titulo)}
                iconeInicial={<HandCoins aria-hidden className="size-3.5" />}
                // Atalho só em tela larga; nas outras a ação fica no menu e no detalhe.
                className="hidden 2xl:inline-flex"
              >
                {verbo}
              </Botao>
            )}
            <Menu rotulo={`Ações de ${titulo.numeroDocumento} parcela ${titulo.numeroParcela}`}>
              {(fechar) => (
                <>
                  <ItemDeMenu
                    icone={<Eye aria-hidden className="size-4" />}
                    onClick={() => {
                      fechar()
                      aoVerDetalhe(titulo)
                    }}
                  >
                    Ver detalhes
                  </ItemDeMenu>
                  <ItemDeMenu
                    disabled={!aberto}
                    icone={<HandCoins aria-hidden className="size-4" />}
                    onClick={() => {
                      fechar()
                      aoBaixar(titulo)
                    }}
                  >
                    {ehReceber ? 'Registrar recebimento' : 'Registrar pagamento'}
                  </ItemDeMenu>
                  <SeparadorDeMenu />
                  <ItemDeMenu
                    tom="perigo"
                    disabled={!aberto || titulo.valorBaixado > 0}
                    icone={<Ban aria-hidden className="size-4" />}
                    onClick={() => {
                      fechar()
                      aoCancelar(titulo)
                    }}
                  >
                    Cancelar título
                  </ItemDeMenu>
                </>
              )}
            </Menu>
          </div>
        )
      },
    },
  ]

  return (
    <>
      <div className="grid grid-cols-1 gap-3 min-[420px]:grid-cols-2 sm:gap-4 xl:grid-cols-4">
        <Indicador
          rotulo={ehReceber ? 'A receber' : 'A pagar'}
          Icone={Wallet}
          destaque
          valor={formatarMoedaCompacta(resumo.aberto)}
          apoio={`${resumo.quantidadeAberta} ${pluralizar(resumo.quantidadeAberta, 'título em aberto', 'títulos em aberto')}`}
        />
        <Indicador
          rotulo="Vencido"
          Icone={TriangleAlert}
          valor={formatarMoedaCompacta(resumo.vencido)}
          apoio={
            resumo.quantidadeVencida > 0 ? (
              <span className="text-negative-ink">
                {resumo.quantidadeVencida} {pluralizar(resumo.quantidadeVencida, 'título atrasado', 'títulos atrasados')}
              </span>
            ) : (
              'Nada em atraso'
            )
          }
        />
        <Indicador
          rotulo="Vence em 7 dias"
          Icone={AlarmClock}
          valor={formatarMoedaCompacta(resumo.semana)}
          apoio={`${resumo.quantidadeSemana} ${pluralizar(resumo.quantidadeSemana, 'título', 'títulos')} até ${formatarDataISO(adicionarDias(referencia, 7))}`}
        />
        <Indicador
          rotulo={ehReceber ? 'Recebido em 30 dias' : 'Pago em 30 dias'}
          Icone={CheckCircle2}
          valor={formatarMoedaCompacta(resumo.baixadoNoPeriodo)}
          apoio={`${resumo.quantidadeBaixas} ${pluralizar(resumo.quantidadeBaixas, 'baixa registrada', 'baixas registradas')}`}
        />
      </div>

      <Cartao>
        <div className="flex flex-wrap items-center justify-between gap-3 border-b border-line px-4 py-3">
          <CampoDeBusca
            rotulo={`Buscar contas a ${verbo.toLowerCase()}`}
            placeholder={`Documento, descrição ou ${ehReceber ? 'cliente' : 'fornecedor'}`}
            valor={busca}
            aoMudar={definirBusca}
          />
          <Segmentos
            rotulo="Filtrar títulos"
            valor={filtro}
            aoMudar={definirFiltro}
            opcoes={[
              { valor: 'abertos', rotulo: 'Em aberto', contagem: contagens.abertos },
              { valor: 'vencidos', rotulo: 'Vencidos', contagem: contagens.vencidos },
              { valor: 'semana', rotulo: 'Próximos 7 dias', contagem: contagens.semana },
              { valor: 'baixados', rotulo: ehReceber ? 'Recebidos' : 'Pagos', contagem: contagens.baixados },
              { valor: 'cancelados', rotulo: 'Cancelados', contagem: contagens.cancelados },
              { valor: 'todos', rotulo: 'Todos', contagem: contagens.todos },
            ]}
          />
        </div>

        <Tabela
          legenda={ehReceber ? 'Contas a receber' : 'Contas a pagar'}
          itens={visiveis}
          colunas={colunas}
          chaveDoItem={(titulo) => titulo.id}
          ordenacaoInicial={{ chave: 'vencimento', direcao: 'crescente' }}
          vazio={
            <EstadoVazio
              icone={<Receipt aria-hidden className="size-5" />}
              titulo={titulos.length === 0 ? 'Nenhum título' : 'Nenhum título corresponde ao filtro'}
              descricao={
                filtro === 'vencidos' && !termo
                  ? 'Nenhum título em atraso.'
                  : 'Ajuste a busca ou escolha outro filtro.'
              }
            />
          }
        />
      </Cartao>
    </>
  )
}

function ListaDePrazos({
  prazos,
  titulos,
  aoEditar,
  aoDuplicar,
  aoAlternarStatus,
}: {
  prazos: PrazoPagamento[]
  titulos: TituloFinanceiro[]
  aoEditar: (prazo: PrazoPagamento) => void
  aoDuplicar: (prazo: PrazoPagamento) => void
  aoAlternarStatus: (prazo: PrazoPagamento) => void
}) {
  const exemplo = 1_000
  const ordenados = [...prazos].sort((a, b) => Number(b.ativo) - Number(a.ativo) || a.id - b.id)

  return (
    <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
      {ordenados.map((prazo) => {
        const documentosQueUsam = new Set(
          titulos.filter((titulo) => titulo.prazoNome === prazo.nome).map((titulo) => `${titulo.tipo}-${titulo.numeroDocumento}`),
        ).size
        const parcelas = gerarParcelas(exemplo, hoje(), prazo.parcelas)

        return (
          <Cartao key={prazo.id} className={cn('flex flex-col', !prazo.ativo && 'opacity-70')}>
            <div className="flex items-start justify-between gap-3 border-b border-line px-5 py-4">
              <div className="min-w-0">
                <div className="flex flex-wrap items-center gap-2">
                  <h2 className="text-[0.9375rem] font-semibold tracking-tight text-ink">{prazo.nome}</h2>
                  <Etiqueta tom={prazo.ativo ? 'positivo' : 'neutro'} comPonto>
                    {prazo.ativo ? 'Ativo' : 'Inativo'}
                  </Etiqueta>
                </div>
                <p className="numerico mt-0.5 text-xs text-ink-muted">
                  Cód. {prazo.id} · {prazo.parcelas.length} {pluralizar(prazo.parcelas.length, 'parcela', 'parcelas')} ·{' '}
                  {resumirDias(prazo)}
                </p>
              </div>
              <Menu rotulo={`Ações do prazo ${prazo.nome}`}>
                {(fechar) => (
                  <>
                    <ItemDeMenu
                      icone={<PencilLine aria-hidden className="size-4" />}
                      onClick={() => {
                        fechar()
                        aoEditar(prazo)
                      }}
                    >
                      Editar
                    </ItemDeMenu>
                    <ItemDeMenu
                      icone={<Copy aria-hidden className="size-4" />}
                      onClick={() => {
                        fechar()
                        aoDuplicar(prazo)
                      }}
                    >
                      Duplicar
                    </ItemDeMenu>
                    <SeparadorDeMenu />
                    <ItemDeMenu
                      tom={prazo.ativo ? 'perigo' : 'neutro'}
                      icone={prazo.ativo ? <Ban aria-hidden className="size-4" /> : <CheckCircle2 aria-hidden className="size-4" />}
                      onClick={() => {
                        fechar()
                        aoAlternarStatus(prazo)
                      }}
                    >
                      {prazo.ativo ? 'Inativar' : 'Ativar'}
                    </ItemDeMenu>
                  </>
                )}
              </Menu>
            </div>

            <div className="grid flex-1 gap-4 px-5 py-4">
              <div aria-hidden className="flex h-2.5 overflow-hidden rounded-full bg-surface-3">
                {prazo.parcelas.map((parcela, indice) => (
                  <div
                    key={parcela.numero}
                    className={cn('h-full border-r-2 border-surface last:border-r-0', indice % 2 === 0 ? 'bg-brand' : 'bg-brand/55')}
                    style={{ width: `${parcela.percentual}%` }}
                  />
                ))}
              </div>

              <ol className="grid gap-1.5">
                {parcelas.map((parcela) => (
                  <li key={parcela.numero} className="flex items-center gap-3 text-sm">
                    <span className="numerico grid size-6 shrink-0 place-items-center rounded-full bg-surface-2 text-xs font-semibold text-ink-soft">
                      {parcela.numero}
                    </span>
                    <span className="min-w-0 flex-1 whitespace-nowrap text-ink-soft">
                      {parcela.dias === 0 ? 'À vista' : `${parcela.dias} dias`}
                    </span>
                    <span className="numerico shrink-0 text-right text-ink-muted">{formatarPercentual(parcela.percentual)}</span>
                    <span className="numerico min-w-[5.5rem] shrink-0 text-right font-medium whitespace-nowrap text-ink">
                      {formatarMoeda(parcela.valor)}
                    </span>
                  </li>
                ))}
              </ol>
            </div>

            <div className="flex items-center justify-between gap-2 border-t border-line bg-surface-2/40 px-5 py-2.5 text-xs text-ink-muted">
              <span className="inline-flex items-center gap-1.5">
                <CalendarRange aria-hidden className="size-3.5" />
                Exemplo com {formatarMoeda(exemplo)}
              </span>
              <span>
                {documentosQueUsam === 0
                  ? 'Sem uso'
                  : `${documentosQueUsam} ${pluralizar(documentosQueUsam, 'documento', 'documentos')}`}
              </span>
            </div>
          </Cartao>
        )
      })}
    </div>
  )
}
