using AutoMapper;
using GestaoPedidos.Application.DTO.Usuarios;
using GestaoPedidos.Domain.Abstractions.Usuarios;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Domain.Exceptions.Usuarios;
using GestaoPedidos.Infrastructure.Security;

namespace GestaoPedidos.Application.UseCases.Usuarios.Commands
{
    public class RegistrarUsuarioUseCase
    {
        private readonly IUsuarioRepository _repository;
        private readonly IMapper _mapper;

        public RegistrarUsuarioUseCase (IUsuarioRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<UsuarioDTO> Executar(UsuarioCreateDTO dto)
        {
            var usuarioExistente = await _repository.ObterPorEmail(dto.Email);
            if (usuarioExistente != null)
                throw new BadHttpRequestException(UsuariosExceptions.Usuario_JaExistente);

            var usuario = _mapper.Map<Usuario>(dto);
            usuario.Senha = PasswordHelper.HashPassword(dto.Senha);
            await _repository.Cadastrar(usuario);
            return _mapper.Map<UsuarioDTO>(usuario);

        }

    }
}
