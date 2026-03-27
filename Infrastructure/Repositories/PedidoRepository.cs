using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities.Pedidos;
using GestaoPedidos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestaoPedidos.Infrastructure.Repositories
{
    public class PedidoRepository : IPedidoRepository
    {
        private readonly AppDbContext _context;

        public PedidoRepository (AppDbContext context)
        {
            _context = context;
        }

        public async Task Cadastrar(Pedido pedido)
        {
            await _context.Pedidos.AddAsync(pedido);
            await _context.SaveChangesAsync();
        }

        public async Task<Pedido?> ObterPorId(int id)
        {
            return await _context.Pedidos.Include(p => p.Itens).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task Atualizar (Pedido pedido)
        {
            _context.Pedidos.Update(pedido);
            await _context.SaveChangesAsync();
        }
    }
}
