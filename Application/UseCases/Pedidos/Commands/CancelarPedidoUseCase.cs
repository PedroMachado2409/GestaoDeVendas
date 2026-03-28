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
        private readonly IPedidoRepository _repository;
        private readonly IProdutoRepository _produtoRepository;
        private readonly ITitulosRepository _titulosRepository;

        public CancelarPedidoUseCase (IMapper mapper, IPedidoRepository repository, 
            IProdutoRepository produtoRepository, ITitulosRepository titulosRepository)
        {
            _mapper = mapper;
            _repository = repository;
            _produtoRepository = produtoRepository;
            _titulosRepository = titulosRepository; 
        }

        public async Task<PedidoResponseDTO> Executar(int pedidoId)
        {
            var pedido = await _repository.ObterPorId(pedidoId);
            if (pedido == null)
                throw new BadRequestException(PedidosExceptions.Pedido_NaoEncontrado);
            var titulo = await _titulosRepository.ObterTituloPelaOrigem(pedidoId);
            if (titulo != null && pedido.Status == StatusPedido.Finalizado)
                await _titulosRepository.DeletarTitulo(titulo.Id);
            
            foreach (var item in pedido.Itens)
            {
                var produto = await _produtoRepository.ObterPorId(item.ProdutoId);
                if (produto == null)
                    throw new NotFoundException(ProdutoExceptions.Produto_NaoEncontrado);

                if(pedido.Status == StatusPedido.Aberto)
                {
                   produto.CancelarReservaDeQuantidade(item.Quantidade);
                } else {
                    produto.AumentarEstoque(item.Quantidade);
                }
                await _produtoRepository.Atualizar(produto);
            }
            pedido.Cancelar();
            await _repository.Atualizar(pedido);
            
            return _mapper.Map<PedidoResponseDTO>(pedido);
        }



    }
}
