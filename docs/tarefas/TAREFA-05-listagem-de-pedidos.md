# TAREFA-05 — `GET /api/Pedido` com filtros e paginação

| | |
|---|---|
| Requisito | RF-001.3, RF-001.4.1, RNF-01, RNF-02, RNF-03 |
| Depende de | TAREFA-01, TAREFA-03, TAREFA-04 |
| Toca banco | Não (usa os índices criados antes) |
| Tamanho | G |
| Risco | Médio — é o endpoint mais consultado da API |

## Objetivo

Entregar a listagem de pedidos: o buraco que fez o consumidor atual guardar
identificadores no navegador para simular uma lista.

## Contexto para ler antes

- `Domain/Abstractions/IPedidoRepository.cs` — a assinatura comentada
  `//Task <List<Pedido>> Listar();` é literalmente esta tarefa.
- `Infrastructure/Repositories/ProdutoRepository.cs` — padrão de consulta com
  `AsNoTracking()`.
- `frontend/src/api/recursos.ts`, comentário acima de `pedidosApi` — a limitação
  que você está removendo. Não altere o frontend, só entenda o contexto.

## Passo a passo

### 1. Filtro de entrada

`Application/DTO/Pedidos/FiltroDePedidosDTO.cs`, herdando de
`ParametrosDePagina` (TAREFA-01):

```csharp
public class FiltroDePedidosDTO : ParametrosDePagina
{
    public StatusPedido? Status { get; set; }
    public int? ClienteId { get; set; }
    public int? UsuarioId { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
}
```

Usar o enum diretamente faz o ASP.NET Core recusar valor inválido com 400 antes
de chegar no seu código — de graça.

### 2. Validador

`FiltroDePedidosValidator`, herdando as regras de página:

- `ClienteId` e `UsuarioId`, quando informados, maiores que zero.
- `DataInicio` não pode ser posterior a `DataFim`.

### 3. DTO de saída

`PedidoResumoDTO` — **é diferente do `PedidoResponseDTO`**. A listagem não
devolve itens (RF-001.3.5):

```csharp
public class PedidoResumoDTO
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public int? UsuarioId { get; set; }
    public string? UsuarioNome { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime DataCadastro { get; set; }
    public decimal ValorTotal { get; set; }
    public int QuantidadeItens { get; set; }
}
```

### 4. Repositório

Adicione à interface:

```csharp
Task<(List<PedidoResumoDTO> Itens, int Total)> Listar(FiltroDePedidosDTO filtro);
```

> **Decisão a tomar e registrar:** devolver DTO direto do repositório contraria a
> separação de camadas que o projeto mantém. A alternativa purista é criar um
> tipo de leitura no domínio. A alternativa pragmática, comum em CQRS, é aceitar
> que a consulta projete direto. Escolha uma, e escreva o porquê no commit —
> não deixe implícito.

Na implementação:

```csharp
var consulta = _context.Pedidos.AsNoTracking().AsQueryable();

if (filtro.Status is not null)
    consulta = consulta.Where(p => p.Status == filtro.Status);

if (filtro.ClienteId is not null)
    consulta = consulta.Where(p => p.ClienteId == filtro.ClienteId);

if (filtro.UsuarioId is not null)
    consulta = consulta.Where(p => p.UsuarioId == filtro.UsuarioId);

if (filtro.DataInicio is not null)
    consulta = consulta.Where(p => p.DataCadastro >= filtro.DataInicio);

if (filtro.DataFim is not null)
{
    var limite = filtro.DataFim.Value.Date.AddDays(1);
    consulta = consulta.Where(p => p.DataCadastro < limite);
}

var total = await consulta.CountAsync();

var itens = await consulta
    .OrderByDescending(p => p.DataCadastro)
    .ThenByDescending(p => p.Id)
    .Skip(filtro.QuantidadeParaPular())
    .Take(filtro.TamanhoPagina)
    .Select(p => new PedidoResumoDTO { /* projeção */ })
    .ToListAsync();
```

Pontos que não são detalhe:

- **`ThenByDescending(p => p.Id)`** — sem desempate, dois pedidos com o mesmo
  instante podem trocar de posição entre a página 1 e a 2. O usuário vê o mesmo
  pedido duas vezes, ou nenhuma. Bug clássico de paginação.
