# TAREFA-01 — Envelope de paginação reutilizável

| | |
|---|---|
| Requisito | RF-001.3.3, RNF-07, decisão D3 |
| Depende de | — |
| Toca banco | Não |
| Tamanho | P |
| Risco | Baixo — nenhum endpoint existente muda |

## Objetivo

Criar o tipo genérico que todas as listagens paginadas da API vão devolver, e o
tipo que recebe os parâmetros de página. Nesta tarefa nada é usado ainda: é
infraestrutura para a TAREFA-05, e depois para as listagens de Cliente, Produto
e Usuário.

Fazer isso primeiro, separado, evita que o formato de paginação nasça enfiado
dentro do caso de uso de pedido e depois precise ser extraído.

## Contexto para ler antes

- `Application/DTO/` — veja como os DTOs existentes são organizados por pasta.
- `Application/Validators/Pedidos/CriarPedidoRequestValidator.cs` — o padrão de
  validação com FluentValidation que você vai repetir aqui.

## Passo a passo

### 1. Crie `Application/DTO/Comum/ResultadoPaginado.cs`

```csharp
namespace GestaoPedidos.Application.DTO.Comum
{
    public class ResultadoPaginado<T>
    {
        public IReadOnlyList<T> Itens { get; init; } = [];
        public int Pagina { get; init; }
        public int TamanhoPagina { get; init; }
        public int Total { get; init; }
        public int TotalPaginas => TamanhoPagina == 0
            ? 0
            : (int)Math.Ceiling(Total / (double)TamanhoPagina);
    }
}
```

`TotalPaginas` é derivado, não armazenado — não existe como ficar dessincronizado.

### 2. Crie `Application/DTO/Comum/ParametrosDePagina.cs`

```csharp
namespace GestaoPedidos.Application.DTO.Comum
{
    public class ParametrosDePagina
    {
        public const int TamanhoPadrao = 20;
        public const int TamanhoMaximo = 100;

        public int Pagina { get; set; } = 1;
        public int TamanhoPagina { get; set; } = TamanhoPadrao;

        public int QuantidadeParaPular() => (Pagina - 1) * TamanhoPagina;
    }
}
```

### 3. Crie `Application/Validators/Comum/ParametrosDePaginaValidator.cs`

- `Pagina` maior ou igual a 1.
- `TamanhoPagina` entre 1 e `ParametrosDePagina.TamanhoMaximo`.

Herde de `AbstractValidator<ParametrosDePagina>`. O registro é automático — o
`Program.cs` já varre o assembly com `AddValidatorsFromAssemblyContaining`.

## Armadilhas

- **Não** use `IReadOnlyList<T>` com `= null!`. Com `TreatWarningsAsErrors`
  ligado, prefira inicializar com `[]` como no exemplo.
- **Não** coloque `TotalPaginas` como propriedade gravável. Se um dia alguém
  preencher errado, a paginação mente e ninguém percebe.
- Se `TamanhoPagina` chegar 0, `TotalPaginas` divide por zero. O validador
  impede, mas a guarda no cálculo é barata e protege quem construir o objeto
  direto no código.

## Testes a escrever

Arquivo: `GestaoPedidosTests/Application/DTO/Comum/ResultadoPaginadoTests.cs`

| Cenário | Esperado |
|---|---|
| Total 137, tamanho 20 | `TotalPaginas` é 7 |
| Total 140, tamanho 20 | `TotalPaginas` é 7 |
| Total 0, tamanho 20 | `TotalPaginas` é 0 |
| Total 1, tamanho 20 | `TotalPaginas` é 1 |
| `TamanhoPagina` 0 | `TotalPaginas` é 0, sem exceção |

Arquivo: `GestaoPedidosTests/Application/Validators/Comum/ParametrosDePaginaValidatorTests.cs`

| Entrada | Esperado |
|---|---|
| Pagina 0 | Inválido |
| Pagina 1, tamanho 20 | Válido |
| Tamanho 0 | Inválido |
| Tamanho 101 | Inválido |
| Tamanho 100 | Válido |

Lembre do padrão MSTest do projeto:

```csharp
[TestClass]
public class ResultadoPaginadoTests
{
    [TestMethod]
    public void TotalPaginas_Deve_Arredondar_Para_Cima()
    {
        var resultado = new ResultadoPaginado<string>
        {
            Total = 137,
            TamanhoPagina = 20
        };

        resultado.TotalPaginas.Should().Be(7);
    }
}
```

## Critérios de aceite

- [ ] `ResultadoPaginado<T>` e `ParametrosDePagina` existem em
      `Application/DTO/Comum/`.
- [ ] `TotalPaginas` é calculado, não atribuído.
- [ ] Validador recusa página menor que 1 e tamanho fora de 1..100.
- [ ] Todos os cenários das tabelas acima têm teste.
- [ ] Build, format e testes passam.

## Como validar

```bash
dotnet test GestaoPedidos.sln --filter "ClassName~Paginad"
```
