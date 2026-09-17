# Rubrica de nível — ancorada no GestaoPedidos

Referência para autoavaliação. Cada célula aponta um exemplo real deste
repositório, não um conceito abstrato.

**[v]** = escrito por você · **[c]** = escrito pelo Claude na sessão de 31/08/2026

## Tabela principal

| Dimensão | Júnior | Pleno | Sênior | Seu nível |
|---|---|---|---|---|
| Modelagem de domínio | Entidade anêmica, montada em etapas<br>`MovimentacaoEstoque` original **[v]** | Setters privados, construtor valida<br>`Usuario.AlterarSenha()` **[v]** | Invariante impossível de violar, documentado<br>trilhas no topo de `Produto.cs` **[c]** | **Oscila** |
| Erro e contrato HTTP | `Exception` genérica, status solto | Hierarquia → status, `problem+json` + `traceId` **[v]** | Status pelo que **revela**<br>404 em vez de 403 | **Pleno com vazamento** |
| Concorrência e transação | `SaveChanges` no repositório<br>3 repositórios **[v]** | Unidade de trabalho<br>`UnitOfWork.ExecutarEmTransacao` **[v]** | Atomicidade ≠ concorrência<br>`Produto.Versao` **[v/c]** | **Teto alto, piso baixo** |
| Persistência e migrações | Aceita o gerado<br>`defaultValue: 0`, `DropColumn` do `Versao` **[v]** | Lê e ajusta o arquivo | Migração recusa dado inválido<br>`HardeningSegurancaIntegridade` **[v]** | **Maior descompasso** |
| Segurança | Senha em texto, sem autorização | Hash, papéis, rate limit, `FallbackPolicy` **[v]** | Revogação e enumeração<br>`token_version` **[v]** | **Alto, com 1 furo** |
| Consistência | Duas convenções, código comentado, typo **[v]** | Uma convenção aplicada | Verificada por ferramenta<br>`format --verify-no-changes` **[v]** | **Configurou e não rodava** |
| Teste | Testa o que é fácil<br>`CadastrarPedidoUseCase` sem teste **[v]** | Testa onde está o risco | Testa o modo de falha<br>`Cancelar_Duas_Vezes...` **[v]** | **Instinto sênior, cobertura júnior** |
| Documentação | Ausente ou mentirosa<br>README diz .NET 10, é .NET 8 **[v]** | README com matriz de permissão **[v]** | Comentário que explica o **porquê**<br>`UseEphemeralDataProtectionProvider` **[v]** | **Pleno na intenção, júnior na manutenção** |

## Consolidado

| Nível | Dimensões |
|---|---|
| Sênior demonstrado | Segurança, migração defensiva, `UnitOfWork`, `FluentValidationFilter` |
| Pleno | Arquitetura, tratamento de erro, modelagem (quando há tempo), documentação inicial |
| Júnior | Ler artefato gerado, cobertura de teste, manter consistência, higiene de segredo |

**Padrão:** o nível sênior já foi demonstrado em quatro pontos deste repositório.
O que segura não é conhecimento — é **não voltar**. A primeira passada sai boa;
não existe uma segunda.

Prova: o `Versao` foi apagado por um `DropColumn` gerado automaticamente, num
projeto onde você mesmo escreveu a migração que verifica os dados antes de criar
restrição.

## Hábitos a praticar, por retorno

| # | Hábito | Custo | O que evita |
|---|---|---|---|
| 1 | Ler todo arquivo gerado antes de aceitar — migração acima de tudo | 2 min | Perda do controle de concorrência; 2 colunas históricas zeradas |
| 2 | Tirar segredo do repositório (`user-secrets`) | 10 min | Único item com consequência fora do projeto |
| 3 | `dotnet format --verify-no-changes` antes de commitar | 30 s | Toda a lista da higienização |
| 4 | Escrever primeiro o teste do caminho mais complexo | 1 h | Cobertura invertida |
| 5 | Ao criar operação nova, procurar o par dela | 5 min | O bug de cancelamento de pedido de compra |

Nenhum exige conceito novo. Os cinco já aparecem no seu próprio código — só não
de forma consistente.
