using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestaoPedidos.Infrastructure.Repositories
{
    public class TitulosRepository : ITitulosRepository
    {
        private readonly AppDbContext _context;

        public TitulosRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task CadastrarTitulo(Titulo titulos)
        {
            await _context.Titulos.AddAsync(titulos);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Titulo>> ListarTitulos()
        {
            var titulos = await _context.Titulos.OrderBy(t => t.Id).ToListAsync();
            return titulos;
        }

        public async Task<Titulo?> ObterTituloPorId(int id)
        {
            var titulo = await _context.Titulos
                .FirstOrDefaultAsync(t => t.Id == id);
            return titulo;
        }

        public async Task<Titulo?> ObterTituloPelaOrigem(int origemId)
        {
            var titulo = await _context.Titulos.FirstOrDefaultAsync(t => t.IdOrigem == origemId);
            return titulo;
        }

        public async Task AtualizarTitulo(Titulo titulo)
        {
             _context.Titulos.Update(titulo);
            await _context.SaveChangesAsync();
        }

        public async Task DeletarTitulo(int id)
        {
            var titulo = await _context.Titulos.FirstOrDefaultAsync(t => t.Id == id);
             _context.Titulos.Remove(titulo);
            await _context.SaveChangesAsync();
        }
    }
}
