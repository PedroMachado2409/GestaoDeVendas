# TAREFA-04 — `ValorTotal` persistido

| | |
|---|---|
| Requisito | RF-001.2, decisão D4 |
| Depende de | TAREFA-02 |
| Toca banco | **Sim** — migração com carga retroativa |
| Tamanho | M |
| Risco | **Alto** — mexe em valor financeiro de registro histórico |

## Objetivo

Transformar `ValorTotal` de propriedade calculada em memória para coluna
persistida no banco.

## Por que isso importa

Hoje, em `AppDbContext.cs`:

```csharp
entity.Ignore(p => p.ValorTotal);
```

e na entidade:

```csharp
public decimal ValorTotal => _itens.Sum(i => i.SubTotal);
```

Funciona para exibir **um** pedido. Mas significa que:

- listar 20 pedidos exige carregar todos os itens dos 20;
- não existe `WHERE ValorTotal > 500`;
- não existe `SUM(ValorTotal)` para faturamento;
- qualquer relatório teria que ser calculado fora do banco.

É o item que destrava o épico de relatórios inteiro.

## Contexto para ler antes

- `Domain/Entities/Pedidos/Pedido.cs` e `PedidoItem.cs` — `SubTotal` é
  `Preco * Quantidade`.
- `Migrations/20260730011548_HardeningSegurancaIntegridade.cs` — o projeto já
  tem o hábito de **verificar os dados antes de criar a restrição**, e
  interromper com mensagem clara se algo estiver inconsistente. Siga esse
  padrão.
- `AppDbContext.cs`, bloco `Produto` — o padrão de nomenclatura das restrições
  de verificação: `CK_Tabela_Campo_Regra`.

## Passo a passo

### 1. Domínio

Troque a propriedade calculada por uma propriedade com *setter* privado e um
método de recálculo:

```csharp
public decimal ValorTotal { get; private set; }

private void RecalcularValorTotal()
    => ValorTotal = _itens.Sum(i => i.SubTotal);
```

Chame `RecalcularValorTotal()`:

- no fim do construtor que recebe os itens;
- no fim de `AtualizarItem`, **inclusive no caminho de remoção**.

> Cuidado: em `AtualizarItem` existe um `return` antecipado no caminho que
> remove o item. Se você chamar o recálculo só no fim do método, esse caminho
> passa batido e o valor fica velho. Esse é o defeito mais provável desta
> tarefa.

### 2. Mapeamento

Remova o `entity.Ignore(p => p.ValorTotal)` e configure:

```csharp
entity.Property(p => p.ValorTotal).HasPrecision(18, 2);
entity.ToTable("Pedidos", table =>
{
    table.HasCheckConstraint("CK_Pedidos_ValorTotal_Positivo", "\"ValorTotal\" > 0");
});
```

`HasPrecision(18, 2)` é o mesmo usado em `Produto.Preco` e `PedidoItem.Preco`.
Manter a mesma precisão evita arredondamento divergente entre a soma dos itens e
a coluna.

### 3. Migração, em cinco passos

```bash
dotnet ef migrations add PedidoComValorTotal --project GestaoPedidos/GestaoPedidos.csproj
```

O arquivo gerado vai criar a coluna já obrigatória, com valor padrão zero. **Isso
não serve.** Edite o `Up` para a sequência:

1. Criar a coluna **anulável**.
2. Preencher os pedidos existentes:

```sql
UPDATE "Pedidos" p
SET "ValorTotal" = s.total
FROM (
    SELECT "PedidoId", SUM("Preco" * "Quantidade") AS total
    FROM "PedidoItens"
    GROUP BY "PedidoId"
) s
WHERE p."Id" = s."PedidoId";
```

3. Verificar antes de travar — interrompa se sobrou algo inconsistente:

```sql
DO $$
DECLARE invalidos integer;
BEGIN
    SELECT COUNT(*) INTO invalidos
    FROM "Pedidos"
    WHERE "ValorTotal" IS NULL OR "ValorTotal" <= 0;

    IF invalidos > 0 THEN
        RAISE EXCEPTION
            'Migração interrompida: % pedido(s) com valor total nulo ou não positivo. Corrija os itens desses pedidos e execute novamente.',
            invalidos;
    END IF;
END $$;
```

4. Tornar a coluna obrigatória.
5. Criar a restrição de verificação.

O `Down` remove restrição e coluna.

### 4. Aplique, reverta, aplique de novo

Como na TAREFA-02. Aqui a reversão importa mais: você está mexendo em dado
financeiro.

## Armadilhas

- **Pedido sem item nenhum no banco** faz o `UPDATE` não atingir a linha, e ela
  fica `NULL`. A verificação do passo 3 pega isso e interrompe — que é o
  comportamento desejado. Não "resolva" preenchendo com zero: zero passaria na
  restrição de positivo? Não, `> 0` recusa. Mas mesmo que passasse, seria
  inventar dado.
- **Não** use `HasComputedColumnSql`. Coluna calculada no banco não é atualizável
  pelo EF e complica a TAREFA-08.
- **Não** confie no AutoMapper para manter o valor. O mapeamento já traz
  `ValorTotal` para o DTO; o que muda é a origem do número.
- Verifique se algum teste existente constrói `Pedido` e afirma sobre
  `ValorTotal`. O comportamento observável não deve mudar — se mudar, é bug.

## Testes a escrever

Arquivo: `GestaoPedidosTests/Domain/Entities/PedidoTests.cs`

| Cenário | Esperado |
|---|---|
| Pedido com itens 10,00×2 e 5,50×4 | `ValorTotal` é 42,00 |
| Após aumentar a quantidade de um item | `ValorTotal` recalculado |
| Após reduzir a quantidade de um item | `ValorTotal` recalculado |
| Após **remover** um item (quantidade 0) | `ValorTotal` recalculado — cobre o `return` antecipado |
| Tentativa de remover o último item | Lança e `ValorTotal` permanece o anterior |

O quarto cenário é o mais importante da tarefa. Escreva-o primeiro.

## Critérios de aceite

- [ ] `ValorTotal` é coluna persistida, `numeric(18,2)`, não nula.
- [ ] Restrição `CK_Pedidos_ValorTotal_Positivo` existe.
- [ ] Migração preenche os pedidos existentes a partir dos itens.
- [ ] Migração **interrompe com mensagem clara** se algum pedido ficar sem valor
      válido, em vez de gravar zero.
- [ ] Migração reverte sem erro.
- [ ] Recálculo cobre todos os caminhos de alteração de item, remoção inclusive.
- [ ] Nenhum contrato de resposta mudou.

## Como validar

Depois de aplicar, compare a coluna com a soma dos itens. O resultado tem que
ser vazio:

```sql
SELECT p."Id", p."ValorTotal", SUM(i."Preco" * i."Quantidade") AS soma_itens
FROM "Pedidos" p
JOIN "PedidoItens" i ON i."PedidoId" = p."Id"
GROUP BY p."Id", p."ValorTotal"
HAVING p."ValorTotal" <> SUM(i."Preco" * i."Quantidade");
```

Guarde essa consulta. Ela é a verificação de sanidade da TAREFA-08 também.
