using AutoMapper;
using GestaoPedidos.Application.DTO.Usuarios;
using GestaoPedidos.Domain.Abstractions.Usuarios;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Domain.Enum;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Usuarios;

namespace GestaoPedidos.Application.UseCases.Usuarios.Commands
{
    public class RegistrarUsuarioUseCase
    {
        private readonly IUsuarioRepository _repository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IMapper _mapper;

        public RegistrarUsuarioUseCase(
            IUsuarioRepository repository,
            IPasswordHasher passwordHasher,
            IMapper mapper)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
        }

        public async Task<UsuarioDTO> Executar(UsuarioCreateDTO dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();
            var usuarioExistente = await _repository.ObterPorEmail(email);

            if (usuarioExistente is not null)
            {
                throw new ConflictException(UsuariosExceptions.Usuario_JaExistente);
            }

            var senhaHash = _passwordHasher.Hash(dto.Senha);
            var usuario = new Usuario(dto.Nome, email, senhaHash, UserRole.Vendedor);

            await _repository.Cadastrar(usuario);

            return _mapper.Map<UsuarioDTO>(usuario);
        }
    }
}
