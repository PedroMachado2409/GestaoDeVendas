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
| Criar/finalizar/cancelar pedidos | Sim | Sim |
| Listar e administrar usuários | Não | Sim |
| Alterar o próprio perfil e senha | Sim | Sim |

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

Pedidos mantêm suas rotas e os campos `clienteId` e `clienteNome`. O vínculo
interno aponta para Pessoas. A migração `RenomearClientesParaPessoas` renomeia
a tabela e seus índices, preservando os registros e os vínculos existentes.
O campo `TipoPessoa` já presente na entidade é incluído com valor inicial
`Cliente` nos cadastros anteriores.
