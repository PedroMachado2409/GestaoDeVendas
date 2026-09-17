using AutoMapper;
using GestaoPedidos.Application.DTO.MovimentacaoEstoque;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Produtos;

namespace GestaoPedidos.Application.UseCases.MovimentacoesEstoque
{
    public class ListarMovimentacaoEstoqueUseCase
    {
        private readonly IMapper _mapper;
        private readonly IMovimentacaoEstoqueRepository _movimentacaoEstoqueRepository;
        private readonly IProdutoRepository _produtoRepository;

        public ListarMovimentacaoEstoqueUseCase(
            IMovimentacaoEstoqueRepository movimentacaoEstoqueRepository,
            IProdutoRepository produtoRepository,
            IMapper mapper)
        {
            _movimentacaoEstoqueRepository = movimentacaoEstoqueRepository;
            _produtoRepository = produtoRepository;
            _mapper = mapper;
        }

        public async Task<List<MovimentacaoEstoqueResponseDTO>> Executar(int produtoId)
        {
            _ = await _produtoRepository.ObterPorId(produtoId)
                ?? throw new NotFoundException(ProdutoExceptions.Produto_NaoEncontrado);

            var movimentacoes = await _movimentacaoEstoqueRepository.ListarPorProduto(produtoId);

            // Produto sem movimentação é resposta válida: lista vazia, 200.
            return _mapper.Map<List<MovimentacaoEstoqueResponseDTO>>(movimentacoes);
        }
    }
}
