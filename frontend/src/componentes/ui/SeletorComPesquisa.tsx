import { AlertCircle, CornerDownLeft, Search, SearchX, X } from 'lucide-react'
import { useEffect, useId, useMemo, useRef, useState, type KeyboardEvent, type ReactNode } from 'react'

import { cn, normalizarTexto } from '@/lib/utils'

import { Botao } from './Botao'
import { Entrada } from './Campo'
import { Etiqueta } from './Etiqueta'
import { Modal } from './Modal'

interface Propriedades<T> {
  itens: T[]
  obterId: (item: T) => number
  obterTitulo: (item: T) => string
  /** Linha secundária no campo e na lista (e-mail, marca…). */
  obterDescricao?: (item: T) => string
  /** Conteúdo à direita na lista (preço, estoque…). */
  obterComplemento?: (item: T) => ReactNode
  /** Texto extra considerado na busca, além do título, descrição e código. */
  obterTextoDeBusca?: (item: T) => string
  /** Motivo para o item não poder ser escolhido, ou null se pode. */
  obterMotivoIndisponivel?: (item: T) => string | null

  valor: number | null
  aoEscolher: (item: T) => void
  aoLimpar?: () => void

  /** Ex.: "cliente", "produto" — usado nas mensagens. */
  nomeDaEntidade: string
  placeholder: string
  tituloDoModal: string
  descricaoDoModal?: string
  /**
   * Modo "adicionar": o campo volta a ficar vazio depois de cada escolha,
   * pronto para o próximo código.
   */
  limparAoEscolher?: boolean
  carregando?: boolean
  desabilitado?: boolean

  // Repassados pelo <Campo>, para o rótulo apontar para o campo de código.
  id?: string
  'aria-invalid'?: boolean
  'aria-describedby'?: string
}

/**
 * Substitui o <select> nativo para escolher registros.
 *
 * Dois caminhos para o mesmo resultado: quem sabe o código digita e aperta
 * Enter; quem não sabe abre a pesquisa. O <select> obrigava a rolar uma lista
 * sem busca e não mostrava o código.
 */
