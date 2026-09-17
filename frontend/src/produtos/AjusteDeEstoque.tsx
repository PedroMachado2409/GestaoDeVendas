import { useMutation, useQueryClient } from '@tanstack/react-query'
import { ArrowDownRight, ArrowRight, ArrowUpRight, Minus, Plus } from 'lucide-react'
import { useEffect, useState } from 'react'
import { toast } from 'sonner'

import { produtosApi } from '@/api/recursos'
import type { Produto } from '@/api/tipos'
import { Botao } from '@/componentes/ui/Botao'
import { Campo, Entrada } from '@/componentes/ui/Campo'
import { Segmentos } from '@/componentes/ui/Filtros'
import { Modal } from '@/componentes/ui/Modal'
import { notificarErro } from '@/lib/notificacoes'
import { cn, formatarNumero } from '@/lib/utils'

type Sentido = 'entrada' | 'saida'

interface Propriedades {
  produto: Produto | null
  aoFechar: () => void
}

/**
 * A API recebe um inteiro com sinal. Na tela o sentido é escolhido à parte e a
 * quantidade é sempre positiva — é mais difícil errar o sinal assim.
 */
export function AjusteDeEstoque({ produto, aoFechar }: Propriedades) {
  const clienteDeConsultas = useQueryClient()
  const [sentido, definirSentido] = useState<Sentido>('entrada')
  const [quantidadeTexto, definirQuantidadeTexto] = useState('')

  useEffect(() => {
    if (produto) {
      definirSentido('entrada')
      definirQuantidadeTexto('')
    }
  }, [produto])

  const quantidade = Number(quantidadeTexto)
  const disponivel = produto?.estoque ?? 0
  const delta = sentido === 'entrada' ? quantidade : -quantidade
  const resultado = disponivel + (Number.isFinite(delta) ? delta : 0)

  const erro = !quantidadeTexto
    ? undefined
    : !Number.isInteger(quantidade) || quantidade <= 0
      ? 'Informe um número inteiro maior que zero.'
      : resultado < 0
        ? `A saída não pode passar do disponível (${formatarNumero(disponivel)}).`
        : undefined

  const valido = Boolean(quantidadeTexto) && !erro

  const ajustar = useMutation({
    mutationFn: () => produtosApi.ajustarEstoque(produto!.id, { quantidade: delta }),
    onSuccess: (atualizado) => {
      toast.success('Estoque ajustado', {
        description: `${atualizado.nome}: ${formatarNumero(disponivel)} → ${formatarNumero(atualizado.estoque)}`,
      })
      void clienteDeConsultas.invalidateQueries({ queryKey: ['produtos'] })
      void clienteDeConsultas.invalidateQueries({ queryKey: ['movimentacoes', atualizado.id] })
      aoFechar()
    },
    onError: (causa) => notificarErro(causa, 'Não foi possível ajustar o estoque'),
  })

  function enviar(evento?: React.FormEvent) {
    evento?.preventDefault()
    if (valido && !ajustar.isPending) ajustar.mutate()
  }

  return (
    <Modal
      aberto={produto !== null}
      aoFechar={aoFechar}
      titulo="Ajustar estoque"
      descricao={
        produto
          ? `${produto.nome} · ${produto.marca}. O ajuste fica registrado no histórico e não pode ser apagado — um erro se corrige com outro ajuste.`
          : undefined
      }
      largura="estreita"
      rodape={
        <>
          <Botao variante="sutil" onClick={aoFechar} disabled={ajustar.isPending}>
            Cancelar
          </Botao>
          <Botao
            variante="primario"
            onClick={() => enviar()}
            disabled={!valido}
            carregando={ajustar.isPending}
          >
            Registrar ajuste
          </Botao>
        </>
      }
    >
      <form onSubmit={enviar} className="grid gap-5" noValidate>
        <div className="grid gap-1.5">
          <span className="text-[0.8125rem] font-medium text-ink-soft">Sentido</span>
          <Segmentos
            rotulo="Sentido do ajuste"
            valor={sentido}
            aoMudar={definirSentido}
            opcoes={[
              { valor: 'entrada', rotulo: 'Entrada' },
              { valor: 'saida', rotulo: 'Saída' },
            ]}
          />
        </div>

        <Campo rotulo="Quantidade" erro={erro} obrigatorio>
          {(propriedades) => (
            <Entrada
              {...propriedades}
              type="number"
              min={1}
              step={1}
              inputMode="numeric"
              placeholder="0"
              autoFocus
              value={quantidadeTexto}
              onChange={(evento) => definirQuantidadeTexto(evento.target.value)}
              prefixo={
                sentido === 'entrada' ? (
                  <Plus aria-hidden className="size-4" />
                ) : (
                  <Minus aria-hidden className="size-4" />
                )
              }
              className="numerico"
            />
          )}
        </Campo>

        <div
          aria-live="polite"
          className="flex items-center justify-between gap-3 rounded-xl border border-line bg-surface-2/60 px-4 py-3"
        >
          <div>
            <p className="text-xs text-ink-muted">Disponível hoje</p>
            <p className="numerico text-lg font-semibold tracking-tight text-ink">
              {formatarNumero(disponivel)}
            </p>
          </div>
          <span
            aria-hidden
            className={cn(
              'grid size-8 place-items-center rounded-full',
              !valido
                ? 'bg-surface-3 text-ink-muted'
                : sentido === 'entrada'
                  ? 'bg-positive-soft text-positive-ink'
                  : 'bg-negative-soft text-negative-ink',
            )}
          >
            {!valido ? (
              <ArrowRight className="size-4" />
            ) : sentido === 'entrada' ? (
              <ArrowUpRight className="size-4" />
            ) : (
              <ArrowDownRight className="size-4" />
            )}
          </span>
          <div className="text-right">
            <p className="text-xs text-ink-muted">Após o ajuste</p>
            <p
              className={cn(
                'numerico text-lg font-semibold tracking-tight',
                valido ? 'text-ink' : 'text-ink-muted',
              )}
            >
              {valido ? formatarNumero(resultado) : '—'}
            </p>
          </div>
        </div>

        {produto && produto.quantidadeReservada > 0 && (
          <p className="text-xs text-ink-muted">
            {formatarNumero(produto.quantidadeReservada)} unidades em reserva de pedidos abertos não
            entram no ajuste.
          </p>
        )}
      </form>
    </Modal>
  )
}
