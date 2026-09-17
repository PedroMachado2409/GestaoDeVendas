using AutoMapper;
using GestaoPedidos.Application.DTO.Produtos;
using GestaoPedidos.Domain.Abstractions;

namespace GestaoPedidos.Application.UseCases.Produtos.Queries
{
    public class ListarProdutoUseCase
    {
        private readonly IMapper _mapper;
        private readonly IProdutoRepository _repository;


        public ListarProdutoUseCase(IMapper mapper, IProdutoRepository repository)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public async Task<List<ProdutoResponseDTO>> Executar()
        {
            var produtos = await _repository.Listar();
            return _mapper.Map<List<ProdutoResponseDTO>>(produtos);
        }

    }
}
