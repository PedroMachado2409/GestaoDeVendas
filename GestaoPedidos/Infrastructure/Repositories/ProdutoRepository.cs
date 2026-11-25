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

        public Task<List<Produto>> Listar(int pagina, int tamanhoPagina)
            => _context.Produtos.AsNoTracking().OrderBy(p => p.Id)
                .Skip((pagina - 1) * tamanhoPagina).Take(tamanhoPagina).ToListAsync();

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
            return produto;
        }

        public Task Atualizar(Produto produto)
        {
            _context.Produtos.Update(produto);
            return Task.CompletedTask;
        }

    }
}
