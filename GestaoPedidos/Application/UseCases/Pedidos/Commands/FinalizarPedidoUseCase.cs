using AutoMapper;
using GestaoPedidos.Application.DTO.Pedidos;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Domain.Enum;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Pedidos;
using GestaoPedidos.Domain.Exceptions.Produtos;

namespace GestaoPedidos.Application.UseCases.Pedidos.Commands
{
    public class FinalizarPedidoUseCase
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IProdutoRepository _produtoRepository;
        private readonly IMovimentacaoEstoqueRepository _movimentacaoEstoqueRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FinalizarPedidoUseCase(
            IPedidoRepository pedidoRepository,
            IProdutoRepository produtoRepository,
            IMovimentacaoEstoqueRepository movimentacaoEstoqueRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _pedidoRepository = pedidoRepository;
            _produtoRepository = produtoRepository;
            _movimentacaoEstoqueRepository = movimentacaoEstoqueRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public Task<PedidoResponseDTO> Executar(int pedidoId)
            => _unitOfWork.ExecutarEmTransacao(async () =>
            {
                var pedido = await _pedidoRepository.ObterPorId(pedidoId)
                    ?? throw new NotFoundException(PedidosExceptions.Pedido_NaoEncontrado);

                if (pedido.Status == StatusPedido.Finalizado)
                {
                    return _mapper.Map<PedidoResponseDTO>(pedido);
                }

                if (pedido.Status != StatusPedido.Aberto)
                {
                    throw new ConflictException(PedidosExceptions.Pedido_Cancelado);
                }

                var produtos = await _produtoRepository.ObterPorIds(
                    pedido.Itens.Select(i => i.ProdutoId));
                var produtosPorId = produtos.ToDictionary(p => p.Id);

                var ehVenda = pedido.TipoMovimentacao == TipoMovimentacao.Saida;
                var origem = ehVenda
                    ? OrigemMovimentacao.PedidoDeVenda
                    : OrigemMovimentacao.PedidoDeCompra;

                foreach (var item in pedido.Itens)
                {
                    if (!produtosPorId.TryGetValue(item.ProdutoId, out var produto))
                    {
                        throw new NotFoundException(ProdutoExceptions.Produto_NaoEncontrado);
                    }

                    if (ehVenda)
                    {
                        produto.ConfirmarReserva(item.Quantidade);
                    }
                    else
                    {
                        produto.EfetivarCompra(item.Quantidade);
                    }

                    // Montada depois de mover o estoque: se a movimentação do
                    // produto falhar, nenhum registro de trilha é criado.
                    var movimentacao = new MovimentacaoEstoque(
                        produto.Id,
                        produto.Nome,
                        item.Quantidade,
                        pedido.Id,
                        origem,
                        pedido.TipoMovimentacao,
                        $"Movimentação referente ao pedido {pedido.Id}.");

                    await _movimentacaoEstoqueRepository.Cadastrar(movimentacao);
                }

                pedido.Finalizar();
                await _pedidoRepository.Atualizar(pedido);
                await _unitOfWork.SalvarAlteracoes();

                return _mapper.Map<PedidoResponseDTO>(pedido);
            });
    }
}
