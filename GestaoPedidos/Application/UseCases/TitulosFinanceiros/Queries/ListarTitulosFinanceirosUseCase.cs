using AutoMapper;
using GestaoPedidos.Application.DTO.TitulosFinanceiros;
using GestaoPedidos.Application.UseCases.Usuarios.Queries;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Enum;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.TituloFinanceiro;

namespace GestaoPedidos.Application.UseCases.TitulosFinanceiros.Queries
{
    public class ListarTitulosFinanceirosUseCase
    {
        private readonly ITituloFinanceiroRepository _repository;
        private readonly IMapper _mapper;
        private readonly ObterUsuarioAutenticadoUseCase _obterUsuarioAutenticado;

        public ListarTitulosFinanceirosUseCase(ITituloFinanceiroRepository repository,
            IMapper mapper, ObterUsuarioAutenticadoUseCase obterUsuarioAutenticado)
        {
            _repository = repository;
            _mapper = mapper;
            _obterUsuarioAutenticado = obterUsuarioAutenticado;
        }

        public async Task<List<TituloFinanceiroResponseDTO>> Executar(int pagina, int tamanhoPagina)
        {
            var usuario = await _obterUsuarioAutenticado.ObterEntidade();
            if (usuario.Role != UserRole.Admin)
            {
                throw new ForbiddenException(TItuloExceptions.Titulo_SemPermissao);
            }

            if (pagina < 1)
            {
                throw new BadRequestException(PaginaExceptions.Pagina_PaginaInvalida);
            }
            if (tamanhoPagina is < 1 or > 100 || (long)(pagina - 1) * tamanhoPagina > int.MaxValue)
            {
                throw new BadRequestException(PaginaExceptions.Pagina_TamanhoInvalido);
            }

            var titulos = await _repository.Listar(pagina, tamanhoPagina);
            return _mapper.Map<List<TituloFinanceiroResponseDTO>>(titulos);
        }
    }
}
