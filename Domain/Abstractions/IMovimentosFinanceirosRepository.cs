using GestaoPedidos.Domain.Entities;

namespace GestaoPedidos.Domain.Abstractions
{
    public interface IMovimentosFinanceirosRepository
    {
        public Task CadastrarMovimento(MovimentoFinanceiro movimentoFinanceiro);
        public  Task<List<MovimentoFinanceiro>> ListarMovimentos();
        public  Task<MovimentoFinanceiro> ObterPorId(int id);
        public  Task<MovimentoFinanceiro> ObterPorOrigem(int origemId);
        public Task Deletar(int id); 

    }
}
