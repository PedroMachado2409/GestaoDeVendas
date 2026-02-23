using AutoMapper;
using GestaoPedidos.Application.DTO.Pedidos;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Domain.Entities.Pedidos;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Pedidos;
using GestaoPedidos.Domain.Exceptions.Produtos;
using GestaoPedidos.Infrastructure.Repositories;

namespace GestaoPedidos.Application.UseCases.Pedidos.Commands
{
    public class FinalizarPedidoUseCase
    {
        private readonly IPedidoRepository _repository;
        private readonly IProdutoRepository _produtoRepository;
        private readonly IMapper _mapper;

        public FinalizarPedidoUseCase (IPedidoRepository repository, IMapper mapper, IProdutoRepository produtoRepository)
        {
            _repository = repository;
            _mapper = mapper;
            _produtoRepository = produtoRepository;
        }

        public async Task<PedidoResponseDTO> Executar(int pedidoId)
        {
            var pedido = await _repository.ObterPorId(pedidoId);
            if(pedido == null)
                throw new BadRequestException(PedidosExceptions.Pedido_NaoEncontrado);
            
            foreach (var item in pedido.Itens)
            {
                var produto = await _produtoRepository.ObterPorId(item.ProdutoId);
                if (produto == null)
                    throw new NotFoundException(ProdutoExceptions.Produto_NaoEncontrado);
                produto.CancelarReservaDeQuantidade(item.Quantidade);
                produto.BaixarEstoque(item.Quantidade);
                await _produtoRepository.Atualizar(produto);
            }
            pedido.Finalizar();
            await _repository.Atualizar(pedido);
            
            return _mapper.Map<PedidoResponseDTO>(pedido);

        }

    }
}
