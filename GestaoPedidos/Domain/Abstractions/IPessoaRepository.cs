using GestaoPedidos.Domain.Entities;

namespace GestaoPedidos.Domain.Abstractions
{
    public interface IPessoaRepository
    {
        public Task<List<Pessoa>> Listar(int pagina, int tamanhoPagina);
        public Task<Pessoa?> ObterPorId(int id);
        public Task<Pessoa?> ObterPorCpf(string cpf);
        public Task<Pessoa?> ObterPorEmail(string email);
        public Task Cadastrar(Pessoa pessoa);
        public Task Atualizar(Pessoa pessoa);
    }
}
