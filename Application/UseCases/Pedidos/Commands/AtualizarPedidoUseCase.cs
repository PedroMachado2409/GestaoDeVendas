
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

            if (dto.Itens.Any(i => i.Quantidade <= 0))
                throw new BadRequestException(PedidosExceptions.Pedido_QuantidadeInvalida);

            if (dto.Itens.GroupBy(i => i.ProdutoId).Any(g => g.Count() > 1))
                throw new BadRequestException("Existem produtos duplicados na atualização do pedido.");

            var pedido = await _pedidoRepository.ObterPorId(id);
            if (pedido == null)
                throw new NotFoundException(PedidosExceptions.Pedido_NaoEncontrado);

            var itensAtuais = pedido.Itens.ToDictionary(item => item.ProdutoId);
            var itensSolicitados = dto.Itens.ToDictionary(item => item.ProdutoId);

            foreach (var itemAtual in pedido.Itens.ToList())
            {
                if (itensSolicitados.ContainsKey(itemAtual.ProdutoId))
                    continue;

                var produtoAtual = await _produtoRepository.ObterPorId(itemAtual.ProdutoId);
                if (produtoAtual == null)
                    throw new NotFoundException(ProdutoExceptions.Produto_NaoEncontrado);

                produtoAtual.CancelarReservaDeQuantidade(itemAtual.Quantidade);
                pedido.AtualizarItem(itemAtual.ProdutoId, 0);
                await _produtoRepository.Atualizar(produtoAtual);
            }

            foreach (var itemSolicitado in dto.Itens)
            {
                var produto = await _produtoRepository.ObterPorId(itemSolicitado.ProdutoId);
                if (produto == null)
                    throw new NotFoundException(ProdutoExceptions.Produto_NaoEncontrado);

                if (!produto.Ativo)
                    throw new BadRequestException(ProdutoExceptions.Produto_Inativo);

                var quantidadeAtual = itensAtuais.TryGetValue(itemSolicitado.ProdutoId, out var itemAtual)
                    ? itemAtual.Quantidade
                    : 0;

                var diferencaQuantidade = itemSolicitado.Quantidade - quantidadeAtual;

                if (diferencaQuantidade > 0)
                    produto.ReservarQuantidade(diferencaQuantidade);
                else if (diferencaQuantidade < 0)
                    produto.CancelarReservaDeQuantidade(Math.Abs(diferencaQuantidade));

                if (itemAtual == null)
                    pedido.AdicionarItem(new PedidoItem(produto.Id, produto.Preco, itemSolicitado.Quantidade));
                else
                    pedido.AtualizarItem(itemSolicitado.ProdutoId, itemSolicitado.Quantidade);

                await _produtoRepository.Atualizar(produto);
            }

            await _pedidoRepository.Atualizar(pedido);
            return _mapper.Map<PedidoResponseDTO>(pedido);
        }
    }
}