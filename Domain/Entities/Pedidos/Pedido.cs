using GestaoPedidos.Domain.Enum;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Pedidos;

namespace GestaoPedidos.Domain.Entities.Pedidos
{
    public class Pedido
    {
        public int Id { get; private set; }
        public int ClienteId { get; private set; }
        public StatusPedido Status {  get; private set; }
        public DateTime DataCadastro { get; private set; } = DateTime.UtcNow;
        public decimal ValorTotal => _itens.Sum(i => i.SubTotal);

        private readonly List<PedidoItem> _itens = new();
        public IReadOnlyCollection<PedidoItem> Itens => _itens.AsReadOnly();

        protected Pedido () { }
        
        public Pedido (int clienteId, List<PedidoItem> itens)
        {
            if (itens is null || !itens.Any())
                throw new BadRequestException(PedidosExceptions.Pedido_ItemObrigatório);

            ClienteId = clienteId;
            Status = StatusPedido.Aberto;
            _itens = itens;
        }
        public void AdicionarItem(PedidoItem item)
        {
            if (Status != StatusPedido.Aberto)
                throw new BadRequestException(PedidosExceptions.Pedido_NaoPodeSerAlterado);

            _itens.Add(item);
        }

        public void AtualizarItem(int produtoId, int novaQuantidade)
        {
            if (Status != StatusPedido.Aberto)
                throw new BadRequestException(PedidosExceptions.Pedido_NaoPodeSerAlterado);

            var item = _itens.FirstOrDefault(i => i.ProdutoId == produtoId);


            if (novaQuantidade <= 0)
            {
                _itens.Remove(item);

                if (!_itens.Any())
                    throw new BadRequestException(PedidosExceptions.Pedido_ItemObrigatório);

                return;
            }

            item.AlterarQuantidade(novaQuantidade);
        }
        public void Finalizar()
        {
            if (Status == StatusPedido.Cancelado)
                throw new BadRequestException(PedidosExceptions.Pedido_Cancelado);
            if (Status == StatusPedido.Finalizado)
                throw new BadRequestException(PedidosExceptions.Pedido_Finalizado);

            Status = StatusPedido.Finalizado;
        }

        public void Cancelar()
        {
            if (Status == StatusPedido.Cancelado)
                throw new BadRequestException(PedidosExceptions.Pedido_NaoPodeCancelar); 


            Status = StatusPedido.Cancelado;
        }


    }
}
