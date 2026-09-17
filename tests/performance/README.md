# Testes de desempenho de clientes

O arquivo `clientes.js` testa os endpoints reais de `ClienteController` com k6.
Ele autentica uma vez no inicio do teste e reutiliza o JWT em todas as chamadas.

## Antes de executar

Use uma base de dados exclusiva para testes. Os cenarios `cadastro`, `fluxo` e
`fluxo-admin` criam registros e nao os removem, pois a API nao possui endpoint de
exclusao de clientes.

Inicie a API em outro terminal:

```powershell
dotnet run --project .\GestaoPedidos\GestaoPedidos.csproj --configuration Release --launch-profile http
```

Configure as credenciais de um usuario de teste:

```powershell
$env:K6_BASE_URL = 'http://localhost:5143'
$env:K6_EMAIL = 'usuario-de-teste@empresa.com'
$env:K6_SENHA = 'SENHA_DO_USUARIO_DE_TESTE'
```

O usuario pode ter o papel `Vendedor` para todos os cenarios, exceto
`fluxo-admin`, que exige `Admin`.

## Executar

O modo padrao e `listagem`, que nao altera o banco:

```powershell
k6 run .\tests\performance\clientes.js
```

Para medir somente cadastro:

```powershell
$env:K6_CENARIO = 'cadastro'
k6 run .\tests\performance\clientes.js
```

Para testar cadastrar, consultar por ID, atualizar e listar:

```powershell
$env:K6_CENARIO = 'fluxo'
k6 run .\tests\performance\clientes.js
```

Para incluir inativacao e reativacao com um administrador:

```powershell
$env:K6_CENARIO = 'fluxo-admin'
k6 run .\tests\performance\clientes.js
```

## Ajustar a carga

```powershell
$env:K6_VUS = '20'
$env:K6_DURACAO = '2m'
$env:K6_PAUSA_SEGUNDOS = '1'
$env:K6_P95_MS = '500'
k6 run .\tests\performance\clientes.js
```

Variaveis disponiveis:

| Variavel | Padrao | Finalidade |
|---|---:|---|
| `K6_BASE_URL` | `http://localhost:5143` | Endereco da API |
| `K6_CENARIO` | `listagem` | `listagem`, `cadastro`, `fluxo` ou `fluxo-admin` |
| `K6_VUS` | `5` | Quantidade maxima de usuarios virtuais |
| `K6_DURACAO` | `30s` | Tempo da carga constante |
| `K6_PAUSA_SEGUNDOS` | `1` | Pausa entre iteracoes de cada usuario |
| `K6_P95_MS` | `500` | Limite de tempo p95 por operacao |

O teste falha quando 1% ou mais das requisicoes ou verificacoes apresentam erro,
ou quando o p95 de alguma operacao ultrapassa `K6_P95_MS`.

## Leitura inicial do resultado

- `http_reqs`: total e taxa de requisicoes por segundo.
- `http_req_duration`: tempo das requisicoes; priorize o p95.
- `http_req_failed`: proporcao de respostas HTTP com erro.
- `checks`: validacao dos status e dos corpos retornados pela API.

Execute cada cenario pelo menos tres vezes e compare resultados obtidos com a
mesma quantidade de dados no banco. A listagem atual retorna todos os clientes;
portanto, o tamanho da tabela influencia diretamente o tempo e o volume da
resposta.
