using AutoMapper;
using GestaoPedidos.Application.DTO.Pessoas;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Pessoas;

namespace GestaoPedidos.Application.UseCases.Pessoas.Commands
{
    public class AtualizarPessoaUseCase
    {
        private readonly IPessoaRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AtualizarPessoaUseCase(IPessoaRepository repository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PessoaResponseDTO> Executar(PessoaUpdateDTO dto)
        {
            var pessoa = await _repository.ObterPorId(dto.Id)
                ?? throw new NotFoundException(PessoasExceptions.Pessoa_NaoEncontrado);

            var cpf = Pessoa.NormalizarCpf(dto.Cpf);
            var email = Pessoa.NormalizarEmail(dto.Email);

            var pessoaComCpfExistente = await _repository.ObterPorCpf(cpf);
            if (pessoaComCpfExistente != null && pessoaComCpfExistente.Id != dto.Id)
            {
                throw new BadRequestException(PessoasExceptions.Pessoa_CpfExistente);
            }

            var pessoaComEmailExistente = await _repository.ObterPorEmail(email);
            if (pessoaComEmailExistente != null && pessoaComEmailExistente.Id != dto.Id)
            {
                throw new BadRequestException(PessoasExceptions.Pessoa_EmailExistente);
            }

            pessoa.Atualizar(dto.Nome, email, cpf, dto.TipoPessoa);
            await _repository.Atualizar(pessoa);
            await _unitOfWork.SalvarAlteracoes();

            return _mapper.Map<PessoaResponseDTO>(pessoa);
        }
    }
}
