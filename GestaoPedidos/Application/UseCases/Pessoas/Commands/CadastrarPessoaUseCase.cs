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
        private readonly IMapper _mapper;

        public CadastrarPessoaUseCase(IPessoaRepository pessoaRepository, IMapper mapper)
        {
            _pessoaRepository = pessoaRepository;
            _mapper = mapper;
        }

        public async Task<PessoaResponseDTO> Executar(PessoaCreateDTO dto)
        {
            var pessoaExistente = await _pessoaRepository.ObterPorCpf(dto.Cpf);
            if (pessoaExistente != null)
            {
                throw new BadRequestException(PessoasExceptions.Pessoa_CpfExistente);
            }

            var emailExistente = await _pessoaRepository.ObterPorEmail(dto.Email);
            if (emailExistente != null)
            {
                throw new BadRequestException(PessoasExceptions.Pessoa_EmailExistente);
            }

            var novoPessoa = new Pessoa(dto.Nome, dto.Email, dto.Cpf, dto.TipoPessoa);

            await _pessoaRepository.Cadastrar(novoPessoa);

            return _mapper.Map<PessoaResponseDTO>(novoPessoa);
        }
    }
}
