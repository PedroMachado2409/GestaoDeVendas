using AutoMapper;
using GestaoPedidos.Application.DTO.Produtos;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Exceptions;

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

        public async Task<List<ProdutoResponseDTO>> Executar(int pagina, int tamanhoPagina)
        {
            if (pagina < 1)
            {
                throw new BadRequestException(PaginaExceptions.Pagina_PaginaInvalida);
            }
            else if (tamanhoPagina is < 1 or > 100 || (long)(pagina - 1) * tamanhoPagina > int.MaxValue)
            {
                throw new BadRequestException(PaginaExceptions.Pagina_TamanhoInvalido);
            }

            var produtos = await _repository.Listar(pagina, tamanhoPagina);
            return _mapper.Map<List<ProdutoResponseDTO>>(produtos);
        }

    }
}
