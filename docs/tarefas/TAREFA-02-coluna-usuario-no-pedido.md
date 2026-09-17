# TAREFA-02 — Coluna `UsuarioId` no pedido

| | |
|---|---|
| Requisito | RF-001.1, RN-01, RN-02, decisão D5 |
| Depende de | TAREFA-01 |
| Toca banco | **Sim** — gera migração |
| Tamanho | M |
| Risco | Médio — migração em tabela existente |

## Objetivo

Dar ao pedido um campo que registre qual usuário o criou, no domínio e no banco.
Nesta tarefa o campo ainda **não é preenchido** por ninguém — isso é a
TAREFA-03. Aqui só existe a estrutura.

Separar assim tem um motivo: a migração é a parte arriscada e reversível
isoladamente. Se ela der problema em uma base com dados, você descobre antes de
ter mexido em caso de uso nenhum.

## Contexto para ler antes

- `Domain/Entities/Pedidos/Pedido.cs` — repare que todo *setter* é `private` e
  que existe um construtor `protected Pedido() { }` só para o EF Core.
- `Infrastructure/Data/AppDbContext.cs`, bloco `modelBuilder.Entity<Pedido>` —
  veja como `ClienteId` é configurado com `HasOne<Cliente>().WithMany()`.
- `Migrations/20260730011548_HardeningSegurancaIntegridade.cs` — o padrão
  defensivo de migração adotado no projeto.

## Passo a passo

### 1. Domínio — `Domain/Entities/Pedidos/Pedido.cs`

Adicione a propriedade:

```csharp
public int? UsuarioId { get; private set; }
```

E receba o valor no construtor, como parâmetro opcional **no fim da lista**,
para não quebrar as chamadas existentes ainda:

```csharp
public Pedido(int clienteId, List<PedidoItem> itens, int? usuarioId = null)
```

> **Por que anulável?** Decisão D5 do RF-001: pedidos que já existem no banco não
> têm autor, e inventar um seria registrar uma venda que a pessoa não fez. O
> campo é anulável no tipo, e a obrigatoriedade para pedidos novos é garantida
> na TAREFA-03, no caso de uso — não pelo banco.

Não crie método para alterar o autor. RN-02: o autor de um pedido nunca muda.

### 2. Mapeamento — `AppDbContext.cs`

Dentro de `modelBuilder.Entity<Pedido>`:

```csharp
entity.HasOne<Usuario>()
    .WithMany()
    .HasForeignKey(p => p.UsuarioId)
    .OnDelete(DeleteBehavior.Restrict);

entity.HasIndex(p => new { p.UsuarioId, p.DataCadastro })
    .IsDescending(false, true);
```

`Restrict` segue o que já é feito com `ClienteId`: usuário com pedido não é
apagado.

O índice composto `(UsuarioId, DataCadastro DESC)` atende o caminho mais
frequente da TAREFA-05 — vendedor listando os próprios pedidos, do mais recente
para o mais antigo.

### 3. Migração

```bash
dotnet ef migrations add PedidoComUsuario --project GestaoPedidos/GestaoPedidos.csproj
```

Abra o arquivo gerado e **confira** antes de aplicar:

- a coluna nasce `nullable: true`;
- nenhuma outra alteração inesperada apareceu junto (se aparecer, alguém deixou
  o modelo dessincronizado do snapshot — resolva isso antes);
- o `Down` remove índice, chave estrangeira e coluna, nessa ordem.

### 4. Aplique e reverta, para provar que a migração é reversível

```bash
dotnet ef database update --project GestaoPedidos/GestaoPedidos.csproj
dotnet ef database update Pedido --project GestaoPedidos/GestaoPedidos.csproj
dotnet ef database update --project GestaoPedidos/GestaoPedidos.csproj
```

A segunda linha volta para a migração anterior. Se o `Down` estiver errado, você
descobre agora, e não em produção.

## Armadilhas

- **Parâmetro opcional no construtor é temporário.** Ele existe para esta tarefa
  compilar sem tocar no caso de uso. Na TAREFA-03 ele deixa de ser opcional para
  o caminho de criação. Deixe um comentário curto explicando, ou o próximo leitor
  vai achar que criar pedido sem autor é aceitável.
- **Não** marque a coluna como obrigatória "porque a tabela está vazia hoje". A
  decisão D5 vale para qualquer ambiente que já tenha rodado a aplicação.
- Se o EF gerar a migração com nome de coluna diferente do esperado, não
  renomeie na mão no SQL — ajuste o mapeamento e regenere.

## Testes a escrever

Arquivo: `GestaoPedidosTests/Domain/Entities/PedidoTests.cs`

| Cenário | Esperado |
|---|---|
| Criar pedido informando `usuarioId` 7 | `pedido.UsuarioId` é 7 |
| Criar pedido sem informar `usuarioId` | `pedido.UsuarioId` é `null` |
| Procurar por método público que altere `UsuarioId` | Não existe |

O terceiro item não é teste automatizado, é conferência na revisão.

## Critérios de aceite

- [ ] `Pedido.UsuarioId` existe, é `int?` e tem *setter* privado.
- [ ] Não há método público que altere o autor.
- [ ] Chave estrangeira para `Usuarios` com `Restrict`.
- [ ] Índice `(UsuarioId, DataCadastro DESC)` criado.
- [ ] Migração aplica e reverte sem erro em base com dados.
- [ ] Nenhum comportamento existente mudou — a suíte de testes continua verde
      sem alteração nos testes antigos.
