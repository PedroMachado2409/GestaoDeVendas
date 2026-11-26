using AutoMapper;
using GestaoPedidos.Application.DTO.TitulosFinanceiros;
using GestaoPedidos.Application.UseCases.Usuarios.Queries;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Enum;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.TituloFinanceiro;

namespace GestaoPedidos.Application.UseCases.TitulosFinanceiros.Commands
{
    public class BaixarTituloFinanceiroUseCase
    {
        private readonly ITituloFinanceiroRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ObterUsuarioAutenticadoUseCase _obterUsuarioAutenticado;

        public BaixarTituloFinanceiroUseCase(ITituloFinanceiroRepository repository,
            IUnitOfWork unitOfWork, IMapper mapper, ObterUsuarioAutenticadoUseCase obterUsuarioAutenticado)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _obterUsuarioAutenticado = obterUsuarioAutenticado;
        }

        public async Task<TituloFinanceiroResponseDTO> Executar(int id, BaixaTituloFinanceiroCreateDTO dto)
        {
            var usuario = await _obterUsuarioAutenticado.ObterEntidade();
            if (usuario.Role != UserRole.Admin)
            {
                throw new ForbiddenException(TItuloExceptions.Titulo_SemPermissao);
            }

            var titulo = await _repository.ObterPorId(id)
                ?? throw new NotFoundException(TItuloExceptions.Titulo_NaoEncontrado);
            var versaoAnterior = titulo.Versao;

            titulo.Baixar(dto.OperacaoId, dto.Valor, dto.Data, dto.Forma, usuario.Id);
            if (titulo.Versao != versaoAnterior)
            {
                await _repository.Atualizar(titulo);
                await _unitOfWork.SalvarAlteracoes();
            }

            return _mapper.Map<TituloFinanceiroResponseDTO>(titulo);
        }
    }
}
