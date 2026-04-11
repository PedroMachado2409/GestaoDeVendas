using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestaoPedidos.Infrastructure.Repositories
{
    public class MovimentoFinanceiroRepository : IMovimentosFinanceirosRepository
    {
        private readonly AppDbContext _context;

        public MovimentoFinanceiroRepository (AppDbContext context)
        {
            _context = context;
        }

        public async Task <List<MovimentoFinanceiro>> ListarMovimentos()
        {
            var movimentacoes = await _context.MovimentacoesFinanceiras.OrderBy(M => M.DataCadastro).ToListAsync();
            return movimentacoes;
        }

        public async Task CadastrarMovimento(MovimentoFinanceiro movimento)
        {
            var novoMovimento = await _context.MovimentacoesFinanceiras.AddAsync(movimento);
            await _context.SaveChangesAsync();
        }

        public async Task<MovimentoFinanceiro> ObterPorOrigem(int origemId)
        {
            var movimento = await _context.MovimentacoesFinanceiras.FirstOrDefaultAsync(M => M.OrigemId == origemId);
            return movimento;
        }   
        public async Task<MovimentoFinanceiro> ObterPorId(int id)
        {
            var movimento = await _context.MovimentacoesFinanceiras.FirstOrDefaultAsync(M => M.Id == id);
            return movimento;
        }

        public async Task Deletar(int id)
        {
            var movimento = await _context.MovimentacoesFinanceiras.FirstOrDefaultAsync(M => M.Id == id);
             _context.Remove(movimento);
        }

    }
}
