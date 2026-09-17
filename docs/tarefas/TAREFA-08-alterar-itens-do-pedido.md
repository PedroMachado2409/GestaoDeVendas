# TAREFA-08 — `PUT /api/Pedido/{id}/itens`

| | |
|---|---|
| Requisito | RF-001.6, RN-05 a RN-09, RN-11, decisão D7 |
| Depende de | TAREFA-04, TAREFA-06 |
| Toca banco | Não |
| Tamanho | G |
| Risco | **Alto** — é a operação que mais pode corromper estoque |

## Objetivo

Permitir alterar os itens de um pedido ainda aberto, ajustando a reserva de
estoque de forma consistente.

## Por que esta tarefa existe

`Pedido.AtualizarItem()` já está implementado no domínio, com validação de
situação e de quantidade — e **nenhum endpoint o chama**. É regra escrita,
paga e não entregue.

## Contexto para ler antes

Leia os três na ordem, é a base inteira da tarefa:

1. `Domain/Entities/Produto.cs` — `ReservarQuantidade`, `LiberarReserva` e
   `ConfirmarReserva`. Entenda que **reservar move do estoque para a reserva**
   (`Estoque -= q; QuantidadeReservada += q`), liberar faz o inverso, e
   confirmar apenas baixa a reserva sem devolver ao estoque.
2. `Application/UseCases/Pedidos/Commands/CadastrarPedidoUseCase.cs` — o padrão
   de carregar produtos por lote com `ObterPorIds` e montar um dicionário.
3. `Domain/Entities/Pedidos/Pedido.cs`, `AtualizarItem` — o método que você vai
   usar, com seu `return` antecipado no caminho de remoção.

## Contrato

Recebe a lista **completa e desejada** de itens, não um delta:

```json
{ "itens": [ { "produtoId": 3, "quantidade": 2 } ] }
```

Lista completa é mais simples de raciocinar e é idempotente: mandar duas vezes o
mesmo corpo produz o mesmo estado. Um delta ("some 2 unidades") aplicado duas
vezes por engano corrompe o pedido.

## Passo a passo

### 1. DTO e validador

`AtualizarItensPedidoRequestDTO` com a lista. Validador com as mesmas regras de
`CriarPedidoRequestValidator`: lista não vazia, `produtoId` e `quantidade`
maiores que zero, e **sem produto repetido** — esta última já existe lá, copie a
regra.

### 2. Caso de uso, dentro de transação

Esqueleto do raciocínio:

```
carregar pedido (404 se não existir)
garantir acesso (AutorizacaoDePedido.GarantirAcesso — TAREFA-07)
se status != Aberto -> BadRequest
carregar produtos envolvidos: união dos produtos atuais do pedido com os pedidos no corpo
calcular, por produto, a diferença entre quantidade desejada e quantidade atual
aplicar as liberações ANTES das reservas
aplicar as alterações no pedido
recalcular ValorTotal
salvar
```

### 3. Libere antes de reservar

O ponto mais importante da tarefa.

Imagine um pedido com 5 unidades do produto A, estoque livre 0, e o usuário quer
trocar para 3 unidades do produto A e 2 do produto B. Se você processar na ordem
do corpo da requisição e o produto B precisar de estoque que ainda está preso na
reserva de A, a operação falha por "estoque insuficiente" — mesmo sendo
perfeitamente possível.

Processar todas as liberações primeiro elimina a classe inteira de falso
negativo. Faça isso explicitamente, em duas passadas, e comente o motivo.

### 4. Preço: nunca reprecificar item existente

Decisão D7 e RN-08. Para cada item:

- **já existe no pedido** → mantém o `Preco` gravado, mesmo que o produto tenha
  mudado de preço desde então;
- **é novo** → entra com o preço vigente do produto.

O pedido é um registro histórico do que foi acordado. Reprecificar em silêncio
mudaria retroativamente o valor de uma venda combinada.

### 5. Produto inativo

RN-11: produto inativo não pode ser adicionado, nem ter a quantidade aumentada.
Mas **pode** ter a quantidade reduzida ou ser removido — senão um pedido com
produto que acabou de ser inativado ficaria impossível de corrigir.

### 6. Não deixe o pedido vazio

RN-06. Lista vazia é barrada pelo validador (400). O caminho de remoção do
último item dentro de `AtualizarItem` já lança — confirme que a mensagem chega
como 400 e não como 500.

### 7. Recalcule o valor total

