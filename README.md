# Gestão de Pedidos

API REST para gestão de pessoas, produtos, usuários e pedidos, construída com
.NET 8, ASP.NET Core, Entity Framework Core e PostgreSQL.

## Principais garantias

- JWT com validação de emissor, audiência, expiração e versão do token.
- Papéis `Admin` e `Vendedor`, com autorização restritiva por padrão.
- Somente administradores cadastram usuários; novos cadastros recebem o perfil Vendedor, e somente admin altera papéis.
- Troca de senha, alteração de papel e inativação revogam tokens anteriores.
- Reservas de estoque com controle otimista de concorrência.
- Criação, cancelamento e finalização de pedidos executados em transação.
- Cancelamento e finalização idempotentes.
- Índices únicos, foreign keys e check constraints no PostgreSQL.
- Erros no padrão `application/problem+json`, incluindo `traceId`.
- Rate limit nos endpoints de cadastro e autenticação.

## Pré-requisitos

- .NET SDK 8.0.414 ou patch mais recente da linha 8.0.4xx, conforme `global.json`.
- PostgreSQL.

## Configuração

Não grave credenciais reais em `appsettings.json`. Configure por variáveis de
ambiente ou User Secrets:

```powershell
dotnet user-secrets init --project GestaoPedidos/GestaoPedidos.csproj
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=gestao_pedidos;Username=postgres;Password=SUA_SENHA" --project GestaoPedidos/GestaoPedidos.csproj
dotnet user-secrets set "Jwt:Key" "UMA_CHAVE_ALEATORIA_COM_PELO_MENOS_32_CARACTERES" --project GestaoPedidos/GestaoPedidos.csproj
```

Em produção, configure também `Jwt:Issuer`, `Jwt:Audience` e as origens
permitidas em `Cors:AllowedOrigins`.

Em desenvolvimento, outra opção é copiar `GestaoPedidos/appsettings.Local.example.json`
para `GestaoPedidos/appsettings.Local.json` e preencher a conexão e uma chave JWT
aleatória com pelo menos 32 caracteres. O arquivo local é ignorado pelo Git e não
é publicado. User Secrets, variáveis de ambiente e argumentos de execução têm
precedência sobre ele. O arquivo local não é carregado em produção.

Se uma credencial já foi versionada, removê-la do arquivo não a revoga: troque a
senha no serviço correspondente. A chave JWT local foi renovada; tokens emitidos
com a chave anterior deixam de ser aceitos.

