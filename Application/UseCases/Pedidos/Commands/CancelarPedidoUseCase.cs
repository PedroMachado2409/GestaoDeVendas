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
        private readonly IMovimentacaoEstoqueRepository _movimentacaoEstoqueRepository;

        public CancelarPedidoUseCase (IMapper mapper, IPedidoRepository repository, 
            IProdutoRepository produtoRepository, ITitulosRepository titulosRepository, IMovimentacaoEstoqueRepository movimentacao)
        {
            _mapper = mapper;
            _repository = repository;
            _produtoRepository = produtoRepository;
            _titulosRepository = titulosRepository; 
            _movimentacaoEstoqueRepository = movimentacao;  
        }

        public async Task<PedidoResponseDTO> Executar(int pedidoId)
        {
            var pedido = await _repository.ObterPorId(pedidoId)
                ?? throw new BadRequestException(PedidosExceptions.Pedido_NaoEncontrado);

            var pedidoFinalizado = pedido.Status == StatusPedido.Finalizado;

            if (pedidoFinalizado)
            {
                var titulo = await _titulosRepository.ObterTituloPelaOrigem(pedidoId);
                if (titulo is not null)
                    await _titulosRepository.DeletarTitulo(titulo.Id);
                
                var movimentacoes = await _movimentacaoEstoqueRepository.ListarPorOrigem(pedido.Id);

                if (movimentacoes is not null)
                {
                    foreach (var movimentacao in movimentacoes)
                    {
                        if(movimentacao.NomeOrigem == Origem.Pedido)
                        await _movimentacaoEstoqueRepository.DeletarMovimentacao(movimentacao.Id);
                    }
                }
            }
            foreach (var item in pedido.Itens)
            {
                var produto = await _produtoRepository.ObterPorId(item.ProdutoId)
                    ?? throw new NotFoundException(ProdutoExceptions.Produto_NaoEncontrado);

                if (!pedidoFinalizado)
                {
                    produto.CancelarQuantidades(pedido.Tipo, pedido.Status, item.Quantidade);
                }
                else
                {
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
