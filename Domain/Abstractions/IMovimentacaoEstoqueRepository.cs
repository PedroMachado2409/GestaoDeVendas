using GestaoPedidos.Domain.Entities;

namespace GestaoPedidos.Domain.Abstractions
{
    public interface IMovimentacaoEstoqueRepository
    {
        public Task CadastrarMovimentacao(MovimentacaoEstoque movimentacaoEstoque);
        public Task<List<MovimentacaoEstoque>>ListarPorProduto(int produtoId);
        public Task<List<MovimentacaoEstoque>>ListarPorOrigem(int origemId);
        public Task<MovimentacaoEstoque?> ObterMovimentacaoPorId(int id);
        public Task DeletarMovimentacao(int id);
    }
}
