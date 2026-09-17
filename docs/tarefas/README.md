# RF-001 — Tarefas de implementação

Quebra do documento [RF-001](../RF-001-pedido-com-dono-e-consulta.md) em tarefas
executáveis, na ordem de dependência. Cada arquivo é autocontido: dá para pegar
uma tarefa, ler só ela e trabalhar.

## Ordem de execução

| # | Tarefa | Depende de | Toca banco | Tamanho |
|---|---|---|:---:|---|
| 01 | [Envelope de paginação reutilizável](TAREFA-01-envelope-de-paginacao.md) | — | Não | P |
| 02 | [Coluna `UsuarioId` no pedido](TAREFA-02-coluna-usuario-no-pedido.md) | 01 | **Sim** | M |
| 03 | [Preencher o autor na criação](TAREFA-03-autor-na-criacao-do-pedido.md) | 02 | Não | M |
| 04 | [`ValorTotal` persistido](TAREFA-04-valor-total-persistido.md) | 02 | **Sim** | M |
| 05 | [`GET /api/Pedido` com filtros](TAREFA-05-listagem-de-pedidos.md) | 01, 03, 04 | Não | G |
| 06 | [Visibilidade no `GET /api/Pedido/{id}`](TAREFA-06-visibilidade-por-id.md) | 03 | Não | P |
| 07 | [Visibilidade e autor nas transições](TAREFA-07-transicoes-com-dono.md) | 03 | **Sim** | M |
| 08 | [`PUT /api/Pedido/{id}/itens`](TAREFA-08-alterar-itens-do-pedido.md) | 04, 06 | Não | G |
| 09 | [Documentação e fechamento](TAREFA-09-documentacao-e-fechamento.md) | todas | Não | P |

As tarefas 02, 04 e 07 geram migração. Elas **não** podem ser reordenadas entre
si sem regerar as migrações, porque o EF Core encadeia cada migração à anterior
pelo *snapshot* do modelo (`Migrations/AppDbContextModelSnapshot.cs`).

## O que você precisa saber sobre este projeto antes de começar

Fatos que valem para todas as tarefas e não estão óbvios no código:

- **Alvo é .NET 8**, apesar do `README.md` afirmar .NET 10. `global.json` fixa o
  SDK em `8.0.414` e os dois `.csproj` têm `net8.0`. Corrigir o README é a
  tarefa 09.
- **`TreatWarningsAsErrors` está ligado** em `Directory.Build.props`. Um aviso de
  nulabilidade quebra a compilação. Não é opcional tratar `null`.
- **Mensagens de erro vivem em arquivos `.resx`**, não em literais no código.
  Ver `Domain/Exceptions/Pedidos/PedidosExceptions.resx`. Mensagem nova entra
  lá e é acessada pela classe gerada `PedidosExceptions`.
- **Casos de uso são registrados automaticamente.** `Program.cs` usa Scrutor
  para registrar toda classe cujo nome termina em `UseCase`, com tempo de vida
  *scoped*. Você **não** precisa mexer no `Program.cs` para um caso de uso novo.
- **Validadores também.** `AddValidatorsFromAssemblyContaining` varre o
  assembly; basta criar a classe herdando `AbstractValidator<T>`.
- **Testes usam MSTest**, não xUnit: `[TestClass]`, `[TestMethod]`,
  `[TestInitialize]`. Com FluentAssertions (`.Should()`) e Moq.
- **Erros saem em `application/problem+json`** com `traceId`, montados por
  `ExceptionMiddleware`. Você lança a exceção de domínio correta
  (`NotFoundException`, `ConflictException`, `BadRequestException`) e o
  middleware traduz para o código HTTP.
- **Transação**: `IUnitOfWork.ExecutarEmTransacao` já cuida do escopo. Toda
  escrita composta passa por ele.

## Convenção de commit sugerida

Uma tarefa, um commit. Mensagem no imperativo, em português:

```
Adiciona coluna UsuarioId ao pedido

Refs RF-001.1
```

## Validação, ao fim de cada tarefa

```bash
dotnet format GestaoPedidos.sln --verify-no-changes --no-restore
dotnet build GestaoPedidos.sln --no-restore
dotnet test GestaoPedidos.sln --no-build --no-restore
```
