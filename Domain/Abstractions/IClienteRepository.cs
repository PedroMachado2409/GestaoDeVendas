using GestaoPedidos.Domain.Entities;

namespace GestaoPedidos.Domain.Abstractions
{
    public interface IClienteRepository
    {
        public Task<List<Cliente>> Listar();
        public Task<Cliente?> ObterPorId(int id);
        public Task<Cliente?> ObterPorCpf(string cpf);
        public Task<Cliente?> ObterPorEmail (string email);
        public Task Cadastrar(Cliente cliente);
        public Task Atualizar(Cliente cliente);
    }
}
