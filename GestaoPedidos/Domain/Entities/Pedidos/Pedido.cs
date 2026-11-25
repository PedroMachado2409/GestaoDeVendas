using GestaoPedidos.Domain.Enum;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Pedidos;

namespace GestaoPedidos.Domain.Entities.Pedidos
{
    public class Pedido
    {
        public int Id { get; private set; }
        public int ClienteId { get; private set; }
        public Pessoa? Cliente { get; private set; }
        public StatusPedido Status { get; private set; }
        public TipoMovimentacao TipoMovimentacao { get; private set; }
        public DateTime DataCadastro { get; private set; } = DateTime.UtcNow;
        public decimal ValorTotal { get; private set; }
        public int UsuarioId { get; private set; }
        public Usuario? Usuario { get; private set; }

        private readonly List<PedidoItem> _itens = new();
        public IReadOnlyCollection<PedidoItem> Itens => _itens.AsReadOnly();

        protected Pedido() { }

        public Pedido(int clienteId, List<PedidoItem> itens, int usuarioId, decimal valorTotal, TipoMovimentacao tipoMovimentacao)
        {
            if (itens is null || itens.Count == 0 || itens.Any(item => item is null))
            {
                throw new BadRequestException(PedidosExceptions.Pedido_ItemObrigatório);
            }

            if (!System.Enum.IsDefined(tipoMovimentacao))
            {
                throw new BadRequestException("Tipo de movimentação inválido.");
            }

            if (itens.Select(item => item.ProdutoId).Distinct().Count() != itens.Count)
            {
                throw new BadRequestException("Não é permitido repetir o mesmo produto no pedido.");
            }

            var totalCalculado = itens.Sum(item => item.SubTotal);
            if (valorTotal != totalCalculado)
            {
                throw new BadRequestException("O valor total deve corresponder à soma dos itens do pedido.");
            }

            ClienteId = clienteId;
            UsuarioId = usuarioId;
            ValorTotal = totalCalculado;
            TipoMovimentacao = tipoMovimentacao;
            Status = StatusPedido.Aberto;
            _itens = itens.Select(item => new PedidoItem(item.ProdutoId, item.Preco, item.Quantidade)).ToList();
            foreach (var item in _itens)
            {
                item.VincularAoPedido(this);
            }
        }

        public void AtualizarItem(int produtoId, int novaQuantidade)
        {
            if (Status != StatusPedido.Aberto)
            {
                throw new BadRequestException(PedidosExceptions.Pedido_NaoPodeSerAlterado);
            }

            var item = _itens.FirstOrDefault(i => i.ProdutoId == produtoId);

            if (item is null)
            {
                throw new NotFoundException(PedidosExceptions.Pedido_NaoEncontrado);
            }

            if (novaQuantidade <= 0)
            {
                if (_itens.Count == 1)
                {
                    throw new BadRequestException(PedidosExceptions.Pedido_ItemObrigatório);
                }

                _itens.Remove(item);
            }
            else
            {
                item.DefinirQuantidade(novaQuantidade);
            }

            ValorTotal = _itens.Sum(i => i.SubTotal);
        }

        public void Finalizar()
        {
            if (Status == StatusPedido.Cancelado)
            {
                throw new BadRequestException(PedidosExceptions.Pedido_Cancelado);
            }

            if (Status == StatusPedido.Finalizado)
            {
                throw new BadRequestException(PedidosExceptions.Pedido_Finalizado);
            }

            Status = StatusPedido.Finalizado;
        }

        public void Cancelar()
        {
            if (Status != StatusPedido.Aberto)
            {
                throw new ConflictException(PedidosExceptions.Pedido_NaoPodeCancelar);
            }

            Status = StatusPedido.Cancelado;
        }

    }
}
