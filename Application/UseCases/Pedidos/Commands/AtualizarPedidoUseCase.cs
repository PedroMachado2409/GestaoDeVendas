using AutoMapper;
using GestaoPedidos.Application.DTO.Pedidos;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities.Pedidos;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Pedidos;
using GestaoPedidos.Domain.Exceptions.Produtos;

namespace GestaoPedidos.Application.UseCases.Pedidos.Commands
{
    public class AtualizarPedidoUseCase
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IProdutoRepository _produtoRepository;
        private readonly IMapper _mapper;

        public AtualizarPedidoUseCase(
            IPedidoRepository pedidoRepository,
            IProdutoRepository produtoRepository,
            IMapper mapper)
        {
            _pedidoRepository = pedidoRepository;
            _produtoRepository = produtoRepository;
            _mapper = mapper;
        }

        public async Task<PedidoResponseDTO> Executar(int id, AtualizarPedidoRequestDTO dto)
        {
            if (dto.Itens == null || !dto.Itens.Any())
                throw new BadRequestException(PedidosExceptions.Pedido_ItemObrigatório);

            var pedido = await _pedidoRepository.ObterPorId(dto.Id);
            if (pedido == null)
                throw new NotFoundException(PedidosExceptions.Pedido_NaoEncontrado);

            var itensAtuais = pedido.Itens.ToDictionary(item => item.ProdutoId);
            var itensSolicitados = dto.Itens.ToDictionary(item => item.ProdutoId);

            foreach (var itemDto in dto.Itens)
            {
                var produto = await _produtoRepository.ObterPorId(itemDto.ProdutoId);
                if (produto == null)
                    throw new NotFoundException(ProdutoExceptions.Produto_NaoEncontrado);
                if (!produto.Ativo)
                    throw new BadRequestException(ProdutoExceptions.Produto_Inativo);

                if (itensAtuais.TryGetValue(itemDto.ProdutoId, out var itemAtual))
                {
                    var diferencaQuantidade = itemDto.Quantidade - itemAtual.Quantidade;

                    if (diferencaQuantidade > 0)
                        produto.ReservarQuantidade(diferencaQuantidade);
                    else if (diferencaQuantidade < 0)
                        produto.CancelarReservaDeQuantidade(Math.Abs(diferencaQuantidade));

                    pedido.AtualizarItem(itemDto.ProdutoId, itemDto.Quantidade);
                    await _produtoRepository.Atualizar(produto);
                    continue;
                }

                produto.ReservarQuantidade(itemDto.Quantidade);
                pedido.AdicionarItem(new PedidoItem(produto.Id, produto.Preco, itemDto.Quantidade));
                await _produtoRepository.Atualizar(produto);
            }
            id = dto.Id;
            await _pedidoRepository.Atualizar(pedido);
            return _mapper.Map<PedidoResponseDTO>(pedido);
        }
    }
}