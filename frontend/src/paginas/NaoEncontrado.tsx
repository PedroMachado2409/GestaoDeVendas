import { Compass } from 'lucide-react'

import { LinkBotao } from '@/componentes/ui/Botao'
import { Cartao } from '@/componentes/ui/Cartao'
import { EstadoVazio } from '@/componentes/ui/Estados'

export function NaoEncontrado() {
  return (
    <Cartao className="mx-auto w-full max-w-lg">
      <EstadoVazio
        icone={<Compass aria-hidden className="size-5" />}
        titulo="Página não encontrada"
        descricao="O endereço acessado não corresponde a nenhuma tela desta aplicação."
        acao={
          <LinkBotao to="/" variante="primario" tamanho="pequeno">
            Ir para o painel
          </LinkBotao>
        }
      />
    </Cartao>
  )
}
