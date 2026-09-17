# RF-001 — Pedido com dono e consulta de pedidos

| Campo | Valor |
|---|---|
| Identificador | RF-001 |
| Título | Pedido com dono e consulta de pedidos |
| Produto | Gestão de Pedidos — API (`GestaoPedidos`) |
| Solicitante | Product Owner |
| Data | 29/08/2026 |
| Versão | 1.0 |
| Status | Aprovado para desenvolvimento |
| Escopo | Somente API. Nenhuma alteração em `frontend/`. |
| Épicos cobertos | Rastreabilidade de venda, Consulta de pedidos |

---

## 1. Contexto

A API hoje cria, consulta por identificador, finaliza e cancela pedidos. Não
existe **nenhuma forma de listar pedidos**: a interface do repositório traz a
assinatura comentada,

```csharp
// GestaoPedidos/Domain/Abstractions/IPedidoRepository.cs
//Task <List<Pedido>> Listar();
```

e o consumidor atual contorna a ausência guardando os identificadores que
conhece em armazenamento local do navegador (`frontend/src/pedidos/registro.ts`,
com a limitação admitida em comentário no próprio `frontend/src/api/recursos.ts`).

Além disso, a entidade `Pedido` guarda `ClienteId`, `Status`, `DataCadastro` e
`Itens`, mas **não registra qual usuário realizou a venda**.

### 1.1 Problemas decorrentes

| # | Problema | Efeito operacional |
|---|---|---|
| P1 | Não há listagem de pedidos | Ninguém consegue responder "quais pedidos estão abertos hoje?" |
| P2 | O conjunto de pedidos visíveis depende do armazenamento local | Trocar de navegador ou limpar o cache faz os pedidos desaparecerem da tela |
| P3 | Indicadores agregados são calculados sobre esse conjunto incidental | Faturamento e contagem exibidos hoje estão **errados por construção** |
| P4 | O pedido não tem autor | Não existe ranking de vendedor, comissão, nem "meus pedidos" |
| P5 | Qualquer vendedor cancela o pedido de qualquer outro | Alteração indevida sem rastro e sem responsável |
| P6 | `ValorTotal` não é persistido (`entity.Ignore(p => p.ValorTotal)`) | Impossível filtrar por faixa de valor ou somar faturamento em SQL |
| P7 | `Pedido.AtualizarItem()` existe no domínio e nenhum endpoint o utiliza | Regra implementada e nunca entregue ao usuário |

## 2. Objetivo

Tornar o pedido **rastreável** (todo pedido novo sabe quem vendeu) e
**consultável** (listagem paginada com filtros, servida pela API e não
reconstruída pelo consumidor), preservando as garantias já existentes de
transação, reserva de estoque e idempotência.

### 2.1 Resultado esperado

Ao final desta entrega, a API responde, sem depender de nenhum estado no
cliente:

- quais pedidos existem, com filtro por situação, cliente, vendedor e período;
- quem vendeu cada pedido e quem executou a última transição de situação;
- qual o valor total de cada pedido, em coluna consultável;
- alteração dos itens de um pedido ainda aberto.

## 3. Escopo

### 3.1 Incluído

1. Campo de autor no pedido, preenchido pelo servidor.
2. Persistência do valor total do pedido.
3. Endpoint de listagem paginada com filtros.
4. Restrição de visibilidade e de transição por perfil.
5. Registro de quem executou a transição de situação.
6. Endpoint de alteração de itens de pedido aberto.
7. Migração de banco de dados, incluindo carga retroativa do valor total.
8. Testes automatizados dos itens acima.

### 3.2 Excluído desta entrega

- Qualquer alteração no diretório `frontend/`.
- Endpoints de relatório e agregação (faturamento por período, ticket médio,
  ranking) — entrega seguinte, dependente desta.
- Movimentação de estoque com histórico.
- Renovação de token (*refresh token*) e recuperação de senha.
- Paginação nas listagens de Cliente, Produto e Usuário — mesmo padrão de
  envelope definido aqui, porém em entrega separada.
- Exclusão de pedido. Pedido não é excluído, é cancelado.

## 4. Decisões de produto

Registradas para não serem reabertas durante o desenvolvimento.

