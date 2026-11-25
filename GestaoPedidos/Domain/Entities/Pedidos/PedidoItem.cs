

using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Pedidos;

namespace GestaoPedidos.Domain.Entities.Pedidos
{
    public class PedidoItem
    {
        public int Id { get; private set; }
        public int PedidoId { get; private set; }
        public Pedido? Pedido { get; private set; }
        public int ProdutoId { get; private set; }
        public decimal Preco { get; private set; }
        public int Quantidade { get; private set; }

        public decimal SubTotal => Preco * Quantidade;

        protected PedidoItem() { }

        public PedidoItem(int produtoId, decimal preco, int quantidade)
        {
            if (quantidade <= 0)
            {
                throw new BadRequestException(PedidosExceptions.Pedido_QuantidadeInvalida);
            }

            if (produtoId <= 0 || preco <= 0)
            {
                throw new BadRequestException("O produto e o preço do item devem ser válidos.");
            }

            ProdutoId = produtoId;
            Preco = preco;
            Quantidade = quantidade;
        }

        public void AlterarQuantidade(int quantidade)
        {
            if (Pedido is not null)
            {
                throw new BadRequestException("Altere os itens pelo pedido para preservar o status e o valor total.");
            }

            DefinirQuantidade(quantidade);
        }

        internal void VincularAoPedido(Pedido pedido) => Pedido = pedido;

        internal void DefinirQuantidade(int quantidade)
        {
            if (quantidade <= 0)
            {
                throw new BadRequestException(PedidosExceptions.Pedido_QuantidadeInvalida);
            }

            Quantidade = quantidade;
        }
    }
}
