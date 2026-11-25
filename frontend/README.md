# Frontend Gestão de Vendas

React, TypeScript e Vite. Execute na pasta `frontend`:

```sh
npm ci
npm run dev
```

O servidor usa `http://localhost:5173`. A API padrão é `http://localhost:5143`;
para alterar, defina `VITE_API_URL` em `.env.local` e reinicie o Vite.

## Integração com a API

- Pessoas: `GET /api/Pessoa?pagina=1&tamanhoPagina=100`, com uma lista simples na resposta.
- O frontend busca as páginas seguintes até completar a lista. Isso mantém a busca,
  os filtros por categoria e situação e os seletores dos pedidos abrangendo todos os registros.
  As tabelas exibem 20 registros por página, depois de filtrar e ordenar.
- Produtos usam a mesma leitura, compatível também com a API que ainda devolve a lista inteira.
- Pedidos enviam `pessoaId` e leem `pessoaNome`. `Saida` representa venda e `Entrada`, compra.
- Compras, estoque e usuários exigem o papel Admin no frontend; a API deve validar as permissões.
- Alterar perfil ou senha revoga o token: o usuário retorna ao login após salvar.
- Financeiro usa dados de demonstração locais enquanto sua integração com a API não estiver pronta.

Para grandes volumes de registros, o próximo passo é implementar busca e filtros na API
e retornar a quantidade total, permitindo carregar somente a página em exibição.

## Verificação

```sh
npm test
npm run lint
npm run build
```

Os testes cobrem paginação, cancelamento de consultas, isolamento entre sessões,
contratos de requisição, tratamento de erros, validação de valores e permissões.