| # | Decisão | Justificativa |
|---|---|---|
| D1 | Vendedor enxerga e altera **apenas os pedidos que criou**; administrador enxerga todos | Coerente com o restante da API, que é restritiva por padrão |
| D2 | Acesso a pedido de outro vendedor responde **404**, não 403 | 403 confirmaria a existência do pedido a quem não pode vê-lo |
| D3 | Paginação em **envelope no corpo** da resposta | Autodescritivo no Swagger; passa a ser o padrão das listagens da API |
| D4 | `ValorTotal` passa a ser **coluna persistida** | Habilita filtro, ordenação e agregação em SQL; pré-requisito dos relatórios |
| D5 | `UsuarioId` **anulável** para os pedidos já existentes, obrigatório para os novos | Não se atribui autoria que não ocorreu |
| D6 | Administrador **pode** transicionar pedido de qualquer vendedor, e a ação fica registrada | Operação não pode travar quando um vendedor deixa a empresa |
| D7 | O preço do item **não é reprecificado** em alteração de pedido | O pedido preserva o preço praticado na criação |

## 5. Perfis e permissões

| Ação | Vendedor | Admin |
|---|---|---|
| Criar pedido | Sim (fica como autor) | Sim (fica como autor) |
| Consultar pedido próprio | Sim | Sim |
| Consultar pedido de outro | **Não** (404) | Sim |
| Listar pedidos | Sim, apenas os próprios | Sim, todos |
| Alterar itens de pedido próprio aberto | Sim | Sim |
| Alterar itens de pedido de outro | **Não** (404) | Sim |
| Finalizar/cancelar pedido próprio | Sim | Sim |
| Finalizar/cancelar pedido de outro | **Não** (404) | Sim |

Pedidos históricos sem autor (`UsuarioId` nulo) são visíveis **somente para
administrador**.

---

## 6. Requisitos funcionais

### RF-001.1 — Registro do vendedor no pedido

O sistema deve associar todo pedido criado ao usuário autenticado que o criou.

- **RF-001.1.1** O identificador do usuário é obtido da *claim*
  `ClaimTypes.NameIdentifier` do token da requisição.
- **RF-001.1.2** O campo **não pode** existir em `CriarPedidoRequestDTO`. O
  cliente da API não escolhe o autor do pedido.
- **RF-001.1.3** A resposta de pedido passa a expor `usuarioId` e `usuarioNome`.
- **RF-001.1.4** Pedidos criados antes desta entrega permanecem sem autor.

### RF-001.2 — Persistência do valor total

- **RF-001.2.1** `Pedido.ValorTotal` deixa de ser propriedade ignorada pelo
  mapeamento e passa a ser coluna `numeric(18,2)`.
- **RF-001.2.2** O valor é calculado como a soma de `Preco × Quantidade` dos
  itens, dentro da mesma transação que cria ou altera o pedido.
- **RF-001.2.3** O valor é recalculado em toda alteração de itens.
- **RF-001.2.4** Restrição de verificação no banco garante valor maior que zero.

### RF-001.3 — Listagem de pedidos

- **RF-001.3.1** Novo endpoint `GET /api/Pedido`, autenticado, disponível para
  os perfis Admin e Vendedor.
- **RF-001.3.2** Filtros aceitos: `status`, `clienteId`, `usuarioId`,
  `dataInicio`, `dataFim`.
- **RF-001.3.3** Paginação por `pagina` e `tamanhoPagina`.
- **RF-001.3.4** Ordenação padrão por `DataCadastro` decrescente, desempatada
  por `Id` decrescente, para que a paginação seja estável.
- **RF-001.3.5** A listagem **não** retorna os itens de cada pedido, apenas o
  cabeçalho com o valor total.
- **RF-001.3.6** Todos os filtros são combináveis, em conjunção lógica.

### RF-001.4 — Restrição de visibilidade

- **RF-001.4.1** Para requisição autenticada com perfil Vendedor, o filtro
  `usuarioId` informado na consulta é **descartado e substituído** pelo
  identificador do próprio usuário.
- **RF-001.4.2** `GET /api/Pedido/{id}` de pedido que o solicitante não pode ver
  responde 404.
- **RF-001.4.3** A mesma restrição vale para alterar itens, finalizar e cancelar.

### RF-001.5 — Registro da transição

- **RF-001.5.1** `Pedido` passa a guardar `TransicionadoPorUsuarioId` e
  `TransicionadoEm`, preenchidos ao finalizar e ao cancelar.
