# TAREFA-10 — Ajuste manual de estoque

| | |
|---|---|
| Tipo | Funcionalidade nova |
| Depende de | Trilha de movimentação (já implementada) |
| Toca banco | **Sim** — valor novo em enum, possível migração |
| Tamanho | M |
| Risco | Médio — altera estoque fora do fluxo de pedido |
| Objetivo pedagógico | Entidade rica, transação, concorrência, SOLID na prática |

## Objetivo

Permitir que um administrador corrija o estoque de um produto informando um
**motivo**, e que essa correção apareça na trilha de auditoria como qualquer
outra movimentação.

## O problema hoje

Se a contagem física não bate com o sistema — peça quebrada, unidade encontrada
no fundo do depósito, mercadoria extraviada — não existe forma de corrigir.

O único caminho é `PUT /api/Produto/{id}`, que chama `Produto.Atualizar` e
sobrescreve nome, marca, preço e estoque de uma vez. Isso tem três problemas:

1. Não registra **por que** o estoque mudou.
2. Sobrescreve o valor absoluto — dois administradores ajustando ao mesmo tempo
   e uma contagem se perde.
3. Não gera movimentação, então a trilha de auditoria fica com um buraco: o
   estoque muda e nada explica.

A trilha que você construiu responde "por que o estoque desse produto está em
3?" — mas só para movimento vindo de pedido. Esta tarefa fecha o buraco.

## Contexto para ler antes

- `Domain/Entities/Produto.cs` — em especial o bloco de comentário no topo, com
  as duas trilhas, e o padrão `Validar... → alterar → AtualizarVersao()`.
- `Domain/Entities/MovimentacaoEstoque.cs` — o construtor exige tudo de uma vez,
  e valida a quantidade.
- `Domain/Enum/OrigemMovimentacao.cs` e `TipoMovimentacao.cs`.
- `Application/UseCases/Pedidos/Commands/FinalizarPedidoUseCase.cs` — o exemplo
  mais próximo do que você vai escrever: move estoque e grava a movimentação, na
  mesma transação.

## Contrato sugerido

```
POST /api/Produto/{id:int}/ajuste-estoque
Authorization: Bearer <token de Admin>

{
  "quantidade": -3,
  "motivo": "Contagem física de 31/08 — 3 unidades avariadas"
}
```

Resposta 200 com o produto atualizado, ou o padrão que você preferir.

---

## Decisões que você precisa tomar

Estas ficam com você de propósito. São elas que fazem o exercício valer.
**Anote a sua escolha e o porquê** — de preferência num comentário no código.

### D1 — Um endpoint com sinal, ou dois endpoints?

`quantidade: -3` num endpoint só, ou `/entrada` e `/saida` separados, cada um
recebendo quantidade positiva?

Os dois funcionam. Pergunta que ajuda a decidir: em qual desenho é **mais
difícil errar**? Pense em quem chama a API com pressa.

### D2 — Ajuste negativo maior que o estoque livre

Produto com `Estoque = 10` e `QuantidadeReservada = 8` (pedidos abertos que já
contam com aquilo). Chega um ajuste de −5.

Pode? Deve recusar? Recusar com qual status? Se aceitar, o que acontece com os
pedidos que já contavam com o estoque?

> Dica: olhe o que `ReservarQuantidadeVenda` faz quando não há estoque
> suficiente, e por que a restrição `CK_Produtos_Estoque_NaoNegativo` existe no
> banco.

### D3 — O motivo é texto livre ou lista fechada?

Você acabou de trocar `NomeOrigem` de texto para enum, e viu o ganho. Vale a
mesma lógica aqui?

Argumento dos dois lados: lista fechada agrupa bem em relatório; texto livre
descreve o caso específico, que é justamente do que um ajuste precisa. Existe
uma terceira opção que combina as duas.

### D4 — Produto inativo aceita ajuste?

`CadastrarPedidoUseCase` recusa produto inativo. Ajuste é a mesma situação, ou
é justamente quando você mais precisa corrigir o saldo?

---

## Passo a passo sugerido

