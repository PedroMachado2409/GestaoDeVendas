using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestaoPedidos.Infrastructure.Repositories
{
    public class ProdutoRepository : IProdutoRepository
    {
        private readonly AppDbContext _context;

        public ProdutoRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<List<Produto>> Listar()
            => _context.Produtos
                .AsNoTracking()
                .OrderBy(p => p.Id)
                .ToListAsync();

        public Task<Produto?> ObterPorId(int id)
            => _context.Produtos.FirstOrDefaultAsync(p => p.Id == id);

        public Task<List<Produto>> ObterPorIds(IEnumerable<int> ids)
        {
            var idsDistintos = ids.Distinct().ToArray();
            return _context.Produtos
                .Where(p => idsDistintos.Contains(p.Id))
                .ToListAsync();
        }

        public async Task<Produto> Cadastrar(Produto produto)
        {
            await _context.Produtos.AddAsync(produto);
            await _context.SaveChangesAsync();
            return produto;
        }

        public async Task Atualizar(Produto produto)
        {
            _context.Produtos.Update(produto);
            await _context.SaveChangesAsync();
        }

        public async Task Atualizar(IEnumerable<Produto> produtos)
        {
            _context.Produtos.UpdateRange(produtos);
            await _context.SaveChangesAsync();
        }
    }
}
