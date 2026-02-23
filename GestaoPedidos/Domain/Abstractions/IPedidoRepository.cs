using GestaoPedidos.Domain.Entities.Pedidos;

namespace GestaoPedidos.Domain.Abstractions
{
    public interface IPedidoRepository
    {
        Task Cadastrar(Pedido pedido);
        //Task <List<Pedido>> Listar();
        Task<Pedido?> ObterPorId(int id);
        Task Atualizar (Pedido pedido);
    }
}