- **RF-001.5.2** Em chamada idempotente (finalizar um pedido já finalizado, ou
  cancelar um já cancelado), os campos **não** são sobrescritos: preservam a
  transição original.
- **RF-001.5.3** Ambos são expostos na resposta de pedido.

### RF-001.6 — Alteração de itens de pedido aberto

- **RF-001.6.1** Novo endpoint `PUT /api/Pedido/{id}/itens`, que recebe a lista
  completa e desejada de itens do pedido.
- **RF-001.6.2** Aplicável somente a pedido com situação `Aberto`.
- **RF-001.6.3** A reserva de estoque é ajustada na mesma transação:
  - quantidade aumentada → `ReservarQuantidade` da diferença;
  - quantidade reduzida → `LiberarReserva` da diferença;
  - item removido → `LiberarReserva` da quantidade integral;
  - item novo → `ReservarQuantidade` da quantidade integral.
- **RF-001.6.4** Item novo entra com o preço vigente do produto no momento da
  alteração; item que permanece **mantém** o preço registrado originalmente.
- **RF-001.6.5** O pedido não pode ficar sem itens.

### RF-001.7 — Correção do código de retorno em pedido inexistente

`ObterPedidoPorIdUseCase` lança hoje `BadRequestException` para pedido não
encontrado, o que produz **400** onde a semântica é **404**. Como esta entrega
altera exatamente esse caminho, a correção entra no escopo.

---

## 7. Regras de negócio

| # | Regra |
|---|---|
| RN-01 | Todo pedido criado a partir desta versão tem exatamente um autor, definido pelo servidor. |
| RN-02 | O autor de um pedido nunca muda. |
| RN-03 | Vendedor só acessa pedido cujo autor é ele próprio. |
| RN-04 | Pedido sem autor é acessível apenas por administrador. |
| RN-05 | Pedido só é alterado enquanto estiver `Aberto`. |
| RN-06 | Pedido nunca fica sem itens; a remoção do último item é recusada. |
| RN-07 | O mesmo produto não aparece duas vezes no mesmo pedido. |
| RN-08 | O preço de um item já existente no pedido não é alterado por reprecificação do produto. |
| RN-09 | Alteração de itens não pode deixar reserva ou estoque negativos. |
| RN-10 | Finalizar confirma a reserva; cancelar devolve ao estoque. Ambas permanecem idempotentes. |
| RN-11 | Produto inativo não pode ser adicionado a um pedido, nem ter sua quantidade aumentada. |
| RN-12 | Cliente inativo não impede a alteração de um pedido que já existe. |
| RN-13 | Toda operação de escrita ocorre em transação única. |

---

## 8. Contrato da API

Erros seguem o padrão já vigente `application/problem+json`, com `traceId`,
produzido por `ExceptionMiddleware`.

### 8.1 `GET /api/Pedido`

**Parâmetros de consulta**

| Parâmetro | Tipo | Obrigatório | Padrão | Regra |
|---|---|---|---|---|
| `status` | `Aberto` \| `Finalizado` \| `Cancelado` | Não | — | Valor inválido → 400 |
| `clienteId` | inteiro | Não | — | Maior que zero |
| `usuarioId` | inteiro | Não | — | Ignorado quando o solicitante é Vendedor |
| `dataInicio` | data | Não | — | Inclusiva |
| `dataFim` | data | Não | — | Inclusiva no dia inteiro (`< dataFim + 1 dia`, em UTC) |
| `pagina` | inteiro | Não | `1` | Mínimo 1 |
| `tamanhoPagina` | inteiro | Não | `20` | Entre 1 e 100 |

**Resposta 200**

```json
{
  "itens": [
    {
      "id": 128,
      "clienteId": 42,
      "clienteNome": "Maria Souza",
      "usuarioId": 7,
      "usuarioNome": "João Vendedor",
      "status": "Aberto",
      "dataCadastro": "2026-08-29T13:04:11Z",
      "valorTotal": 349.90,
      "quantidadeItens": 3
    }
  ],
  "pagina": 1,
  "tamanhoPagina": 20,
  "total": 137,
  "totalPaginas": 7
}
```

**Demais respostas**

| Código | Situação |
|---|---|
| 400 | Parâmetro fora da faixa, data mal formada, `dataInicio` posterior a `dataFim` |
| 401 | Sem token, token expirado ou revogado |
| 403 | Perfil sem acesso ao recurso |

