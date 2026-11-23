using AutoMapper;
using GestaoPedidos.Application.DTO.Pessoas;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Pessoas;

namespace GestaoPedidos.Application.UseCases.Pessoas.Commands
{
    public class AtualizarPessoaUseCase
    {
        private readonly IPessoaRepository _repository;
        private readonly IMapper _mapper;

        public AtualizarPessoaUseCase(IPessoaRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PessoaResponseDTO> Executar(PessoaUpdateDTO dto)
        {
            var pessoa = await _repository.ObterPorId(dto.Id)
                ?? throw new NotFoundException(PessoasExceptions.Pessoa_NaoEncontrado);

            var pessoaComCpfExistente = await _repository.ObterPorCpf(dto.Cpf);
            if (pessoaComCpfExistente != null && pessoaComCpfExistente.Id != dto.Id)
            {
                throw new BadRequestException(PessoasExceptions.Pessoa_CpfExistente);
            }

            var pessoaComEmailExistente = await _repository.ObterPorEmail(dto.Email);
            if (pessoaComEmailExistente != null && pessoaComEmailExistente.Id != dto.Id)
            {
                throw new BadRequestException(PessoasExceptions.Pessoa_EmailExistente);
            }

            pessoa.Atualizar(dto.Nome, dto.Email, dto.Cpf, dto.TipoPessoa);

            await _repository.Atualizar(pessoa);

            return _mapper.Map<PessoaResponseDTO>(pessoa);
        }
    }
}
