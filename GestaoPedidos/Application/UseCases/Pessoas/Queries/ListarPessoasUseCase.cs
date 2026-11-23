using AutoMapper;
using GestaoPedidos.Application.DTO.Pessoas;
using GestaoPedidos.Domain.Abstractions;

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

        public async Task<List<PessoaResponseDTO>> Executar()
        {
            var pessoas = await _repository.Listar();
            return _mapper.Map<List<PessoaResponseDTO>>(pessoas);
        }
    }
}
