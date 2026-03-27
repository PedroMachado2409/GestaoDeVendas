using AutoMapper;
using GestaoPedidos.Application.DTO.Usuarios;
using GestaoPedidos.Domain.Abstractions.Usuarios;
using GestaoPedidos.Domain.Exceptions.Usuarios;
using GestaoPedidos.Infrastructure.Security;

namespace GestaoPedidos.Application.UseCases.Usuarios.Commands
{
    public class AutenticarUseCase
    {
        private readonly IUsuarioRepository _repository;
        private readonly IToken _token;

        public AutenticarUseCase (IUsuarioRepository repository, IToken token)
        {
            _repository = repository;
            _token = token;
        }

        public async Task<LoginResponseDTO> Executar(LoginRequestDTO dto)
        {
            var usuario = await _repository.ObterPorEmail(dto.Email);
            if (usuario == null || !PasswordHelper.VerificarSenha(dto.senha, usuario.Senha))
                throw new BadHttpRequestException(UsuariosExceptions.Usuario_CredenciaisInvalidas);

            var token = _token.GerarToken(usuario);
            return new LoginResponseDTO { Nome = usuario.Nome, Email = usuario.Email, Token = token };

        }

    }
}
