# TAREFA-03 — Preencher o autor na criação e expor na resposta

| | |
|---|---|
| Requisito | RF-001.1.1 a RF-001.1.3, RN-01 |
| Depende de | TAREFA-02 |
| Toca banco | Não |
| Tamanho | M |
| Risco | Médio — é uma regra de segurança, não só de dado |

## Objetivo

Fazer o servidor decidir quem é o autor do pedido, a partir do token, e devolver
essa informação na resposta.

Esta é a tarefa em que a regra de segurança aparece: **o cliente da API nunca
escolhe o autor**. Se o identificador do usuário viesse no corpo da requisição,
qualquer vendedor lançaria vendas no nome de outro.

## Contexto para ler antes

- `Application/UseCases/Pedidos/Commands/CadastrarPedidoUseCase.cs` — o caso de
  uso que você vai alterar. Note que tudo acontece dentro de
  `_unitOfWork.ExecutarEmTransacao`.
- `Application/UseCases/Usuarios/Queries/ObterUsuarioAutenticadoUseCase.cs` —
  **leia com atenção**. Ele já resolve o usuário do token e expõe dois métodos:
  `Executar()` devolve o DTO, `ObterEntidade()` devolve a entidade `Usuario`.
  É ele que você vai reaproveitar. Não escreva leitura de *claim* nova.
- `Program.cs`, bloco `OnTokenValidated` — entenda que um token de usuário
  inativo ou com versão antiga já é rejeitado antes de chegar no controller.

## Passo a passo

### 1. Injete `ObterUsuarioAutenticadoUseCase` no `CadastrarPedidoUseCase`

Adicione ao construtor e ao campo. O registro é automático (Scrutor pega tudo
que termina em `UseCase`), então não mexa no `Program.cs`.

### 2. Obtenha o usuário e passe ao construtor do pedido

Dentro da transação, antes de montar os itens:

```csharp
var usuario = await _obterUsuarioAutenticado.ObterEntidade();
```

E na construção:

```csharp
var pedido = new Pedido(dto.ClienteId, itens, usuario.Id);
```

### 3. Torne o parâmetro obrigatório no caminho de criação

Na TAREFA-02 o `usuarioId` entrou como parâmetro opcional para não quebrar a
compilação. Agora avalie remover o valor padrão: se algum teste antigo constrói
`Pedido` sem autor, ajuste o teste em vez de manter a brecha aberta.

> Se preferir manter o padrão para não reescrever os testes existentes, tudo
> bem — mas então escreva um teste que garanta que **o caso de uso** sempre
> preenche o autor. A garantia tem que existir em algum lugar.

### 4. Confirme que o DTO de entrada não tem o campo

Abra `Application/DTO/Pedidos/CriarPedidoRequestDTO.cs`. Ele tem `ClienteId` e
`Itens`. **Não adicione `UsuarioId`.** Este passo é uma conferência, não uma
alteração — e é o coração da tarefa.

### 5. Exponha na resposta

Em `Application/DTO/Pedidos/PedidoResponseDTO.cs`:

```csharp
public int? UsuarioId { get; set; }
public string? UsuarioNome { get; set; }
```

O nome não está na entidade `Pedido` — ela só tem o identificador. Você tem duas
saídas:

1. Preencher `UsuarioNome` no caso de uso, a partir do `usuario` que já
   carregou. Simples, e resolve o caminho de criação.
2. Carregar o nome no repositório em uma projeção, para os caminhos de consulta.

Para **esta** tarefa, a opção 1 basta: o caso de uso já tem o objeto `Usuario` em
mãos. A consulta é problema da TAREFA-05 e da TAREFA-06, que fazem projeção
própria.

### 6. Ajuste o `PedidoProfile` se necessário

`Application/Mapper/PedidoProfile.cs` mapeia `Pedido` para `PedidoResponseDTO`.
`UsuarioId` casa por convenção. `UsuarioNome` não existe na entidade — se o
AutoMapper reclamar de membro não mapeado, use `.ForMember(..., opt =>
opt.Ignore())` e atribua o nome depois do mapeamento, no caso de uso.

## Armadilhas

- **Não leia a *claim* direto no controller.** A camada de aplicação é quem
  conhece o usuário autenticado (RNF-04). O controller só orquestra.
- **Não confie em um `UsuarioId` que venha do corpo.** Se durante a
  implementação você sentir vontade de aceitar o campo "só para teste ficar mais
  fácil", pare: é exatamente o buraco que a tarefa existe para fechar.
- `ObterEntidade()` lança `UnauthorizedException` quando não consegue resolver o
  usuário. Deixe a exceção subir — o middleware traduz para 401. Não capture.
- Cuidado com a ordem: obter o usuário **dentro** da transação mantém tudo em um
  escopo só e não custa nada, já que é leitura por chave primária.

## Testes a escrever

Arquivo: `GestaoPedidosTests/Application/UseCases/Pedidos/Commands/CadastrarPedidoUseCaseTests.cs`

| Cenário | Esperado |
|---|---|
| Usuário autenticado com id 7 cria pedido válido | Pedido persistido tem `UsuarioId` 7 |
| Resposta do caso de uso | Traz `usuarioId` 7 e `usuarioNome` preenchido |
| Requisição sem usuário resolvível | `UnauthorizedException`, e nenhum pedido é salvo |

Como `ObterUsuarioAutenticadoUseCase` é uma classe concreta e não uma interface,
o Moq precisa que os métodos usados sejam `virtual`, **ou** você extrai uma
interface pequena para ele. Extrair a interface é a saída mais limpa e não
quebra nada — avalie no momento da implementação e registre a escolha no commit.

Monte o teste no padrão MSTest já usado em `TransicaoPedidoUseCaseTests.cs`:
`[TestInitialize]` criando os mocks e configurando
`ExecutarEmTransacao` para executar a operação direto.

## Critérios de aceite

- [ ] `CriarPedidoRequestDTO` **não** possui campo de usuário.
- [ ] Pedido criado registra o identificador do usuário autenticado.
- [ ] Enviar um identificador de usuário extra no corpo do JSON não tem efeito
      algum sobre o autor gravado.
- [ ] Resposta traz `usuarioId` e `usuarioNome`.
- [ ] Os três cenários da tabela têm teste automatizado.

## Como validar manualmente

Autentique-se como um vendedor, crie um pedido e confira a resposta:

```bash
curl -X POST https://localhost:5001/api/Pedido \
  -H "Authorization: Bearer SEU_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"clienteId":1,"usuarioId":999,"itens":[{"produtoId":1,"quantidade":2}]}'
```

O `usuarioId: 999` no corpo tem que ser **ignorado**. A resposta precisa trazer
o seu próprio identificador.