A ordem importa: **de dentro para fora**. Comece pela regra, não pelo endpoint.

### 1. O método na entidade

Em `Produto.cs`, algo como:

```csharp
public void AjustarEstoque(int quantidade)
{
    // sua validação aqui
    // sua alteração aqui
    AtualizarVersao();
}
```

Siga o padrão dos vizinhos: validar primeiro, alterar depois, atualizar a versão
no fim. Se precisar de mensagem nova, ela vai no `ProdutoExceptions.resx`, não
como literal.

### 2. O teste dessa regra

Antes de qualquer coisa de HTTP. É teste de entidade pura, igual aos que já
existem em `GestaoPedidosTests/Domain/Entities/ProdutoTests.cs` — sem mock, sem
banco.

### 3. O valor novo no enum

```csharp
public enum OrigemMovimentacao
{
    PedidoDeVenda = 0,
    PedidoDeCompra = 1,
    AjusteManual = 2      // ← novo
}
```

Acrescentar no fim, com número explícito. Nunca renumere valor existente: os
registros no banco guardam o número, não o nome.

### 4. DTO e validador

Mesmo padrão de `CriarPedidoRequestValidator`. Pense no que precisa validar:
quantidade zero faz sentido? motivo vazio faz sentido? tem tamanho máximo?

### 5. O caso de uso

`AjustarEstoqueUseCase`, dentro de `_unitOfWork.ExecutarEmTransacao`. Ele
precisa: buscar o produto, chamar o método da entidade, montar a
`MovimentacaoEstoque` e mandar salvar — uma vez só, no fim.

### 6. O endpoint

Em `ProdutoController`, com `[Authorize(Roles = "Admin")]`, como as outras
operações de escrita de produto.

### 7. A migração

O valor novo do enum é um `int` — pode não gerar migração alguma. Rode
`dotnet ef migrations add` e **leia o arquivo**. Se vier vazio, delete com
`dotnet ef migrations remove`.

---

## Armadilhas

**Não mexa em `Estoque` de fora da entidade.** A tentação vai ser
`produto.Estoque += quantidade` no caso de uso. O setter é privado justamente
para impedir isso — se você contornar, perde a validação e a versão não é
atualizada.

**Não esqueça o `AtualizarVersao()`.** São nove métodos que o chamam hoje. O seu
será o décimo. Um método que esquece abre um buraco silencioso na proteção de
concorrência, que só aparece com dois usuários simultâneos.

**Não toque em `QuantidadeReservada`.** Ajuste mexe no estoque livre. Reserva
pertence aos pedidos, e mexer nela por fora corrompe pedidos abertos.

**`MovimentacaoEstoque.Quantidade` não aceita negativo.** O construtor lança se
`quantidade <= 0`, e existe `CK_MovimentacoesEstoque_Quantidade_Positiva` no
banco. Então um ajuste de −3 **não** vira uma movimentação com quantidade −3.
Descubra como o modelo já resolve isso — a resposta está no
`TipoMovimentacao`.

**O que vai em `IdOrigem`?** Nas movimentações de pedido é o id do pedido. Num
ajuste manual não existe pedido. Pense no que faz esse campo continuar útil para
auditoria — quem fez o ajuste é uma informação que hoje se perderia.

**Produto e movimentação gravam juntos.** Se a movimentação falhar, o estoque
não pode ter mudado. É o que `ExecutarEmTransacao` garante — desde que você não
salve no meio do caminho.

---

## Critérios de aceite

### Valem independentemente das suas decisões

```gherkin
Dado que estou autenticado como administrador
E que o produto 7 tem estoque 10
Quando eu ajustar o estoque em +5 com um motivo válido
Então o estoque do produto 7 passa a 15
E uma movimentação é criada para o produto 7
E essa movimentação tem origem AjusteManual
E a Versao do produto mudou
```

```gherkin
Dado que estou autenticado como vendedor
Quando eu tentar ajustar o estoque de qualquer produto
Então recebo 403
E o estoque não muda
```

