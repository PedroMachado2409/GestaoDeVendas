# TAREFA-06 — Visibilidade no `GET /api/Pedido/{id}` e correção do código de retorno

| | |
|---|---|
| Requisito | RF-001.4.2, RF-001.7, RN-03, RN-04, decisão D2 |
| Depende de | TAREFA-03 |
| Toca banco | Não |
| Tamanho | P |
| Risco | Baixo, mas é regra de segurança — revise com cuidado |

## Objetivo

Duas coisas pequenas e relacionadas:

1. Vendedor não acessa pedido de outro vendedor.
2. Pedido inexistente para de devolver **400** e passa a devolver **404**.

## O defeito que você vai corrigir

`Application/UseCases/Pedidos/Queries/ObterPedidoPorIdUseCase.cs`:

```csharp
var pedido = await _repository.ObterPorId(id);
if (pedido == null)
{
    throw new BadRequestException(PedidosExceptions.Pedido_NaoEncontrado);
}
```

A exceção lançada é `BadRequestException`, que o `ExceptionMiddleware` traduz
para **400 Requisição inválida**. Mas a requisição não é inválida — ela está
perfeitamente bem formada, o recurso é que não existe. A semântica correta é
**404**, e todos os outros casos de uso de pedido já usam `NotFoundException`
para a mesma situação. É uma inconsistência interna.

## Por que 404 e não 403 para pedido de outro vendedor

Decisão D2 do RF-001. Se um vendedor pedisse o pedido 55 e recebesse **403**,
ele aprenderia que o pedido 55 existe e é de outra pessoa. Recebendo **404**,
ele não distingue "não existe" de "não é seu". A resposta precisa ser
indistinguível nos dois casos — mesmo código, mesmo corpo, mesma mensagem.

Isso é enumeração de recursos, e é uma falha de verdade, não preciosismo.

## Passo a passo

### 1. Corrija a exceção

Troque `BadRequestException` por `NotFoundException`. A mensagem em
`PedidosExceptions.resx` continua a mesma.

### 2. Aplique a regra de visibilidade

No `ObterPedidoPorIdUseCase`, depois de carregar o pedido:

```csharp
var usuario = await _obterUsuarioAutenticado.ObterEntidade();

if (usuario.Role != UserRole.Admin && pedido.UsuarioId != usuario.Id)
{
    throw new NotFoundException(PedidosExceptions.Pedido_NaoEncontrado);
}
```

Repare que a mesma exceção, com a mesma mensagem, cobre os três casos: não
existe, é de outro, ou é histórico sem autor (`UsuarioId` nulo, que nunca casa
com `usuario.Id` — RN-04, resolvido de graça pela comparação).

### 3. Extraia, se for repetir

A TAREFA-07 e a TAREFA-08 precisam exatamente desta verificação. Duas
possibilidades:

- deixar duplicado agora e extrair quando a terceira aparecer;
- extrair já, para um método compartilhado.

Sugestão: **extraia na TAREFA-07**, quando o segundo uso existir de fato. Extrair
uma abstração a partir de um caso só costuma produzir a abstração errada. Mas
deixe um comentário indicando a intenção, para o revisor saber que a duplicação
é temporária e consciente.

## Armadilhas

- **Não** monte mensagens diferentes para "não existe" e "não é seu". Mensagem
  diferente vaza a mesma informação que o 403 vazaria. O corpo da resposta tem
  que ser idêntico.
- **Não** registre em log de nível alto a tentativa de acesso a pedido alheio de
  forma que exponha o dado; um `LogWarning` do middleware já basta.
- Verifique se algum teste existente afirma que pedido inexistente devolve 400.
  Se afirmar, **o teste é que estava errado** — ele documentava o defeito.
  Atualize-o e mencione isso no commit.

## Testes a escrever

| Cenário | Esperado |
|---|---|
| Vendedor consulta pedido próprio | 200, com o pedido |
| Vendedor consulta pedido de outro | `NotFoundException` → 404 |
| Vendedor consulta pedido inexistente | `NotFoundException` → 404 |
| Vendedor consulta pedido histórico sem autor | 404 |
| Admin consulta pedido de qualquer vendedor | 200 |
| Admin consulta pedido histórico sem autor | 200 |
| Corpo das respostas 404 dos casos 2 e 3 | **Idêntico** |

O último cenário é o que prova a decisão D2. Compare título, detalhe e código.

## Critérios de aceite

- [ ] Pedido inexistente devolve 404, não 400.
- [ ] Vendedor recebe 404 ao pedir pedido de outro.
- [ ] As respostas de "não existe" e "não é seu" são indistinguíveis.
- [ ] Admin continua acessando tudo, inclusive históricos sem autor.
- [ ] Todos os cenários da tabela têm teste.

## Como validar

Crie um pedido com o vendedor A, anote o identificador, autentique-se como
vendedor B e consulte:

```bash
curl -i https://localhost:5001/api/Pedido/55 -H "Authorization: Bearer TOKEN_B"
curl -i https://localhost:5001/api/Pedido/999999 -H "Authorization: Bearer TOKEN_B"
```

As duas respostas precisam ser byte a byte iguais, exceto pelo `traceId` e pelo
`instance`.
