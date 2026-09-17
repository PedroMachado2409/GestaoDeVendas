using AutoMapper;
using GestaoPedidos.Application.DTO.Usuarios;
using GestaoPedidos.Domain.Abstractions.Usuarios;
using GestaoPedidos.Domain.Enum;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Usuarios;

namespace GestaoPedidos.Application.UseCases.Usuarios.Commands
{
    public class AlterarRoleUsuarioUseCase
    {
        private readonly IUsuarioRepository _repository;
        private readonly IMapper _mapper;

        public AlterarRoleUsuarioUseCase(IUsuarioRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<UsuarioDTO> Executar(int usuarioId, UserRole role)
        {
            if (!Enum.IsDefined(role))
            {
                throw new BadRequestException("Papel de usuário inválido.");
            }

            var usuario = await _repository.ObterPorId(usuarioId)
                ?? throw new NotFoundException(UsuariosExceptions.Usuario_NaoEncontrado);

            usuario.AlterarRole(role);
            await _repository.Atualizar(usuario);

            return _mapper.Map<UsuarioDTO>(usuario);
        }
    }
}
