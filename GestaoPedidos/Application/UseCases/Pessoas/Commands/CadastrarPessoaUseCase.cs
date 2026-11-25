using AutoMapper;
using GestaoPedidos.Application.DTO.Pessoas;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Pessoas;

namespace GestaoPedidos.Application.UseCases.Pessoas.Commands
{
    public class CadastrarPessoaUseCase
    {
        private readonly IPessoaRepository _pessoaRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CadastrarPessoaUseCase(IPessoaRepository pessoaRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _pessoaRepository = pessoaRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PessoaResponseDTO> Executar(PessoaCreateDTO dto)
        {
            var cpf = Pessoa.NormalizarCpf(dto.Cpf);
            var email = Pessoa.NormalizarEmail(dto.Email);

            var pessoaExistente = await _pessoaRepository.ObterPorCpf(cpf);
            if (pessoaExistente != null)
            {
                throw new BadRequestException(PessoasExceptions.Pessoa_CpfExistente);
            }

            var emailExistente = await _pessoaRepository.ObterPorEmail(email);
            if (emailExistente != null)
            {
                throw new BadRequestException(PessoasExceptions.Pessoa_EmailExistente);
            }

            var novaPessoa = new Pessoa(dto.Nome, email, cpf, dto.TipoPessoa);
            await _pessoaRepository.Cadastrar(novaPessoa);
            await _unitOfWork.SalvarAlteracoes();

            return _mapper.Map<PessoaResponseDTO>(novaPessoa);
        }
    }
}
