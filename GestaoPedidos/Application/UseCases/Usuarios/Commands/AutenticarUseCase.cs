using GestaoPedidos.Application.DTO.Usuarios;
using GestaoPedidos.Domain.Abstractions.Usuarios;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Usuarios;

namespace GestaoPedidos.Application.UseCases.Usuarios.Commands
{
    public class AutenticarUseCase
    {
        private readonly IUsuarioRepository _repository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IToken _token;

        public AutenticarUseCase(
            IUsuarioRepository repository,
            IPasswordHasher passwordHasher,
            IToken token)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
            _token = token;
        }

        public async Task<LoginResponseDTO> Executar(LoginRequestDTO dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();
            var usuario = await _repository.ObterPorEmail(email);

            if (usuario is null
                || !usuario.Ativo
                || !_passwordHasher.Verificar(dto.Senha, usuario.Senha))
            {
                throw new UnauthorizedException(UsuariosExceptions.Usuario_CredenciaisInvalidas);
            }

            return new LoginResponseDTO
            {
                Nome = usuario.Nome,
                Email = usuario.Email,
                Token = _token.GerarToken(usuario)
            };
        }
    }
}
