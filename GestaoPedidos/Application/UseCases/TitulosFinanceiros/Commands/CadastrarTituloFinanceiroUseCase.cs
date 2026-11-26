using AutoMapper;
using GestaoPedidos.Application.DTO.TitulosFinanceiros;
using GestaoPedidos.Application.UseCases.Usuarios.Queries;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Domain.Enum;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Pessoas;
using GestaoPedidos.Domain.Exceptions.TituloFinanceiro;

namespace GestaoPedidos.Application.UseCases.TitulosFinanceiros.Commands
{
    public class CadastrarTituloFinanceiroUseCase
    {
        private readonly ITituloFinanceiroRepository _repository;
        private readonly IPessoaRepository _pessoaRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ObterUsuarioAutenticadoUseCase _obterUsuarioAutenticado;

        public CadastrarTituloFinanceiroUseCase(ITituloFinanceiroRepository repository,
            IPessoaRepository pessoaRepository, IUnitOfWork unitOfWork, IMapper mapper,
            ObterUsuarioAutenticadoUseCase obterUsuarioAutenticado)
        {
            _repository = repository;
            _pessoaRepository = pessoaRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _obterUsuarioAutenticado = obterUsuarioAutenticado;
        }

        public async Task<TituloFinanceiroResponseDTO> Executar(TituloFinanceiroCreateDTO dto)
        {
            var usuario = await _obterUsuarioAutenticado.ObterEntidade();
            if (usuario.Role != UserRole.Admin)
            {
                throw new ForbiddenException(TItuloExceptions.Titulo_SemPermissao);
            }

            var pessoa = await _pessoaRepository.ObterPorId(dto.PessoaId)
                ?? throw new NotFoundException(PessoasExceptions.Pessoa_NaoEncontrado);
            if (!pessoa.Ativo)
            {
                throw new ConflictException(PessoasExceptions.Pessoa_Inativo);
            }

            var titulo = new TituloFinanceiro(dto.PessoaId, dto.Descricao, dto.Tipo,
                dto.ValorOriginal, dto.DataVencimento, 0, OrigemTitulo.Financeiro);
            await _repository.Cadastrar(titulo);
            await _unitOfWork.SalvarAlteracoes();

            return _mapper.Map<TituloFinanceiroResponseDTO>(titulo);
        }
    }
}