- **`dataFim` é inclusiva no dia inteiro** — por isso `< dataFim + 1 dia`, e não
  `<= dataFim`. Com `<=`, um pedido das 14h do dia filtrado ficaria de fora.
- **`Skip`/`Take` depois do `OrderBy`**, sempre.
- **`CountAsync` antes da projeção**, sobre a consulta filtrada e sem
  materializar linha nenhuma (RNF-02).

### 5. Caso de uso — onde mora a regra de visibilidade

`ListarPedidosUseCase`. Aqui acontece o item mais importante da tarefa:

```csharp
var usuario = await _obterUsuarioAutenticado.ObterEntidade();

if (usuario.Role != UserRole.Admin)
{
    // RF-001.4.1: vendedor lista apenas os próprios pedidos.
    // O filtro recebido é descartado, não validado — mesmo que o cliente
    // envie o identificador de outro vendedor.
    filtro.UsuarioId = usuario.Id;
}
```

**Sobrescrever, não validar.** Se você recusasse com 403 quando o vendedor manda
o id de outro, estaria confirmando que aquele outro vendedor existe. Sobrescrever
em silêncio não vaza nada.

### 6. Controller

```csharp
[HttpGet]
public async Task<IActionResult> Listar([FromQuery] FiltroDePedidosDTO filtro)
    => Ok(await _listarPedidos.Executar(filtro));
```

O `[Authorize(Roles = "Admin,Vendedor")]` já está na classe. Não repita.

## Armadilhas

- **Não** traga tudo para a memória e pagine com LINQ depois. É proibido por
  RNF-01, e some com o ganho inteiro.
- **Não** use `Include(p => p.Itens)` na listagem. `QuantidadeItens` sai de uma
  projeção `p.Itens.Count`, que o EF traduz para subconsulta, sem trazer linha.
- **Fusos.** `DataCadastro` é gravado com `DateTime.UtcNow`. A comparação de
  `dataFim` tem que ser em UTC. Se você comparar com horário local, o filtro
  erra por algumas horas — e ninguém percebe até o fechamento do mês.
- Vendedor sem pedido algum recebe **200 com lista vazia**, nunca 404.

## Testes a escrever

Nível de caso de uso, com repositório mockado:

| Cenário | Esperado |
|---|---|
| Vendedor lista sem informar `usuarioId` | Repositório recebe filtro com o id do próprio vendedor |
| Vendedor informa `usuarioId` de outro | Filtro chega ao repositório com o id do **vendedor autenticado** |
| Admin lista sem filtro | Filtro chega com `UsuarioId` nulo |
| Admin informa `usuarioId` | Filtro chega com o id informado |

Use `Verify` com `It.Is<FiltroDePedidosDTO>(f => f.UsuarioId == 7)` para afirmar
sobre o que chegou no repositório — é o coração desses testes.

Nível de envelope:

| Cenário | Esperado |
|---|---|
| Total 137, tamanho 20, página 2 | 20 itens, `total` 137, `totalPaginas` 7 |
| Nenhum resultado | 200, `itens` vazio, `total` 0 |

## Critérios de aceite

- [ ] `GET /api/Pedido` existe e responde no envelope da TAREFA-01.
- [ ] Os cinco filtros funcionam, isolados e combinados.
- [ ] `dataFim` inclui o dia inteiro.
- [ ] Ordenação decrescente por data, desempatada por identificador.
- [ ] Vendedor nunca enxerga pedido de outro, nem forçando o parâmetro.
- [ ] Admin enxerga todos, inclusive os históricos sem autor.
- [ ] `tamanhoPagina` 0, 101 ou negativo devolve 400 em `problem+json` com
      `traceId`.
- [ ] A listagem não carrega os itens dos pedidos.
- [ ] Endpoint documentado no Swagger.

## Como validar

```bash
curl "https://localhost:5001/api/Pedido?status=Aberto&pagina=1&tamanhoPagina=5" \
  -H "Authorization: Bearer TOKEN_DE_VENDEDOR"
```

Repita com o token de outro vendedor: os conjuntos têm que ser disjuntos.

Para conferir a consulta gerada, ligue o log de SQL do EF Core em
desenvolvimento e verifique que sai **um** `SELECT` com `LIMIT`/`OFFSET` e um
`SELECT COUNT`. Se aparecer uma consulta por pedido, você tem um N+1.
