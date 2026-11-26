using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Domain.Enum;
using GestaoPedidos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestaoPedidos.Infrastructure.Repositories
{
    public class TituloFinanceiroRepository : ITituloFinanceiroRepository
    {
        private readonly AppDbContext _context;

        public TituloFinanceiroRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task Cadastrar(TituloFinanceiro titulo)
        {
            await _context.TitulosFinanceiros.AddAsync(titulo);
        }

        public Task<List<TituloFinanceiro>> Listar(int pagina, int tamanhoPagina)
            => _context.TitulosFinanceiros.AsNoTracking().Include(t => t.Baixas).OrderBy(t => t.Id)
                .Skip((pagina - 1) * tamanhoPagina).Take(tamanhoPagina).ToListAsync();

        public Task<TituloFinanceiro?> ObterPorId(int id)
            => _context.TitulosFinanceiros.Include(t => t.Baixas).FirstOrDefaultAsync(t => t.Id == id);

        public Task<TituloFinanceiro?> ObterPorOrigem(int? id, OrigemTitulo origemTitulo)
            => _context.TitulosFinanceiros.Include(t => t.Baixas).FirstOrDefaultAsync(t => t.IdOrigem == id && t.OrigemTitulo == origemTitulo);

        public Task Atualizar(TituloFinanceiro titulo)
        {
            _context.TitulosFinanceiros.Update(titulo);
            return Task.CompletedTask;
        }
    }
}
