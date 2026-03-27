

using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Pedidos;

namespace GestaoPedidos.Domain.Entities.Pedidos
{
    public class PedidoItem
    {
        public int Id { get; private set; }
        public int PedidoId { get; private set; }
        public int ProdutoId { get; private set; }
        public decimal Preco {  get; private set; }
        public int Quantidade { get; private set; }

        public decimal SubTotal => Preco * Quantidade;

        protected PedidoItem () { }

        public PedidoItem(int produtoId, decimal preco, int quantidade)
        {
            if (quantidade <= 0)
                throw new BadRequestException(PedidosExceptions.Pedido_QuantidadeInvalida);
            ProdutoId = produtoId;
            Preco = preco;
            Quantidade = quantidade;
        }

        public void AlterarQuantidade(int quantidade)
        {
            if (quantidade <= 0)
                throw new BadRequestException(PedidosExceptions.Pedido_QuantidadeInvalida);
            Quantidade = quantidade;
        }
    }
}
