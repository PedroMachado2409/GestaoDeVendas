using GestaoPedidos.Domain.Entities.Pedidos;

namespace GestaoPedidos.Domain.Abstractions
{
    public interface IEmailService
    {
        Task EnviarPedidoCriadoAsync(string email, string nomeCliente, string numeroPedido, decimal ValorTotal, IEnumerable<PedidoItem> itens);
    }
}