export function SeletorComPesquisa<T>({
  itens,
  obterId,
  obterTitulo,
  obterDescricao,
  obterComplemento,
  obterTextoDeBusca,
  obterMotivoIndisponivel,
  valor,
  aoEscolher,
  aoLimpar,
  nomeDaEntidade,
  placeholder,
  tituloDoModal,
  descricaoDoModal,
  limparAoEscolher = false,
  carregando = false,
  desabilitado = false,
  id,
  'aria-invalid': invalido,
  'aria-describedby': descritoPor,
}: Propriedades<T>) {
  const idDoErro = useId()
  const campoDeCodigo = useRef<HTMLInputElement>(null)
  const [codigo, definirCodigo] = useState(valor ? String(valor) : '')
  const [erro, definirErro] = useState<string | null>(null)
  const [pesquisaAberta, definirPesquisaAberta] = useState(false)
  const [buscaInicial, definirBuscaInicial] = useState('')

  // Mantém o código em sincronia quando o valor muda de fora (reset do formulário).
  useEffect(() => {
    definirCodigo(valor ? String(valor) : '')
  }, [valor])

  const selecionado = valor ? (itens.find((item) => obterId(item) === valor) ?? null) : null
  const inativo = desabilitado || carregando

  function escolher(item: T) {
    aoEscolher(item)
    definirErro(null)
    definirPesquisaAberta(false)
    if (limparAoEscolher) {
      definirCodigo('')
      campoDeCodigo.current?.focus()
    } else {
      definirCodigo(String(obterId(item)))
    }
  }

  function confirmarCodigo() {
    if (!codigo) {
      abrirPesquisa('')
      return
    }

    const numero = Number(codigo)
    const item = itens.find((candidato) => obterId(candidato) === numero)

    if (!item) {
      definirErro(`Nenhum ${nomeDaEntidade} com o código ${numero}.`)
      return
    }

    const motivo = obterMotivoIndisponivel?.(item)
    if (motivo) {
      definirErro(`${obterTitulo(item)}: ${motivo.toLowerCase()}.`)
      return
    }

    escolher(item)
  }

  function abrirPesquisa(texto: string) {
    definirBuscaInicial(texto)
    definirPesquisaAberta(true)
  }

  function aoDigitarCodigo(texto: string) {
    definirErro(null)
    // Letras não são código: a pessoa está procurando pelo nome.
    if (/[^\d\s]/.test(texto)) {
      abrirPesquisa(texto)
      definirCodigo(valor && !limparAoEscolher ? String(valor) : '')
      return
    }
    definirCodigo(texto.replace(/\D/g, ''))
  }

  function aoTeclarNoCodigo(evento: KeyboardEvent<HTMLInputElement>) {
    if (evento.key === 'Enter') {
      evento.preventDefault()
      confirmarCodigo()
    } else if (evento.key === 'F2' || (evento.key === 'ArrowDown' && evento.altKey)) {
      evento.preventDefault()
      abrirPesquisa('')
    }
  }

  const descricaoSelecionado = selecionado ? obterDescricao?.(selecionado) : undefined
  const descritores = [erro ? idDoErro : undefined, descritoPor].filter(Boolean).join(' ') || undefined

  return (
    <div className="grid gap-1.5">
      <div className="flex items-stretch gap-2">
        <div className="w-28 shrink-0 sm:w-32">
          <Entrada
            ref={campoDeCodigo}
            id={id}
            value={codigo}
            onChange={(evento) => aoDigitarCodigo(evento.target.value)}
            onKeyDown={aoTeclarNoCodigo}
            disabled={inativo}
            inputMode="numeric"
            autoComplete="off"
            placeholder="Código"
            aria-label={`Código do ${nomeDaEntidade}`}
            aria-invalid={Boolean(erro) || invalido}
            aria-describedby={descritores}
            className="numerico"
            sufixo={
              <button
                type="button"
                onClick={confirmarCodigo}
                disabled={inativo}
                aria-label={codigo ? 'Confirmar código' : `Pesquisar ${nomeDaEntidade}`}
                title={codigo ? 'Confirmar código (Enter)' : `Pesquisar ${nomeDaEntidade}`}
                className="grid size-9 place-items-center rounded-lg text-ink-muted transition-colors hover:text-brand-ink disabled:opacity-50"
              >
                <CornerDownLeft aria-hidden className="size-4" />
              </button>
            }
          />
        </div>

        <button
          type="button"
          onClick={() => abrirPesquisa('')}
          disabled={inativo}
          aria-haspopup="dialog"
          className={cn(
            'group flex h-9.5 min-w-0 flex-1 items-center gap-2 rounded-lg border bg-surface px-3 text-left text-sm transition-[border-color,box-shadow] outline-none',
            'border-line hover:border-line-strong focus-visible:border-brand disabled:cursor-not-allowed disabled:bg-surface-2',
          )}
        >
          <span className="flex min-w-0 flex-1 items-baseline gap-2">
            {carregando ? (
              <span className="text-ink-muted">Carregando…</span>
            ) : selecionado ? (
              <>
                <span className="truncate font-medium text-ink">{obterTitulo(selecionado)}</span>
                {descricaoSelecionado && (
                  <span className="hidden truncate text-xs text-ink-muted sm:inline">
                    {descricaoSelecionado}
                  </span>
                )}
              </>
            ) : valor ? (
              <span className="numerico text-ink-muted">Código {valor}</span>
            ) : (
              <span className="truncate text-ink-muted">{placeholder}</span>
            )}
          </span>
          <Search
            aria-hidden
            className="size-4 shrink-0 text-ink-muted transition-colors group-hover:text-ink"
          />
        </button>

        {aoLimpar && valor && !inativo && (
          <Botao
            variante="sutil"
            tamanho="icone"
            onClick={() => {
              aoLimpar()
              definirErro(null)
              campoDeCodigo.current?.focus()
            }}
            aria-label={`Limpar ${nomeDaEntidade}`}
            className="h-9.5 shrink-0"
          >
            <X aria-hidden className="size-4" />
          </Botao>
        )}
      </div>

      {erro && (
        <p id={idDoErro} role="alert" className="flex items-start gap-1 text-xs text-negative-ink">
          <AlertCircle aria-hidden className="mt-px size-3.5 shrink-0" />
          {erro}
        </p>
      )}

      <ModalDePesquisa
        aberto={pesquisaAberta}
        buscaInicial={buscaInicial}
        itens={itens}
        valor={valor}
        obterId={obterId}
        obterTitulo={obterTitulo}
        obterDescricao={obterDescricao}
        obterComplemento={obterComplemento}
        obterTextoDeBusca={obterTextoDeBusca}
        obterMotivoIndisponivel={obterMotivoIndisponivel}
        titulo={tituloDoModal}
        descricao={descricaoDoModal}
        nomeDaEntidade={nomeDaEntidade}
        aoEscolher={escolher}
        aoFechar={() => definirPesquisaAberta(false)}
      />
    </div>
  )
}