O AutoMapper foi atualizado para 15.1.4 para corrigir vulnerabilidades. Essa linha
possui configuração de licença: informe sua chave em `AutoMapper:LicenseKey`
(ou `AutoMapper__LicenseKey` no ambiente), conforme a
[documentação do fornecedor](https://docs.automapper.io/en/stable/15.0-Upgrade-Guide.html).

## Banco de dados

```powershell
dotnet tool restore
dotnet ef database update --project GestaoPedidos/GestaoPedidos.csproj --startup-project GestaoPedidos/GestaoPedidos.csproj
```

A migração de hardening verifica duplicidades, referências órfãs, estoques
negativos e valores fora dos limites antes de criar as constraints. Se ela
interromper, corrija os registros indicados e execute novamente.

## Primeiro administrador

Após aplicar as migrações, informe temporariamente:

```text
BootstrapAdmin__Nome=Administrador
BootstrapAdmin__Email=admin@empresa.com
BootstrapAdmin__Senha=UMA_SENHA_FORTE
```

Inicie a aplicação uma vez. Depois que o administrador for criado, remova a
variável `BootstrapAdmin__Senha`.

## Executar

```powershell
dotnet run --project GestaoPedidos/GestaoPedidos.csproj
```

Em desenvolvimento, o Swagger fica disponível na URL exibida pelo terminal.
O health check público é `GET /health`.

## Validar o projeto

```powershell
dotnet format GestaoPedidos/GestaoPedidos.sln --verify-no-changes --no-restore
dotnet build GestaoPedidos/GestaoPedidos.sln --no-restore
dotnet test GestaoPedidos/GestaoPedidos.sln --no-build --no-restore
dotnet list GestaoPedidos/GestaoPedidos.csproj package --vulnerable --include-transitive
```

## Permissões resumidas

| Recurso | Vendedor | Admin |
|---|---:|---:|
| Consultar/criar/editar pessoas | Sim | Sim |
| Ativar/inativar pessoas | Não | Sim |
| Consultar produtos | Sim | Sim |
| Alterar produtos e estoque | Não | Sim |
| Criar/finalizar/cancelar pedidos de venda | Somente os próprios | Sim |
| Consultar/criar/finalizar/cancelar pedidos de compra | Não | Sim |
| Listar e administrar usuários | Não | Sim |
| Alterar o próprio perfil e senha | Sim | Sim |

Vendedores consultam e operam somente seus pedidos de venda. Pedidos de compra
são restritos a administradores, inclusive após a mudança de papel do usuário
que criou o pedido. Finalização e cancelamento repetidos preservam o resultado
anterior sem repetir a movimentação de estoque.

## Pessoas

O cadastro de pessoas usa `nome`, `email` e `cpf`.

| Operação | Endpoint | Permissão |
|---|---|---|
| Cadastrar | `POST /api/Pessoa` | Admin ou Vendedor |
| Listar | `GET /api/Pessoa` | Admin ou Vendedor |
| Consultar | `GET /api/Pessoa/{id}` | Admin ou Vendedor |
| Atualizar | `PUT /api/Pessoa/{id}` | Admin ou Vendedor |
| Ativar | `PUT /api/Pessoa/{id}/ativar` | Admin |
| Inativar | `PUT /api/Pessoa/{id}/inativar` | Admin |

Pedidos mantêm suas rotas e usam `pessoaId` e `pessoaNome` nas respostas. A criação
recebe `pessoaId`, `tipoMovimentacao` e `itens`. O vínculo interno ainda se chama
`ClienteId` e aponta para Pessoas. A migração `RenomearClientesParaPessoas` renomeia
a tabela e seus índices, preservando os registros e os vínculos existentes.
O campo `TipoPessoa` já presente na entidade é incluído com valor inicial
`Cliente` nos cadastros anteriores.

Alterações de estoque pela edição geral de produtos também geram uma movimentação
de ajuste manual, na mesma transação da atualização. Quantidade zero em ajustes,
estouro do limite de inteiros e valores monetários fora de `numeric(18,2)` são
rejeitados. Novas senhas usam BCrypt com pré-hash SHA-384; os hashes BCrypt
anteriores continuam sendo reconhecidos.

Pessoas e produtos usam `pagina=1` e `tamanhoPagina=20` por padrão. O tamanho
aceito vai de 1 a 100; páginas inválidas ou cujo deslocamento ultrapasse o limite
suportado retornam `400`. A resposta continua sendo uma lista simples.

O JSON de criação de pedidos deve informar `tipoMovimentacao` explicitamente.
Omitir o campo ou enviar `null` retorna `400`, sem assumir compra ou venda.
Os itens vinculados a um pedido são alterados pelo método `Pedido.AtualizarItem`,
que protege o status e recalcula o total. O construtor copia os itens recebidos,
evitando alterações do pedido por referências externas.

Usuários usam `VersaoToken` também como controle de concorrência. Uma edição
baseada em dados antigos recebe `409`, preservando as mudanças de senha, papel,
situação e revogação de tokens realizadas por outra requisição. O repositório
salva somente as propriedades efetivamente alteradas da entidade rastreada.
A migração `ProtegeConcorrenciaUsuarioEItensPedido` registra essas alterações
de mapeamento; ela não cria nem remove colunas ou registros.
