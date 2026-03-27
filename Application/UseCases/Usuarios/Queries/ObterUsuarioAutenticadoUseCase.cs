using AutoMapper;
using GestaoPedidos.Application.DTO.Usuarios;
using GestaoPedidos.Domain.Abstractions.Usuarios;
using System.Security.Claims;

namespace GestaoPedidos.Application.UseCases.Usuarios.Queries
{
    public class ObterUsuarioAutenticadoUseCase
    {
        private readonly IConfiguration _config;
        private readonly IUsuarioRepository _repository;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IMapper _mapper;

        public ObterUsuarioAutenticadoUseCase (IConfiguration config, IUsuarioRepository repository, IHttpContextAccessor contextAccessor, IMapper mapper)
        {
            _config = config;
            _repository = repository;
            _contextAccessor = contextAccessor;
            _mapper = mapper;
        }

        public async Task <UsuarioDTO> Executar()
        {
            var httpContext = _contextAccessor.HttpContext;
            var email = httpContext?.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var usuario = await _repository.ObterPorEmail(email);
            return _mapper.Map<UsuarioDTO>(usuario);
        }
    }
}