Página sem resultados retorna **200** com `itens` vazio e `total` igual a zero.

### 8.2 `GET /api/Pedido/{id}`

Mantém o formato atual, acrescido de `usuarioId`, `usuarioNome`, `clienteNome`,
`transicionadoPorUsuarioId` e `transicionadoEm`.

| Código | Situação |
|---|---|
| 200 | Pedido acessível ao solicitante |
| 404 | Pedido inexistente **ou** pertencente a outro vendedor |

### 8.3 `PUT /api/Pedido/{id}/itens`

**Requisição**

```json
{
  "itens": [
    { "produtoId": 3, "quantidade": 2 },
    { "produtoId": 9, "quantidade": 1 }
  ]
}
```

**Validação** (`FluentValidation`, mesmo padrão de `CriarPedidoRequestValidator`)

- `itens` não vazio;
- `produtoId` maior que zero;
- `quantidade` maior que zero;
- sem `produtoId` repetido na mesma requisição.

**Respostas**

| Código | Situação |
|---|---|
| 200 | Pedido atualizado, com itens e valor total recalculado |
| 400 | Validação de entrada, ou pedido não está `Aberto` |
| 404 | Pedido inexistente, de outro vendedor, ou produto informado inexistente |
| 409 | Estoque insuficiente para o aumento solicitado, ou conflito de concorrência no produto |

### 8.4 `PUT /api/Pedido/{id}/finalizar` e `PUT /api/Pedido/{id}/cancelar`

Comportamento preservado, com duas alterações: passam a respeitar a restrição de
visibilidade (404 para pedido de outro vendedor) e passam a registrar autor e
instante da transição.

---

## 9. Modelo de dados

### 9.1 Alterações em `Pedidos`

| Coluna | Tipo | Nulo | Observação |
|---|---|---|---|
| `UsuarioId` | integer | Sim | Nulo apenas para registros anteriores à migração |
| `ValorTotal` | numeric(18,2) | Não | Carga retroativa a partir dos itens |
| `TransicionadoPorUsuarioId` | integer | Sim | Preenchido na primeira transição |
| `TransicionadoEm` | timestamp with time zone | Sim | Preenchido na primeira transição |

### 9.2 Restrições e índices

- Chave estrangeira `Pedidos.UsuarioId` → `Usuarios.Id`, com `OnDelete: Restrict`,
  seguindo o comportamento já adotado para `ClienteId`.
- Chave estrangeira `Pedidos.TransicionadoPorUsuarioId` → `Usuarios.Id`, com
  `OnDelete: Restrict`.
- Índice `(UsuarioId, DataCadastro DESC)` — atende ao caminho mais frequente,
  que é o vendedor listando os próprios pedidos.
- Índice `(Status, DataCadastro DESC)`.
- Restrição de verificação `CK_Pedidos_ValorTotal_Positivo`
  (`"ValorTotal" > 0`), no mesmo padrão de nomenclatura já usado em `Produtos`
  e `PedidoItens`.

### 9.3 Migração

1. Criar as colunas, `ValorTotal` inicialmente anulável.
2. Preencher `ValorTotal` de todos os pedidos existentes com a soma de
   `Preco × Quantidade` de `PedidoItens`.
3. Interromper a migração, com mensagem clara, se algum pedido resultar em
   valor total nulo ou não positivo — mesma abordagem defensiva já adotada na
   migração de *hardening*.
4. Tornar `ValorTotal` obrigatório e criar a restrição de verificação.
5. Criar os índices.

A migração precisa ser reversível.

---

## 10. Requisitos não funcionais

| # | Requisito |
|---|---|
| RNF-01 | A listagem executa em consulta única paginada no banco. É proibido trazer todos os pedidos para a memória e paginar em C#. |
| RNF-02 | A contagem total é obtida em consulta de agregação, sem materializar as linhas. |
| RNF-03 | O tempo de resposta da listagem permanece abaixo de 300 ms para 100 mil pedidos, com filtro por vendedor e período. |
| RNF-04 | A camada `Domain` continua sem dependência de ASP.NET Core. A obtenção do usuário autenticado permanece na camada de aplicação, reaproveitando `ObterUsuarioAutenticadoUseCase`. |
| RNF-05 | Os novos endpoints aparecem no Swagger com os códigos de resposta documentados. |
| RNF-06 | Nenhum endpoint existente muda de contrato de forma incompatível: os campos novos são adicionados, nenhum é removido ou renomeado. |
| RNF-07 | O envelope de paginação é definido como tipo genérico reutilizável, para servir às listagens de Cliente, Produto e Usuário na entrega seguinte. |
| RNF-08 | `dotnet format --verify-no-changes` e a suíte de testes passam sem falha. |

