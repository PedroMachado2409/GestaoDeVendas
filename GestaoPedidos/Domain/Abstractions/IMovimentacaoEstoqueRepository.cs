using GestaoPedidos.Domain.Entities;

namespace GestaoPedidos.Domain.Abstractions
{
    /// <summary>
    /// Trilha de auditoria: só escreve e lê. Não há alteração nem exclusão —
    /// um histórico que pode ser apagado deixa de explicar o estoque atual.
    /// Erro de lançamento se corrige com movimentação em sentido contrário.
    /// </summary>
    public interface IMovimentacaoEstoqueRepository
    {
        Task Cadastrar(MovimentacaoEstoque movimentacaoEstoque);
        Task<List<MovimentacaoEstoque>> ListarPorProduto(int produtoId);
    }
}
