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

        public async Task<List<Produto>> Listar()
        {
            var produtos = await _context.Produtos.AsNoTracking().OrderBy(p => p.Id).ToListAsync();
            return produtos;
        }
         
        public async Task<Produto?> ObterPorId(int? id)
        {
            var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.Id == id);
            return produto;
        }

        public async Task<Produto> Cadastrar(Produto produto)
        {
             await _context.Produtos.AddAsync(produto);
            await _context.SaveChangesAsync();
            return produto;
        }
        public async Task AdicionarEmLote(List<Produto> produtos)
        {
            await _context.Produtos.AddRangeAsync(produtos);
            await _context.SaveChangesAsync();
        }

        public async Task Atualizar(Produto produto)
        {
             _context.Produtos.Update(produto);
            await _context.SaveChangesAsync();
        }
    }
}
