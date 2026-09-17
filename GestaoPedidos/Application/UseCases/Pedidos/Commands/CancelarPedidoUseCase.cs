using AutoMapper;
using GestaoPedidos.Application.DTO.Pedidos;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Enum;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Pedidos;
using GestaoPedidos.Domain.Exceptions.Produtos;

namespace GestaoPedidos.Application.UseCases.Pedidos.Commands
{
    public class CancelarPedidoUseCase
    {
        private readonly IMapper _mapper;
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IProdutoRepository _produtoRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CancelarPedidoUseCase(
            IMapper mapper,
            IPedidoRepository pedidoRepository,
            IProdutoRepository produtoRepository,
            IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _pedidoRepository = pedidoRepository;
            _produtoRepository = produtoRepository;
            _unitOfWork = unitOfWork;
        }

        public Task<PedidoResponseDTO> Executar(int pedidoId)
            => _unitOfWork.ExecutarEmTransacao(async () =>
            {
                var pedido = await _pedidoRepository.ObterPorId(pedidoId)
                    ?? throw new NotFoundException(PedidosExceptions.Pedido_NaoEncontrado);

                if (pedido.Status == StatusPedido.Cancelado)
                {
                    return _mapper.Map<PedidoResponseDTO>(pedido);
                }

                if (pedido.Status != StatusPedido.Aberto)
                {
                    throw new ConflictException(PedidosExceptions.Pedido_NaoPodeCancelar);
                }

                var produtos = await _produtoRepository.ObterPorIds(
                    pedido.Itens.Select(i => i.ProdutoId));
                var produtosPorId = produtos.ToDictionary(p => p.Id);

                foreach (var item in pedido.Itens)
                {
                    if (!produtosPorId.TryGetValue(item.ProdutoId, out var produto))
                    {
                        throw new NotFoundException(ProdutoExceptions.Produto_NaoEncontrado);
                    }

                    // Venda e compra reservam em campos diferentes do produto:
                    // venda retira do estoque, compra apenas anota a pendência.
                    // Desfazer sempre pela trilha de venda devolveria ao estoque
                    // unidades que nunca saíram dele.
                    if (pedido.TipoMovimentacao == TipoMovimentacao.Saida)
                    {
                        produto.LiberarReserva(item.Quantidade);
                    }
                    else
                    {
                        produto.CancelarCompraPendente(item.Quantidade);
                    }
                }

                pedido.Cancelar();
                await _pedidoRepository.Atualizar(pedido);
                await _unitOfWork.SalvarAlteracoes();

                return _mapper.Map<PedidoResponseDTO>(pedido);
            });
    }
}
