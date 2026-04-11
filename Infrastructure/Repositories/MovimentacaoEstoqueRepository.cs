using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestaoPedidos.Infrastructure.Repositories
{
    public class MovimentacaoEstoqueRepository : IMovimentacaoEstoqueRepository
    {
        private readonly AppDbContext _context;

        public MovimentacaoEstoqueRepository (AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<MovimentacaoEstoque>> ListarPorProduto(int produtoId)
        {
            var movimentacoes = await _context.MovimentacaoEstoque.Where(m => m.ProdutoId == produtoId)
                .OrderBy(M => M.DataMovimentacao).ToListAsync();
            return movimentacoes;
        }  
        
        public async Task<List<MovimentacaoEstoque>> ListarPorOrigem(int origemId)
        {
            var movimentacoes = await _context.MovimentacaoEstoque.Where(m => m.IdOrigem == origemId)
                .OrderBy(M => M.DataMovimentacao).ToListAsync();
            return movimentacoes;
        }

        public async Task CadastrarMovimentacao(MovimentacaoEstoque movimentacaoEstoque)
        {
            await _context.MovimentacaoEstoque.AddAsync(movimentacaoEstoque);
            await _context.SaveChangesAsync();
        }

        public async Task<MovimentacaoEstoque> ObterMovimentacaoPorId(int id)
        {
            var movimentacao = await _context.MovimentacaoEstoque.FirstOrDefaultAsync(M => M.Id == id);
            return movimentacao;
        }

        public async Task DeletarMovimentacao(int id)
        {
            var movimentacao = await _context.MovimentacaoEstoque.FirstOrDefaultAsync(M => M.Id == id);
             _context.MovimentacaoEstoque.Remove(movimentacao);
            await _context.SaveChangesAsync();
        }
    }
}
