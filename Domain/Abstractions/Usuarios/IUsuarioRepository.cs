using GestaoPedidos.Domain.Entities;

namespace GestaoPedidos.Domain.Abstractions.Usuarios
{
    public interface IUsuarioRepository
    {
        public Task<List<Usuario>> Listar();
        public Task<Usuario?> ObterPorId(int id);
        public Task<Usuario?> ObterPorEmail(string email);
        public Task<Usuario> Cadastrar(Usuario usuario);
        public Task Atualizar(Usuario usuario);
    }
}