---

## 11. Critérios de aceite

### CA-01 — Autoria do pedido

```gherkin
Dado que estou autenticado como o vendedor "João"
Quando eu criar um pedido válido
Então o pedido criado tem "João" como autor
E a resposta traz usuarioId e usuarioNome preenchidos
```

```gherkin
Dado que estou autenticado como o vendedor "João"
Quando eu enviar um pedido com o identificador de outro usuário no corpo
Então o campo é ignorado
E o pedido é registrado com "João" como autor
```

### CA-02 — Valor total

```gherkin
Dado um pedido com dois itens de 10,00 na quantidade 2 e 5,50 na quantidade 4
Quando o pedido for criado
Então a coluna ValorTotal do pedido vale 42,00
```

```gherkin
Dado um pedido aberto com ValorTotal 42,00
Quando eu remover um dos itens
Então ValorTotal é recalculado e persistido no mesmo instante
```

### CA-03 — Listagem e paginação

```gherkin
Dado que existem 137 pedidos que posso ver
Quando eu consultar a listagem com pagina=2 e tamanhoPagina=20
Então recebo 20 itens
E o envelope informa total 137 e totalPaginas 7
```

```gherkin
Quando eu consultar a listagem com tamanhoPagina=500
Então recebo 400 em application/problem+json
E a resposta contém traceId
```

```gherkin
Dado que existem pedidos criados em 10/08 e em 15/08
Quando eu filtrar por dataInicio=10/08 e dataFim=10/08
Então recebo apenas o pedido de 10/08
```

### CA-04 — Visibilidade

```gherkin
Dado que o vendedor "Ana" possui o pedido 55
E que estou autenticado como o vendedor "João"
Quando eu consultar a listagem informando usuarioId de "Ana"
Então o filtro é substituído pelo meu identificador
E o pedido 55 não aparece no resultado
```

```gherkin
Dado que estou autenticado como o vendedor "João"
Quando eu consultar o pedido 55, pertencente a "Ana"
Então recebo 404
E a resposta não revela que o pedido existe
```

```gherkin
Dado que estou autenticado como administrador
Quando eu consultar a listagem sem filtro de vendedor
Então vejo pedidos de todos os vendedores, inclusive os históricos sem autor
```

### CA-05 — Transição

```gherkin
Dado que estou autenticado como o vendedor "João"
Quando eu tentar cancelar o pedido 55, pertencente a "Ana"
Então recebo 404
E o pedido 55 permanece aberto
```

```gherkin
Dado que estou autenticado como administrador
Quando eu cancelar o pedido 55, pertencente a "Ana"
Então o pedido é cancelado
E o pedido registra que a transição foi executada por mim, com o instante
```

```gherkin
Dado um pedido já finalizado por "Ana" ontem
Quando eu, como administrador, chamar finalizar novamente hoje
Então a resposta é 200
E o registro da transição continua sendo o de "Ana", de ontem
E o estoque não é alterado uma segunda vez
```

### CA-06 — Alteração de itens

```gherkin
Dado um pedido aberto com 2 unidades do produto A, cujo estoque livre é 10
Quando eu alterar a quantidade do produto A para 5
Então o estoque livre do produto A passa a 7
E a quantidade reservada aumenta em 3
E ValorTotal é recalculado
```

```gherkin
Dado um pedido aberto com 5 unidades do produto A, cujo estoque livre é 1
Quando eu alterar a quantidade do produto A para 9
Então recebo 409
E nem o pedido nem o estoque são alterados
```

```gherkin
Dado um pedido aberto com um único item
Quando eu enviar uma lista de itens vazia
Então recebo 400
E o pedido permanece inalterado
```

```gherkin
Dado um pedido finalizado
Quando eu tentar alterar seus itens
Então recebo 400
```

