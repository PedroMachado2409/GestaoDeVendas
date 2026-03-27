using AutoMapper;
using GestaoPedidos.Application.DTO.Produtos;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Exceptions.Produtos;

namespace GestaoPedidos.Application.UseCases.Produtos.Queries
{
    public class ObterProdutoPorIdUseCase
    {
        private readonly IMapper _mapper;
        private readonly IProdutoRepository _repository;

        public ObterProdutoPorIdUseCase (IMapper mapper, IProdutoRepository repository)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public async Task <ProdutoResponseDTO> Executar(int id)
        {
            var produto = await _repository.ObterPorId(id);
            if (produto == null)
                throw new BadHttpRequestException(ProdutoExceptions.Produto_NaoEncontrado);
            return _mapper.Map<ProdutoResponseDTO>(produto);
        }

    }
}
