using GestaoPedidos.Domain.Entities;

namespace GestaoPedidos.Domain.Abstractions
{
    public interface IProdutoRepository
    {
        public Task<List<Produto>> Listar();
        public Task<Produto> Cadastrar(Produto produto);
        public Task<Produto> ObterPorId(int? id);
        public Task AdicionarEmLote(List<Produto> produtos);
        public Task Atualizar(Produto produto);
    }
}
