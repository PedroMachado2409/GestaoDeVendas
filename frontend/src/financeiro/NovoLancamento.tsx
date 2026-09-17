import { AlertCircle, CalendarClock } from 'lucide-react'
import { useEffect, useState } from 'react'

import { Botao } from '@/componentes/ui/Botao'
import { Campo, Entrada } from '@/componentes/ui/Campo'
import { Segmentos } from '@/componentes/ui/Filtros'
import { Modal } from '@/componentes/ui/Modal'
import { SeletorComPesquisa } from '@/componentes/ui/SeletorComPesquisa'
import { formatarMoeda } from '@/lib/utils'

import { descreverVencimento, formatarDataISO, hoje } from './datas'
import { descreverParcelas, formatarPercentual, gerarParcelas, lerNumero } from './prazos'
import type { NovoTitulo } from './contexto'
import type { Contraparte, DataISO, PrazoPagamento, TipoTitulo } from './tipos'

interface Propriedades {
  aberto: boolean
  tipoInicial: TipoTitulo
  prazos: PrazoPagamento[]
  clientes: Contraparte[]
  fornecedores: Contraparte[]
  proximoNumeroDocumento: string
  aoSalvar: (titulos: NovoTitulo[]) => void
  aoFechar: () => void
}

export function NovoLancamento({
  aberto,
  tipoInicial,
  prazos,
  clientes,
  fornecedores,
  proximoNumeroDocumento,
  aoSalvar,
  aoFechar,
}: Propriedades) {
  const [tipo, definirTipo] = useState<TipoTitulo>(tipoInicial)
  const [descricao, definirDescricao] = useState('')
  const [contraparteId, definirContraparteId] = useState<number | null>(null)
  const [valorTexto, definirValorTexto] = useState('')
  const [dataBase, definirDataBase] = useState<DataISO>(hoje())
  const [prazoId, definirPrazoId] = useState<number | null>(null)
  const [observacao, definirObservacao] = useState('')
  const [tentouSalvar, definirTentouSalvar] = useState(false)

  useEffect(() => {
    if (aberto) {
      definirTipo(tipoInicial)
      definirDescricao('')
      definirContraparteId(null)
      definirValorTexto('')
      definirDataBase(hoje())
      definirPrazoId(null)
      definirObservacao('')
      definirTentouSalvar(false)
    }
  }, [aberto, tipoInicial])

  const contrapartes = tipo === 'Receber' ? clientes : fornecedores
  const contraparte = contrapartes.find((item) => item.id === contraparteId) ?? null
  const prazo = prazos.find((item) => item.id === prazoId) ?? null
  const valor = lerNumero(valorTexto)
  const valorValido = Number.isFinite(valor) && valor > 0 && Math.round(valor * 100) === valor * 100
  const parcelas = prazo && valorValido && dataBase ? gerarParcelas(valor, dataBase, prazo.parcelas) : []

  const erros = {
    descricao: descricao.trim() ? undefined : 'Informe a descrição.',
    contraparte: contraparte ? undefined : `Escolha o ${tipo === 'Receber' ? 'cliente' : 'fornecedor'}.`,
    valor: valorValido ? undefined : 'Informe um valor maior que zero, com até 2 casas decimais.',
    dataBase: dataBase ? undefined : 'Informe a data base.',
    prazo: prazo ? undefined : 'Escolha o prazo de pagamento.',
  }
  const valido = Object.values(erros).every((erro) => erro === undefined)
  const mostrar = (erro: string | undefined) => (tentouSalvar ? erro : undefined)

  function mudarTipo(novo: TipoTitulo) {
    definirTipo(novo)
    // Cliente e fornecedor são cadastros diferentes; o escolhido não vale mais.
    definirContraparteId(null)
  }

  function salvar() {
    definirTentouSalvar(true)
    if (!valido || !prazo || !contraparte) {
      return
    }

    aoSalvar(
      parcelas.map((parcela) => ({
        tipo,
        descricao: descricao.trim(),
        numeroDocumento: proximoNumeroDocumento,
        contraparteId: contraparte.id,
        contraparteNome: contraparte.nome,
        pedidoId: null,
        origem: 'LancamentoManual',
        numeroParcela: parcela.numero,
        totalParcelas: parcelas.length,
        prazoNome: prazo.nome,
        valorOriginal: parcela.valor,
        valorBaixado: 0,
        dataCadastro: dataBase,
        dataVencimento: parcela.vencimento,
        status: 'Aberto',
        baixas: [],
        observacao: observacao.trim() || undefined,
      })),
    )
  }

  return (
    <Modal
      aberto={aberto}
      aoFechar={aoFechar}
      titulo="Novo lançamento"
      descricao="Conta sem pedido de origem (aluguel, serviço, frete…). As parcelas saem do prazo escolhido."
      largura="larga"
      rodape={
        <>
          <div className="mr-auto flex items-baseline gap-2 max-sm:w-full max-sm:justify-between">
            <span className="text-[0.8125rem] text-ink-muted">
              {parcelas.length > 0 ? `${parcelas.length} ${parcelas.length === 1 ? 'parcela' : 'parcelas'}` : 'Total'}
            </span>
            <span className="numerico text-lg font-semibold tracking-tight text-ink">
              {formatarMoeda(valorValido ? valor : 0)}
            </span>
          </div>
          <Botao variante="sutil" onClick={aoFechar}>
            Cancelar
          </Botao>
          <Botao variante="primario" onClick={salvar} disabled={tentouSalvar && !valido}>
            Lançar
          </Botao>
        </>
      }
    >
      <form
        className="grid gap-5"
        noValidate
        onSubmit={(evento) => {
          evento.preventDefault()
          salvar()
        }}
      >
        <div className="grid gap-1.5">
          <span className="text-[0.8125rem] font-medium text-ink-soft">Tipo</span>
          <Segmentos
            rotulo="Tipo de lançamento"
            valor={tipo}
            aoMudar={mudarTipo}
            opcoes={[
              { valor: 'Receber', rotulo: 'A receber' },
              { valor: 'Pagar', rotulo: 'A pagar' },
            ]}
          />
        </div>

        <div className="grid gap-4 sm:grid-cols-2">
          <Campo rotulo="Descrição" erro={mostrar(erros.descricao)} obrigatorio>
            {(propriedades) => (
              <Entrada
                {...propriedades}
                value={descricao}
                onChange={(evento) => definirDescricao(evento.target.value)}
                placeholder={tipo === 'Receber' ? 'Ex.: Serviço de personalização' : 'Ex.: Aluguel do galpão'}
              />
            )}
          </Campo>

          <Campo rotulo="Documento" dica="Gerado automaticamente.">
            {(propriedades) => <Entrada {...propriedades} value={proximoNumeroDocumento} readOnly className="numerico" />}
          </Campo>
        </div>

        <Campo rotulo={tipo === 'Receber' ? 'Cliente' : 'Fornecedor'} erro={mostrar(erros.contraparte)} obrigatorio>
          {(propriedades) => (
            <SeletorComPesquisa
              {...propriedades}
              itens={contrapartes}
              valor={contraparteId}
              aoEscolher={(item) => definirContraparteId(item.id)}
              aoLimpar={() => definirContraparteId(null)}
              obterId={(item) => item.id}
              obterTitulo={(item) => item.nome}
              obterDescricao={(item) => item.documento}
              nomeDaEntidade={tipo === 'Receber' ? 'cliente' : 'fornecedor'}
              placeholder={tipo === 'Receber' ? 'Pesquisar cliente' : 'Pesquisar fornecedor'}
              tituloDoModal={tipo === 'Receber' ? 'Pesquisar cliente' : 'Pesquisar fornecedor'}
              descricaoDoModal="Busque por nome, documento ou código."
            />
          )}
        </Campo>

        <div className="grid gap-4 sm:grid-cols-2">
          <Campo rotulo="Valor total (R$)" erro={mostrar(erros.valor)} obrigatorio>
            {(propriedades) => (
              <Entrada
                {...propriedades}
                value={valorTexto}
                onChange={(evento) => definirValorTexto(evento.target.value.replace(/[^\d.,]/g, ''))}
                inputMode="decimal"
                placeholder="0,00"
                className="numerico"
              />
            )}
          </Campo>

          <Campo
            rotulo="Data base"
            erro={mostrar(erros.dataBase)}
            dica="Os dias do prazo contam a partir desta data."
            obrigatorio
          >
            {(propriedades) => (
              <Entrada
                {...propriedades}
                type="date"
                value={dataBase}
                onChange={(evento) => definirDataBase(evento.target.value)}
                className="numerico"
              />
            )}
          </Campo>
        </div>

        <Campo rotulo="Prazo de pagamento" erro={mostrar(erros.prazo)} obrigatorio>
          {(propriedades) => (
            <SeletorComPesquisa
              {...propriedades}
              itens={prazos}
              valor={prazoId}
              aoEscolher={(item) => definirPrazoId(item.id)}
              aoLimpar={() => definirPrazoId(null)}
              obterId={(item) => item.id}
              obterTitulo={(item) => item.nome}
              obterDescricao={descreverParcelas}
              obterMotivoIndisponivel={(item) => (item.ativo ? null : 'Inativo')}
              obterComplemento={(item) => `${item.parcelas.length}x`}
              nomeDaEntidade="prazo"
              placeholder="Pesquisar prazo"
              tituloDoModal="Pesquisar prazo de pagamento"
              descricaoDoModal="Busque pelo nome ou pelos dias (ex.: 60)."
            />
          )}
        </Campo>

        <div className="grid gap-2">
          <span className="flex items-center gap-1.5 text-[0.8125rem] font-medium text-ink-soft">
            <CalendarClock aria-hidden className="size-4 text-ink-muted" />
            Parcelas que serão geradas
          </span>
          {parcelas.length === 0 ? (
            <p className="flex items-center justify-center gap-1.5 rounded-lg border border-dashed border-line-strong px-4 py-6 text-center text-xs text-ink-muted">
              <AlertCircle aria-hidden className="size-3.5" />
              Informe o valor, a data base e o prazo para ver as parcelas.
            </p>
          ) : (
            <div className="scrollbar-fina overflow-x-auto rounded-lg border border-line">
              <table className="w-full min-w-[28rem] text-sm">
                <caption className="sr-only">Parcelas do lançamento</caption>
                <thead>
                  <tr className="border-b border-line bg-surface-2/60 text-xs font-semibold tracking-wide text-ink-muted uppercase">
                    <th scope="col" className="px-3 py-2 text-left">Parcela</th>
                    <th scope="col" className="px-3 py-2 text-left">Vencimento</th>
                    <th scope="col" className="px-3 py-2 text-right">%</th>
                    <th scope="col" className="px-3 py-2 text-right">Valor</th>
                  </tr>
                </thead>
                <tbody>
                  {parcelas.map((parcela) => (
                    <tr key={parcela.numero} className="border-b border-line/70 last:border-0">
                      <td className="numerico px-3 py-2.5 font-medium text-ink">
                        {parcela.numero}/{parcelas.length}
                      </td>
                      <td className="px-3 py-2.5">
                        <span className="numerico text-ink">{formatarDataISO(parcela.vencimento)}</span>
                        <span className="ml-2 text-xs text-ink-muted">{descreverVencimento(parcela.vencimento)}</span>
                      </td>
                      <td className="numerico px-3 py-2.5 text-right text-ink-soft">
                        {formatarPercentual(parcela.percentual)}
                      </td>
                      <td className="numerico px-3 py-2.5 text-right font-semibold text-ink">
                        {formatarMoeda(parcela.valor)}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>

        <Campo rotulo="Observação">
          {(propriedades) => (
            <Entrada
              {...propriedades}
              value={observacao}
              onChange={(evento) => definirObservacao(evento.target.value)}
              placeholder="Opcional"
            />
          )}
        </Campo>
      </form>
    </Modal>
  )
}
