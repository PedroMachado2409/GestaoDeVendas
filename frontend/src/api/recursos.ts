import { http, URL_API } from '@/lib/http'

import type {
  AjusteEstoque,
  Cliente,
  ClienteCreate,
  ClienteUpdate,
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

export const clientesApi = {
  listar: (sinal?: AbortSignal) => http.get<Cliente[]>('/api/Cliente', { sinal }),
  obter: (id: number) => http.get<Cliente>(`/api/Cliente/${id}`),
  criar: (dados: ClienteCreate) => http.post<Cliente>('/api/Cliente', dados),
  atualizar: ({ id, ...dados }: ClienteUpdate) =>
    http.put<Cliente>(`/api/Cliente/${id}`, { id, ...dados }),
  ativar: (id: number) => http.put<void>(`/api/Cliente/${id}/ativar`),
  inativar: (id: number) => http.put<void>(`/api/Cliente/${id}/inativar`),
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
    http.post<Usuario>('/api/Usuario/registrar', dados, { anonimo: true }),
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
