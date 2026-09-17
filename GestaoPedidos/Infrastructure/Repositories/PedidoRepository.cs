using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities.Pedidos;
using GestaoPedidos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestaoPedidos.Infrastructure.Repositories
{
    public class PedidoRepository : IPedidoRepository
    {
        private readonly AppDbContext _context;

        public PedidoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task <List<Pedido>> Listar()
        {
            var pedidos = await _context.Pedidos
                .Include(p => p.Itens)
                .Include(p => p.Cliente)
                .Include(p => p.Usuario)
                .ToListAsync();
            return pedidos;
        }

        public async Task Cadastrar(Pedido pedido)
        {
            await _context.Pedidos.AddAsync(pedido);
        }

        public async Task<Pedido?> ObterPorId(int id)
        {
            return await _context.Pedidos
                .Include(p => p.Itens)
                .Include(p => p.Cliente)
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task Atualizar(Pedido pedido)
        {
            _context.Pedidos.Update(pedido);
            await Task.CompletedTask;
        }
    }
}
