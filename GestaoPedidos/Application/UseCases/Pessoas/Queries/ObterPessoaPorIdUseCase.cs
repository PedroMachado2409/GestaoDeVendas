using AutoMapper;
using GestaoPedidos.Application.DTO.Pessoas;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Pessoas;

namespace GestaoPedidos.Application.UseCases.Pessoas.Queries
{
    public class ObterPessoaPorIdUseCase
    {
        private readonly IPessoaRepository _repository;
        private readonly IMapper _mapper;

        public ObterPessoaPorIdUseCase(IPessoaRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PessoaResponseDTO> Executar(int id)
        {
            var pessoa = await _repository.ObterPorId(id)
                ?? throw new NotFoundException(PessoasExceptions.Pessoa_NaoEncontrado);

            return _mapper.Map<PessoaResponseDTO>(pessoa);
        }
    }
}
