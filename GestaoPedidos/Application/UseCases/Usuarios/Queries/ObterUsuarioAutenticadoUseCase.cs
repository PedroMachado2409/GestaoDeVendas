using System.Security.Claims;
using AutoMapper;
using GestaoPedidos.Application.DTO.Usuarios;
using GestaoPedidos.Domain.Abstractions.Usuarios;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Usuarios;

namespace GestaoPedidos.Application.UseCases.Usuarios.Queries
{
    public class ObterUsuarioAutenticadoUseCase
    {
        private readonly IUsuarioRepository _repository;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IMapper _mapper;

        public ObterUsuarioAutenticadoUseCase(
            IUsuarioRepository repository,
            IHttpContextAccessor contextAccessor,
            IMapper mapper)
        {
            _repository = repository;
            _contextAccessor = contextAccessor;
            _mapper = mapper;
        }

        public async Task<UsuarioDTO> Executar()
            => _mapper.Map<UsuarioDTO>(await ObterEntidade());

        public async Task<Usuario> ObterEntidade()
        {
            var idClaim = _contextAccessor.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(idClaim, out var usuarioId))
            {
                throw new UnauthorizedException(UsuariosExceptions.Usuario_NaoEncontrado);
            }

            return await _repository.ObterPorId(usuarioId)
                ?? throw new UnauthorizedException(UsuariosExceptions.Usuario_NaoEncontrado);
        }
    }
}
