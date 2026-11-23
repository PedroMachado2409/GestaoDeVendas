import { http, URL_API } from '@/lib/http'

import type {
  AjusteEstoque,
  Pessoa,
  PessoaCreate,
  PessoaUpdate,
  CriarPedido,
  LoginRequest,
  LoginResponse,
  MovimentacaoEstoque,
  Papel,
  Pedido,
  Produto,
  ProdutoCreate,
  ProdutoUpdate,
  Usuario,
  UsuarioCreate,
  UsuarioUpdate,
  UsuarioUpdateSenha,
} from './tipos'

export const pessoasApi = {
  listar: (sinal?: AbortSignal) => http.get<Pessoa[]>('/api/Pessoa', { sinal }),
  obter: (id: number) => http.get<Pessoa>(`/api/Pessoa/${id}`),
  criar: (dados: PessoaCreate) => http.post<Pessoa>('/api/Pessoa', dados),
  atualizar: ({ id, ...dados }: PessoaUpdate) =>
    http.put<Pessoa>(`/api/Pessoa/${id}`, { id, ...dados }),
  ativar: (id: number) => http.put<void>(`/api/Pessoa/${id}/ativar`),
  inativar: (id: number) => http.put<void>(`/api/Pessoa/${id}/inativar`),
}

export const produtosApi = {
  listar: (sinal?: AbortSignal) => http.get<Produto[]>('/api/Produto', { sinal }),
  obter: (id: number) => http.get<Produto>(`/api/Produto/${id}`),
  criar: (dados: ProdutoCreate) => http.post<Produto>('/api/Produto', dados),
  atualizar: ({ id, ...dados }: ProdutoUpdate) =>
    http.put<Produto>(`/api/Produto/${id}`, { id, ...dados }),
  ativar: (id: number) => http.put<void>(`/api/Produto/${id}/ativar`),
  inativar: (id: number) => http.put<void>(`/api/Produto/${id}/inativar`),
  /** Admin. Grava uma movimentação AjusteManual e devolve o produto atualizado. */
  ajustarEstoque: (id: number, dados: AjusteEstoque) =>
    http.post<Produto>(`/api/Produto/${id}/ajustarEstoque`, dados),
}

export const movimentacoesApi = {
  /** Admin. Da mais recente para a mais antiga. */
  listarPorProduto: (produtoId: number, sinal?: AbortSignal) =>
    http.get<MovimentacaoEstoque[]>(`/api/MovimentacaoEstoque/produto/${produtoId}`, { sinal }),
}

export const pedidosApi = {
  /** Todos os pedidos, com cliente e vendedor carregados. Sem paginação. */
  listar: (sinal?: AbortSignal) => http.get<Pedido[]>('/api/Pedido', { sinal }),
  obter: (id: number) => http.get<Pedido>(`/api/Pedido/${id}`),
  criar: (dados: CriarPedido) => http.post<Pedido>('/api/Pedido', dados),
  finalizar: (id: number) => http.put<Pedido>(`/api/Pedido/${id}/finalizar`),
  cancelar: (id: number) => http.put<Pedido>(`/api/Pedido/${id}/cancelar`),
}

export const usuariosApi = {
  autenticar: (dados: LoginRequest) =>
    http.post<LoginResponse>('/api/Usuario/autenticar', dados, { anonimo: true }),
  registrar: (dados: UsuarioCreate) =>
    http.post<Usuario>('/api/Usuario/registrar', dados),
  autenticado: (sinal?: AbortSignal) => http.get<Usuario>('/api/Usuario/autenticado', { sinal }),
  listar: (sinal?: AbortSignal) => http.get<Usuario[]>('/api/Usuario', { sinal }),
  atualizarPerfil: (dados: UsuarioUpdate) => http.put<Usuario>('/api/Usuario/perfil', dados),
  atualizarSenha: (dados: UsuarioUpdateSenha) => http.put<Usuario>('/api/Usuario/senha', dados),
  alterarPapel: (id: number, role: Papel) => http.put<Usuario>(`/api/Usuario/${id}/role`, { role }),
  ativar: (id: number) => http.put<void>(`/api/Usuario/${id}/ativar`),
  inativar: (id: number) => http.put<void>(`/api/Usuario/${id}/inativar`),
}

export const sistemaApi = {
  /** GET /health é anônimo e responde texto puro ("Healthy"). */
  saude: async (sinal?: AbortSignal) => {
    const resposta = await fetch(`${URL_API}/health`, { signal: sinal })
    return { ok: resposta.ok, texto: (await resposta.text()).trim() }
  },
}
