using AutoMapper;
using GestaoPedidos.Application.DTO.Pessoas;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Exceptions;

namespace GestaoPedidos.Application.UseCases.Pessoas.Queries
{
    public class ListarPessoasUseCase
    {
        private readonly IPessoaRepository _repository;
        private readonly IMapper _mapper;

        public ListarPessoasUseCase(IPessoaRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<PessoaResponseDTO>> Executar(int pagina, int tamanhoPagina)
        {
            if (pagina < 1)
            {
                throw new BadRequestException(PaginaExceptions.Pagina_PaginaInvalida);
            }
            else if (tamanhoPagina is < 1 or > 100 || (long)(pagina - 1) * tamanhoPagina > int.MaxValue)
            {
                throw new BadRequestException(PaginaExceptions.Pagina_TamanhoInvalido);
            }

            var pessoas = await _repository.Listar(pagina, tamanhoPagina);
            return _mapper.Map<List<PessoaResponseDTO>>(pessoas);
        }
    }
}
