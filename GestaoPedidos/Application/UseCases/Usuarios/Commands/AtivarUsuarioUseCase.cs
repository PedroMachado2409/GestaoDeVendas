
using GestaoPedidos.Domain.Abstractions.Usuarios;
using GestaoPedidos.Domain.Exceptions.Usuarios;


namespace GestaoPedidos.Application.UseCases.Usuarios.Commands
{
    public class AtivarUsuarioUseCase
    {
        private readonly IUsuarioRepository _repository;

        public AtivarUsuarioUseCase(IUsuarioRepository usuarioRepository)
        {
            _repository = usuarioRepository;
        }

        public async Task<bool> Executar(int id)
        {
            var usuario = await _repository.ObterPorId(id);
            if (usuario == null)
            {
                throw new GestaoPedidos.Domain.Exceptions.NotFoundException(UsuariosExceptions.Usuario_NaoEncontrado);
            }

            if (usuario.Ativo == true)
            {
                throw new GestaoPedidos.Domain.Exceptions.BadRequestException(UsuariosExceptions.Usuario_JaAtivo);
            }

            usuario.Ativar();
            await _repository.Atualizar(usuario);
            return true;

        }
    }
}
