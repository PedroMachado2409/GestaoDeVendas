using GestaoPedidos.Domain.Entities;

namespace GestaoPedidos.Domain.Abstractions
{
    public interface IProdutoRepository
    {
        Task<List<Produto>> Listar();
        Task<Produto> Cadastrar(Produto produto);
        Task<Produto?> ObterPorId(int id);
        Task<List<Produto>> ObterPorIds(IEnumerable<int> ids);
        Task Atualizar(Produto produto);
        Task Atualizar(IEnumerable<Produto> produtos);
    }
}