interface PropriedadesModal<T>
  extends Pick<
    Propriedades<T>,
    | 'itens'
    | 'valor'
    | 'obterId'
    | 'obterTitulo'
    | 'obterDescricao'
    | 'obterComplemento'
    | 'obterTextoDeBusca'
    | 'obterMotivoIndisponivel'
    | 'nomeDaEntidade'
  > {
  aberto: boolean
  buscaInicial: string
  titulo: string
  descricao?: string
  aoEscolher: (item: T) => void
  aoFechar: () => void
}

function ModalDePesquisa<T>({
  aberto,
  buscaInicial,
  itens,
  valor,
  obterId,
  obterTitulo,
  obterDescricao,
  obterComplemento,
  obterTextoDeBusca,
  obterMotivoIndisponivel,
  nomeDaEntidade,
  titulo,
  descricao,
  aoEscolher,
  aoFechar,
}: PropriedadesModal<T>) {
  const idDaLista = useId()
  const campoDeBusca = useRef<HTMLInputElement>(null)
  const lista = useRef<HTMLUListElement>(null)
  const [busca, definirBusca] = useState('')
  const [ativo, definirAtivo] = useState(0)

  // Roda depois do showModal() do <Modal> (efeito do filho vem antes), então o
  // foco vai para a busca e não para o botão de fechar.
  useEffect(() => {
    if (aberto) {
      definirBusca(buscaInicial)
      definirAtivo(0)
      campoDeBusca.current?.focus()
    }
  }, [aberto, buscaInicial])

  const resultados = useMemo(() => {
    const termo = normalizarTexto(busca)
    if (!termo) {
      return itens
    }

    const codigo = /^\d+$/.test(termo) ? Number(termo) : null
    const encontrados = itens.filter((item) => {
      const texto = normalizarTexto(
        [
          String(obterId(item)),
          obterTitulo(item),
          obterDescricao?.(item) ?? '',
          obterTextoDeBusca?.(item) ?? '',
        ].join(' '),
      )
      return termo.split(/\s+/).every((parte) => texto.includes(parte))
    })

    // Código exato primeiro: quem digitou "12" quer o 12, não o 112.
    return codigo === null
      ? encontrados
      : [...encontrados].sort(
          (a, b) => Number(obterId(b) === codigo) - Number(obterId(a) === codigo),
        )
  }, [itens, busca, obterId, obterTitulo, obterDescricao, obterTextoDeBusca])

  useEffect(() => {
    lista.current
      ?.querySelector(`[data-indice="${ativo}"]`)
      ?.scrollIntoView({ block: 'nearest' })
  }, [ativo])

  function mover(passo: number) {
    if (resultados.length === 0) {
      return
    }
    definirAtivo((atual) => (atual + passo + resultados.length) % resultados.length)
  }

  function tentarEscolher(item: T | undefined) {
    if (item && !obterMotivoIndisponivel?.(item)) {
      aoEscolher(item)
    }
  }

  function aoTeclar(evento: KeyboardEvent<HTMLInputElement>) {
    if (evento.key === 'ArrowDown') {
      evento.preventDefault()
      mover(1)
    } else if (evento.key === 'ArrowUp') {
      evento.preventDefault()
      mover(-1)
    } else if (evento.key === 'Enter') {
      evento.preventDefault()
      tentarEscolher(resultados[ativo])
    }
  }

  const idDoAtivo = resultados[ativo] ? `${idDaLista}-${obterId(resultados[ativo])}` : undefined

  return (
    <Modal
      aberto={aberto}
      aoFechar={aoFechar}
      titulo={titulo}
      descricao={descricao}
      rodape={
        <>
          <p className="mr-auto hidden text-xs text-ink-muted sm:block">
            <Tecla>↑</Tecla> <Tecla>↓</Tecla> navegar · <Tecla>Enter</Tecla> escolher ·{' '}
            <Tecla>Esc</Tecla> fechar
          </p>
          <Botao variante="secundario" onClick={aoFechar}>
            Cancelar
          </Botao>
        </>
      }
    >
      <div className="sticky -top-4 z-10 -mx-5 -mt-4 border-b border-line bg-surface px-5 pt-4 pb-3">
        <Entrada
          ref={campoDeBusca}
          type="search"
          role="combobox"
          aria-expanded
          aria-controls={idDaLista}
          aria-activedescendant={idDoAtivo}
          aria-label={`Pesquisar ${nomeDaEntidade}`}
          placeholder="Nome, código ou outro dado…"
          value={busca}
          onChange={(evento) => {
            definirBusca(evento.target.value)
            definirAtivo(0)
          }}
          onKeyDown={aoTeclar}
          prefixo={<Search aria-hidden className="size-4" />}
        />
        <p className="mt-2 text-xs text-ink-muted" aria-live="polite">
          {resultados.length === itens.length
            ? `${itens.length} ${itens.length === 1 ? 'registro' : 'registros'}`
            : `${resultados.length} de ${itens.length}`}
        </p>
      </div>

      {resultados.length === 0 ? (
        <div className="flex flex-col items-center gap-2 px-4 py-10 text-center">
          <SearchX aria-hidden className="size-6 text-ink-muted" />
          <p className="text-sm font-medium text-ink">Nada encontrado</p>
          <p className="text-xs text-ink-muted">Tente outro nome ou confira o código.</p>
        </div>
      ) : (
        <ul ref={lista} id={idDaLista} role="listbox" aria-label={titulo} className="-mx-2 grid gap-0.5 pt-2">
          {resultados.map((item, indice) => {
            const idDoItem = obterId(item)
            const motivo = obterMotivoIndisponivel?.(item) ?? null
            const escolhido = idDoItem === valor
            const destacado = indice === ativo
            const descricaoDoItem = obterDescricao?.(item)

            return (
              <li
                key={idDoItem}
                id={`${idDaLista}-${idDoItem}`}
                data-indice={indice}
                role="option"
                aria-selected={escolhido}
                aria-disabled={Boolean(motivo)}
                onMouseMove={() => definirAtivo(indice)}
                onClick={() => tentarEscolher(item)}
                className={cn(
                  'flex items-center gap-3 rounded-lg px-2 py-2.5 transition-colors',
                  motivo ? 'cursor-not-allowed' : 'cursor-pointer',
                  destacado && 'bg-surface-2',
                  escolhido && 'bg-brand-soft',
                )}
              >
                <span
                  className={cn(
                    'numerico min-w-11 shrink-0 rounded-md border px-1.5 py-0.5 text-center text-xs font-semibold',
                    escolhido
                      ? 'border-brand/30 bg-surface text-brand-ink'
                      : 'border-line bg-surface-2 text-ink-soft',
                  )}
                >
                  {idDoItem}
                </span>

                <span className={cn('min-w-0 flex-1', motivo && 'opacity-55')}>
                  <span className="block truncate text-sm font-medium text-ink">
                    {obterTitulo(item)}
                  </span>
                  {descricaoDoItem && (
                    <span className="block truncate text-xs text-ink-muted">{descricaoDoItem}</span>
                  )}
                </span>

                {motivo ? (
                  <Etiqueta tom="neutro" className="shrink-0">
                    {motivo}
                  </Etiqueta>
                ) : (
                  obterComplemento && (
                    <span className="shrink-0 text-right text-xs text-ink-soft">
                      {obterComplemento(item)}
                    </span>
                  )
                )}
              </li>
            )
          })}
        </ul>
      )}
    </Modal>
  )
}

function Tecla({ children }: { children: ReactNode }) {
  return (
    <kbd className="rounded border border-line bg-surface px-1 font-sans text-[0.6875rem] text-ink-soft">
      {children}
    </kbd>
  )
}
