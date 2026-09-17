# TAREFA-07 — Visibilidade e registro de autor nas transições

| | |
|---|---|
| Requisito | RF-001.4.3, RF-001.5, RN-10, decisão D6 |
| Depende de | TAREFA-03 |
| Toca banco | **Sim** — duas colunas novas |
| Tamanho | M |
| Risco | Médio — mexe em caminho que altera estoque |

## Objetivo

Fazer `finalizar` e `cancelar` respeitarem o dono do pedido, e registrarem quem
executou a transição e quando.

## O problema atual

`FinalizarPedidoUseCase` e `CancelarPedidoUseCase` carregam o pedido por
identificador e agem. Não há verificação de quem está pedindo. Hoje, qualquer
vendedor autenticado cancela o pedido de qualquer outro — devolvendo estoque,
mudando o resultado do mês de outra pessoa, sem deixar rastro de quem foi.

## Contexto para ler antes

- `Application/UseCases/Pedidos/Commands/FinalizarPedidoUseCase.cs` e
  `CancelarPedidoUseCase.cs` — **leia o trecho de idempotência com atenção**:

```csharp
if (pedido.Status == StatusPedido.Cancelado)
{
    return _mapper.Map<PedidoResponseDTO>(pedido);
}
```

  Chamar cancelar duas vezes devolve sucesso e **não** mexe no estoque de novo.
  Esse comportamento é testado em `TransicaoPedidoUseCaseTests.cs` e não pode
  regredir.

- `Domain/Entities/Pedidos/Pedido.cs`, métodos `Finalizar()` e `Cancelar()`.

## Passo a passo

### 1. Domínio

```csharp
public int? TransicionadoPorUsuarioId { get; private set; }
public DateTime? TransicionadoEm { get; private set; }
```

Altere as assinaturas para receber quem executou:

```csharp
public void Finalizar(int usuarioId)
public void Cancelar(int usuarioId)
```

e registre **junto com a mudança de status**, dentro do método, depois das
validações:

```csharp
Status = StatusPedido.Finalizado;
TransicionadoPorUsuarioId = usuarioId;
TransicionadoEm = DateTime.UtcNow;
```

> **RF-001.5.2, o ponto delicado:** os campos só são escritos quando a transição
> realmente acontece. Como o caso de uso retorna cedo no caminho idempotente, os
> métodos do domínio nem chegam a ser chamados na segunda vez — o registro
> original se preserva sozinho. Confirme que é assim, e escreva o teste que
> prova. Se você mover o registro para o caso de uso, **antes** da checagem de
> idempotência, quebra.

### 2. Mapeamento e migração

Duas colunas anuláveis, mais chave estrangeira de
`TransicionadoPorUsuarioId` para `Usuarios` com `Restrict`.

```bash
dotnet ef migrations add PedidoComRegistroDeTransicao --project GestaoPedidos/GestaoPedidos.csproj
```

Sem carga retroativa: transições passadas não têm autor conhecido, e inventar um
seria o mesmo erro que a decisão D5 evita.

### 3. Regra de visibilidade nos dois casos de uso

Mesma verificação da TAREFA-06. **Agora sim extraia**, porque este é o segundo e
terceiro uso:

```csharp
// Application/UseCases/Pedidos/AutorizacaoDePedido.cs
internal static void GarantirAcesso(Pedido pedido, Usuario usuario)
{
    if (usuario.Role != UserRole.Admin && pedido.UsuarioId != usuario.Id)
    {
        throw new NotFoundException(PedidosExceptions.Pedido_NaoEncontrado);
    }
}
```

Volte na TAREFA-06 e troque a verificação inline por esta chamada.

Chame logo depois de carregar o pedido, **antes** da checagem de idempotência.
Um vendedor não pode nem descobrir que o pedido de outro já está cancelado.

### 4. Admin transiciona pedido alheio

É permitido (decisão D6) e já sai de graça da verificação acima. O que a decisão
exige é que fique registrado — e fica, porque `TransicionadoPorUsuarioId` recebe
quem executou, não o dono do pedido.

### 5. Exponha na resposta

`TransicionadoPorUsuarioId` e `TransicionadoEm` em `PedidoResponseDTO`.

## Armadilhas

- **A ordem importa:** carregar → verificar acesso → verificar idempotência →
  agir. Inverter os dois do meio vaza informação.
- **Não** registre a transição no caminho idempotente. O teste
  `Cancelar_Duas_Vezes_Nao_Deve_Devolver_Estoque_Duas_Vezes` continua tendo que
  passar, e você vai adicionar um irmão dele para os novos campos.
- `DateTime.UtcNow` — o projeto usa UTC em todo lugar (`Cliente.DataCadastro`,
  `Produto.DataCadastro`, `Pedido.DataCadastro`). Não quebre a convenção.
- Os testes existentes chamam `pedido.Finalizar()` sem argumento e vão parar de
  compilar. Atualize-os: é sinal de que a mudança tem alcance real, não de que
  algo está errado.

## Testes a escrever

Estenda `GestaoPedidosTests/Application/UseCases/Pedidos/Commands/TransicaoPedidoUseCaseTests.cs`:

| Cenário | Esperado |
|---|---|
| Vendedor finaliza pedido próprio | 200; autor da transição é ele; instante preenchido |
| Vendedor tenta cancelar pedido de outro | `NotFoundException`; status inalterado; **estoque inalterado** |
| Admin cancela pedido de outro vendedor | Cancelado; autor da transição é o admin |
| Cancelar duas vezes, por pessoas diferentes | O registro continua sendo o da **primeira** transição |
| Cancelar duas vezes | Estoque devolvido uma vez só (teste existente, precisa continuar verde) |
| Finalizar pedido histórico sem autor, como vendedor | 404 |
| Finalizar pedido histórico sem autor, como admin | Funciona |

O quarto cenário é o mais sutil e o mais provável de quebrar em refatoração
futura. Nomeie-o de forma explícita, algo como
`Cancelar_Duas_Vezes_Deve_Preservar_O_Registro_Da_Primeira_Transicao`.

## Critérios de aceite

- [ ] Vendedor não finaliza nem cancela pedido de outro; recebe 404.
- [ ] Estoque não é alterado quando o acesso é negado.
- [ ] Admin transiciona qualquer pedido.
- [ ] Toda transição efetiva registra autor e instante.
- [ ] Chamada idempotente preserva o registro original.
- [ ] Verificação de acesso extraída e reaproveitada pela TAREFA-06.
- [ ] Testes existentes de idempotência continuam passando.
