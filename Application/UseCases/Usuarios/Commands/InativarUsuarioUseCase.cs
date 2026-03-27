using GestaoPedidos.Domain.Abstractions.Usuarios;
using GestaoPedidos.Domain.Exceptions.Usuarios;

namespace GestaoPedidos.Application.UseCases.Usuarios.Commands
{
    public class InativarUsuarioUseCase
    {
        private readonly IUsuarioRepository _repository;

        public InativarUsuarioUseCase(IUsuarioRepository usuarioRepository)
        {
            _repository = usuarioRepository;
        }

        public async Task<bool> Executar(int id)
        {
            var usuario = await _repository.ObterPorId(id);
            if (usuario == null)
                throw new BadHttpRequestException(UsuariosExceptions.Usuario_NaoEncontrado);

            if (usuario.Ativo == false)
                throw new BadHttpRequestException(UsuariosExceptions.Usuario_JaInativo);

            usuario.Inativar();
            await _repository.Atualizar(usuario);
            return true;
        }
    }
}