Se você fez a TAREFA-04 direito, `AtualizarItem` já recalcula sozinho. Confirme,
e escreva o teste mesmo assim.

### 8. Controller

```csharp
[HttpPut("{id:int}/itens")]
public async Task<IActionResult> AtualizarItens(int id, [FromBody] AtualizarItensPedidoRequestDTO dto)
    => Ok(await _atualizarItensPedido.Executar(id, dto));
```

## Armadilhas

- **`ProdutoRepository.Atualizar` chama `SaveChangesAsync` internamente.** Isso é
  uma anomalia do projeto: repositório não deveria salvar. Dentro de
  `ExecutarEmTransacao` a atomicidade se mantém, porque a transação explícita não
  é confirmada por um `SaveChanges`. Mas fique atento e prefira deixar o
  `IUnitOfWork.SalvarAlteracoes()` no fim, como fazem os outros casos de uso.
- **`Produto.Versao` é token de concorrência.** Duas alterações simultâneas no
  mesmo produto: a segunda levanta `DbUpdateConcurrencyException`, que o
  middleware traduz para 409. Não capture para "tentar de novo" — o 409 é a
  resposta correta.
- **Estoque insuficiente vira 409, não 400.** `Produto.ReservarQuantidade` lança
  `ConflictException`. Deixe subir.
- **Nada pode ser aplicado pela metade.** Se a terceira reserva falhar, as duas
  primeiras não podem valer. A transação garante isso no banco — mas garanta
  também que você não fez escrita fora dela.
- Cuidado ao calcular a diferença quando o item **sai** do pedido: a quantidade
  desejada é zero, e a liberação é da quantidade integral.

## Testes a escrever

Este é o conjunto mais extenso da entrega. Não economize.

**Ajuste de reserva**

| Situação inicial | Ação | Esperado |
|---|---|---|
| 2 de A, estoque livre 10 | Alterar para 5 | Estoque 7, reserva 5 |
| 5 de A, estoque livre 5 | Alterar para 2 | Estoque 8, reserva 2 |
| 3 de A | Remover A, pedido tem outro item | Estoque devolvido integralmente |
| 2 de A | Adicionar B com 1 | Reserva de B criada, A intocado |
| 5 de A, livre 0; trocar por 3 de A e 2 de B | — | Sucesso, graças à liberação antes da reserva |

**Recusas**

| Situação | Esperado |
|---|---|
| Aumentar além do estoque | 409, nada alterado |
| Pedido finalizado | 400 |
| Pedido cancelado | 400 |
| Lista vazia | 400 |
| Remover o único item | 400 |
| Produto inexistente | 404 |
| Produto inativo, aumentando quantidade | 409 |
| Produto inativo, reduzindo quantidade | Sucesso |
| Pedido de outro vendedor | 404 |

**Consistência**

| Cenário | Esperado |
|---|---|
| Alteração bem-sucedida | `ValorTotal` igual à soma dos itens |
| Falha no meio | Estoque e pedido idênticos ao estado inicial |
| Item preexistente com preço antigo, produto reprecificado | Item mantém o preço antigo |

## Critérios de aceite

- [ ] Endpoint existe e só aceita pedido `Aberto`.
- [ ] Reserva ajustada corretamente nos quatro casos: aumento, redução, remoção,
      inclusão.
- [ ] Liberações processadas antes das reservas.
- [ ] Item preexistente nunca é reprecificado.
- [ ] Pedido nunca fica sem itens.
- [ ] `ValorTotal` recalculado e persistido.
- [ ] Falha não deixa estado parcial.
- [ ] Visibilidade por vendedor respeitada.
- [ ] Todos os cenários das três tabelas cobertos.

## Como validar

Depois de exercitar o endpoint, rode a consulta de sanidade da TAREFA-04:

```sql
SELECT p."Id", p."ValorTotal", SUM(i."Preco" * i."Quantidade") AS soma_itens
FROM "Pedidos" p
JOIN "PedidoItens" i ON i."PedidoId" = p."Id"
GROUP BY p."Id", p."ValorTotal"
HAVING p."ValorTotal" <> SUM(i."Preco" * i."Quantidade");
```

E confira que estoque mais reserva permanece coerente:

```sql
SELECT "Id", "Nome", "Estoque", "QuantidadeReservada"
FROM "Produtos"
WHERE "Estoque" < 0 OR "QuantidadeReservada" < 0;
```

As duas precisam voltar vazias. As restrições de verificação do banco já
impedem valores negativos, mas rodar a consulta confirma que você não passou
perto.
