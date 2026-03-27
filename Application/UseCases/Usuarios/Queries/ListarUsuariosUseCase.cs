using AutoMapper;
using GestaoPedidos.Application.DTO.Usuarios;
using GestaoPedidos.Domain.Abstractions.Usuarios;

namespace GestaoPedidos.Application.UseCases.Usuarios.Queries
{
    public class ListarUsuariosUseCase
    {
        private readonly IUsuarioRepository _repository;
        private readonly IMapper _mapper;

        public ListarUsuariosUseCase(IUsuarioRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<UsuarioDTO>> Executar()
        {
            var usuarios = await _repository.Listar();
            return _mapper.Map<List<UsuarioDTO>>(usuarios);
        }
    }
}
