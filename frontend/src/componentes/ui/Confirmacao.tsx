import type { ReactNode } from 'react'

import { Botao, type VarianteBotao } from './Botao'
import { Modal } from './Modal'

interface Propriedades {
  aberto: boolean
  titulo: string
  descricao?: ReactNode
  rotuloConfirmar?: string
  varianteConfirmar?: VarianteBotao
  processando?: boolean
  aoConfirmar: () => void
  aoCancelar: () => void
}

export function Confirmacao({
  aberto,
  titulo,
  descricao,
  rotuloConfirmar = 'Confirmar',
  varianteConfirmar = 'primario',
  processando = false,
  aoConfirmar,
  aoCancelar,
}: Propriedades) {
  return (
    <Modal
      aberto={aberto}
      aoFechar={aoCancelar}
      titulo={titulo}
      largura="estreita"
      rodape={
        <>
          <Botao variante="sutil" onClick={aoCancelar} disabled={processando}>
            Cancelar
          </Botao>
          <Botao variante={varianteConfirmar} onClick={aoConfirmar} carregando={processando}>
            {rotuloConfirmar}
          </Botao>
        </>
      }
    >
      <div className="text-sm text-ink-soft">{descricao}</div>
    </Modal>
  )
}