```gherkin
Dado que estou autenticado como administrador
Quando eu enviar um ajuste com quantidade zero
Então recebo 400 em application/problem+json
E o estoque não muda
E nenhuma movimentação é criada
```

```gherkin
Dado que estou autenticado como administrador
Quando eu ajustar o estoque de um produto inexistente
Então recebo 404
```

```gherkin
Dado um produto com estoque 10 e 4 unidades reservadas
Quando eu ajustar o estoque em +3
Então o estoque passa a 13
E QuantidadeReservada continua 4
```

```gherkin
Dado que o ajuste foi aplicado com sucesso
Quando eu consultar GET /api/MovimentacaoEstoque/produto/7
Então o ajuste aparece na lista
E o motivo informado está na observação
E ProdutoNome guarda o nome do produto no momento do ajuste
```

```gherkin
Dado que a gravação da movimentação falha
Quando o ajuste for executado
Então o estoque do produto permanece o valor anterior
E nenhuma movimentação é gravada
```

### Você escreve estes, conforme decidir

- **D2** — o que acontece num ajuste negativo maior que o estoque livre. Escreva
  o cenário com o comportamento que você escolheu, e o status que decidiu.
- **D4** — ajuste em produto inativo: aceita ou recusa. Escreva o cenário nos
  dois sentidos.
- **D1** — se optar por dois endpoints, duplique os cenários acima para cada um.

---

## Definição de pronto

- [ ] `Produto.AjustarEstoque` existe, valida, e chama `AtualizarVersao()`
- [ ] Nenhum código fora da entidade altera `Estoque` diretamente
- [ ] Mensagens novas em `ProdutoExceptions.resx`, não como literal
- [ ] `OrigemMovimentacao.AjusteManual` acrescentado no fim, sem renumerar
- [ ] Ajuste e movimentação gravam na mesma transação
- [ ] Endpoint restrito a `Admin`
- [ ] Todos os critérios da primeira lista com teste automatizado
- [ ] Cenários das suas decisões (D1–D4) escritos e cobertos
- [ ] Migração lida antes de aplicar — ou removida, se vier vazia
- [ ] `dotnet format --verify-no-changes`, `build` e `test` sem falha
- [ ] Suas decisões D1 a D4 registradas em comentário ou no commit

---

## Como validar à mão

```bash
# ajuste de +5
curl -X POST https://localhost:7086/api/Produto/7/ajuste-estoque \
  -H "Authorization: Bearer SEU_TOKEN_ADMIN" \
  -H "Content-Type: application/json" \
  -d '{"quantidade":5,"motivo":"Contagem fisica de 31/08"}'

# conferir a trilha
curl https://localhost:7086/api/MovimentacaoEstoque/produto/7 \
  -H "Authorization: Bearer SEU_TOKEN_ADMIN"
```

E no banco, o estoque nunca pode ficar inconsistente:

```sql
SELECT "Id", "Nome", "Estoque", "QuantidadeReservada", "QuantidadeCompradaPendente"
FROM "Produtos"
WHERE "Estoque" < 0 OR "QuantidadeReservada" < 0 OR "QuantidadeCompradaPendente" < 0;
```

Precisa voltar vazia.

---

## Onde cada princípio aparece

Depois de terminar, volte aqui e confira se você percebeu cada um:

| Princípio | Onde aparece nesta tarefa |
|---|---|
| **S** · Responsabilidade única | O caso de uso ajusta estoque. Não lista, não valida CPF, não faz mais nada |
| **O** · Aberto/fechado | Valor novo no enum, sem editar nada que já existe |
| **I** · Segregação | Você não precisa de método novo em `IMovimentacaoEstoqueRepository` — `Cadastrar` já serve |
| **D** · Inversão | O caso de uso depende de `IProdutoRepository` e `IMovimentacaoEstoqueRepository`, não das classes |
| **KISS** | Resista a criar `IAjustavel`, `IEstoqueService` ou uma hierarquia de tipos de ajuste. Um método na entidade e um caso de uso bastam |

O último é o mais difícil de cumprir, e o que mais separa nível.