```gherkin
Dado um pedido aberto com o produto A ao preço de 10,00
E que o preço cadastrado do produto A passou a 12,00
Quando eu alterar a quantidade do produto A
Então o item continua registrado a 10,00
```

---

## 12. Cenários de teste obrigatórios

Além dos critérios acima, a entrega precisa cobrir:

1. Alteração de itens que falha no meio da operação não deixa estoque
   inconsistente — verificar o efeito da transação.
2. Duas alterações concorrentes no mesmo produto: a segunda recebe 409 pelo
   token de concorrência `Produto.Versao`, e não sobrescreve a primeira.
3. Listagem com filtro por período que não retorna nada devolve 200 com
   envelope vazio, e não 404.
4. Pedido histórico sem autor não aparece para vendedor algum e aparece para
   administrador.
5. Token de vendedor inativo ou revogado recebe 401 na listagem, pelo mecanismo
   já existente de versão de token.

---

## 13. Riscos e impactos

| Risco | Efeito | Mitigação |
|---|---|---|
| Carga retroativa de `ValorTotal` divergir dos itens | Faturamento histórico incorreto | Migração interrompe e relata pedidos com valor não positivo antes de aplicar a restrição |
| `ValorTotal` dessincronizar dos itens ao longo do tempo | Relatórios futuros mentem | Recálculo centralizado, exigido em toda operação que altera itens, e teste que compara coluna e soma |
| Consumidores atuais dependem do índice local de pedidos | Comportamento duplicado após a entrega | Fora do escopo desta entrega; comunicar que o índice local pode ser aposentado |
| Vendedor deixar a empresa com pedidos abertos | Pedidos sem quem os transicione | Decisão D6 — administrador transiciona qualquer pedido |
| Índices novos em tabela grande | Migração demorada | Aplicar em janela de manutenção; avaliar criação concorrente no PostgreSQL |

---

## 14. Dependências

- Nenhuma dependência externa. Todo o material necessário já existe:
  autenticação com identificação do usuário, transação por unidade de trabalho,
  reserva de estoque com controle otimista e tratamento padronizado de erro.
- **Esta entrega é pré-requisito** do épico de relatórios: faturamento por
  período, ticket médio e ranking de vendedor dependem de `ValorTotal`
  persistido e de `UsuarioId` no pedido.

## 15. Definição de pronto

- [ ] Requisitos funcionais RF-001.1 a RF-001.7 implementados.
- [ ] Migração aplicada e revertida com sucesso em base com dados.
- [ ] Todos os critérios de aceite cobertos por teste automatizado em
      `GestaoPedidosTests`.
- [ ] Cenários da seção 12 cobertos.
- [ ] `dotnet format --verify-no-changes`, `dotnet build` e `dotnet test` sem
      falha.
- [ ] Swagger exibindo os endpoints novos com os códigos documentados.
- [ ] `README.md` atualizado na tabela de permissões, com a regra de
      visibilidade por vendedor.
- [ ] Nenhum contrato existente quebrado.

## 16. Questões em aberto

| # | Questão | Responsável | Situação |
|---|---|---|---|
| Q1 | O cliente do sistema é pessoa física, jurídica ou ambos? Define se `Cliente` ganha CNPJ e razão social | Product Owner | **Em aberto** — não bloqueia esta entrega |
| Q2 | Comissão de venda entra no roteiro? Muda o que o relatório de vendedor precisa expor | Product Owner | Em aberto |
| Q3 | Pedido histórico sem autor deve ser atribuível manualmente por um administrador? | Product Owner | Em aberto |

---

## Anexo A — Backlog subsequente

Registrado para dar contexto de sequência, fora do escopo desta entrega.

| Ordem | Item | Observação |
|---|---|---|
| 1 | Paginação e busca em Cliente, Produto e Usuário | Reaproveita o envelope definido em RNF-07 |
| 2 | Endpoint de relatórios de venda | Depende desta entrega |
| 3 | Movimentação de estoque com histórico | `Domain/Entities/Movimentacao.cs` está vazio; hoje não há como auditar variação de estoque |
| 4 | Renovação de token | Sessão expira sem renovação, com `ClockSkew` zerado |
| 5 | Recuperação e redefinição de senha | Não existe nem por autoatendimento, nem por administrador |
| 6 | Trilha de auditoria de ações administrativas | Alteração de papel, de preço e inativação não deixam rastro |
