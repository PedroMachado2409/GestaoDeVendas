using AutoMapper;
using GestaoPedidos.Application.DTO.MovimentacoesEstoque;
using GestaoPedidos.Domain.Abstractions;

namespace GestaoPedidos.Application.UseCases.MovimentacoesEstoque
{
    public class ListarMovimentacaoUseCase
    {
        private readonly IMovimentacaoEstoqueRepository _movimentacaoEstoqueRepository;
        private readonly IMapper _mapper;

        public ListarMovimentacaoUseCase (IMovimentacaoEstoqueRepository movimentacaoEstoqueRepository, IMapper mapper)
        {
            _movimentacaoEstoqueRepository = movimentacaoEstoqueRepository;
            _mapper = mapper;
        }

        public async Task<List<MovimentacaoResponseDTO>> Executar(int produtoId)
        {
            var movimentacoes = await _movimentacaoEstoqueRepository.ListarPorProduto(produtoId);
            return _mapper.Map<List<MovimentacaoResponseDTO>>(movimentacoes);
        }
    }
}
