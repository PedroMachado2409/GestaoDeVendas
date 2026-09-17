# TAREFA-09 — Documentação e fechamento

| | |
|---|---|
| Requisito | RNF-05, RNF-06, RNF-08, Definição de Pronto do RF-001 |
| Depende de | todas as anteriores |
| Toca banco | Não |
| Tamanho | P |
| Risco | Baixo |

## Objetivo

Fechar a entrega: documentação coerente com o que foi construído, Swagger
completo e a suíte inteira verde.

Tarefa de fechamento não é burocracia. Documentação que mente custa mais caro que
documentação que não existe, porque a pessoa seguinte confia nela.

## 1. Corrija a versão da plataforma no `README.md`

O README afirma:

> API REST para gestão de clientes, produtos, usuários e pedidos, construída com
> .NET 10, ASP.NET Core, Entity Framework Core e PostgreSQL.

e nos pré-requisitos pede ".NET SDK 10.0.100 ou patch mais recente da linha
10.0.1xx".

Mas o repositório diz outra coisa:

- `global.json` → `"version": "8.0.414"`;
- `GestaoPedidos/GestaoPedidos.csproj` → `<TargetFramework>net8.0</TargetFramework>`;
- `GestaoPedidosTests/GestaoPedidosTests.csproj` → `net8.0`;
- pacotes EF Core e JwtBearer na linha `8.0.x`.

Alinhe o README ao que está no disco: **.NET 8**, SDK 8.0.414. Se a intenção era
migrar para o 10 e a migração foi revertida, isso é assunto de outro card — não
resolva aqui, só faça o texto parar de mentir.

## 2. Atualize a tabela de permissões do `README.md`

A tabela atual diz:

| Recurso | Vendedor | Admin |
|---|---:|---:|
| Criar/finalizar/cancelar pedidos | Sim | Sim |

Isso ficou incompleto. Substitua pelas linhas que refletem a decisão D1:

| Recurso | Vendedor | Admin |
|---|---:|---:|
| Criar pedidos | Sim | Sim |
| Consultar e listar pedidos | Somente os próprios | Todos |
| Alterar itens de pedido aberto | Somente os próprios | Todos |
| Finalizar/cancelar pedidos | Somente os próprios | Todos |

Acrescente uma frase explicando que pedido de outro vendedor responde 404, e o
porquê — é o tipo de decisão que parece bug para quem chega depois.

## 3. Acrescente as garantias novas à lista do README

A seção "Principais garantias" ganha:

- Todo pedido registra o vendedor que o criou, definido pelo servidor.
- Vendedor acessa apenas os próprios pedidos.
- Transição de pedido registra quem executou e quando.
- Listagens paginadas com envelope padronizado.

## 4. Complete o Swagger

Nos endpoints novos e alterados de `PedidoController`, anote os códigos de
resposta:

```csharp
[ProducesResponseType(typeof(ResultadoPaginado<PedidoResumoDTO>), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
```

Sem isso o Swagger anuncia só o 200 e quem consome a API descobre os outros
códigos por tentativa e erro.

Suba a aplicação em desenvolvimento e confira na interface do Swagger que
aparecem: `GET /api/Pedido` com todos os parâmetros de consulta e
`PUT /api/Pedido/{id}/itens` com o corpo esperado.

## 5. Confirme que nenhum contrato quebrou (RNF-06)

Percorra os DTOs de resposta e verifique que, em relação ao estado anterior à
entrega:

- nenhum campo foi **removido**;
- nenhum campo foi **renomeado**;
- nenhum campo mudou de tipo;
- os campos novos são todos adicionais.

Um consumidor que existia antes precisa continuar funcionando sem alteração.

## 6. Rode a validação completa

```bash
dotnet format GestaoPedidos.sln --verify-no-changes --no-restore
dotnet build GestaoPedidos.sln --no-restore
dotnet test GestaoPedidos.sln --no-build --no-restore
dotnet list GestaoPedidos/GestaoPedidos.csproj package --vulnerable --include-transitive
```

Lembre que `TreatWarningsAsErrors` está ligado: se o build passou, não há aviso
pendente.

## 7. Verifique o ciclo completo de migração

Em uma base com dados representativos:

```bash
dotnet ef database update --project GestaoPedidos/GestaoPedidos.csproj
```

Reverta até antes da primeira migração desta entrega e avance de novo. As três
migrações (TAREFA-02, 04 e 07) precisam ir e voltar sem erro.

## 8. Marque a Definição de Pronto

Volte em [RF-001](../RF-001-pedido-com-dono-e-consulta.md), seção 15, e marque
os itens. Se algum não puder ser marcado, ele não está pronto — e a entrega não
está fechada.

## Critérios de aceite

- [ ] README não afirma mais .NET 10.
- [ ] Tabela de permissões reflete a visibilidade por vendedor.
- [ ] Garantias novas listadas no README.
- [ ] Swagger mostra os endpoints novos com códigos de resposta documentados.
- [ ] Nenhum campo de resposta removido ou renomeado.
- [ ] `format`, `build`, `test` e verificação de vulnerabilidade sem falha.
- [ ] Ciclo de migração validado nos dois sentidos.
- [ ] Definição de Pronto do RF-001 integralmente marcada.

## Depois desta entrega

O próximo item do backlog é o **endpoint de relatórios**, que só se tornou
viável agora: ele depende de `ValorTotal` persistido (TAREFA-04) e de
`UsuarioId` no pedido (TAREFA-02). Ver o Anexo A do RF-001.
