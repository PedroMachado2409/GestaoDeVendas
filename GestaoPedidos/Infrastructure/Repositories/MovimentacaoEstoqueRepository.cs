using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestaoPedidos.Infrastructure.Repositories
{
    public class MovimentacaoEstoqueRepository : IMovimentacaoEstoqueRepository
    {
        private readonly AppDbContext _context;

        public MovimentacaoEstoqueRepository(AppDbContext context)
        {
            _context = context;
        }

        // Sem SaveChanges: quem decide a hora de gravar é o caso de uso,
        // pela unidade de trabalho.
        public async Task Cadastrar(MovimentacaoEstoque movimentacaoEstoque)
            => await _context.MovimentacoesEstoque.AddAsync(movimentacaoEstoque);

        public Task<List<MovimentacaoEstoque>> ListarPorProduto(int produtoId)
            => _context.MovimentacoesEstoque
                .AsNoTracking()
                .Where(m => m.ProdutoId == produtoId)
                .OrderByDescending(m => m.DataMovimentacao)
                .ThenByDescending(m => m.Id)
                .ToListAsync();
    }
}
