using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestaoPedidos.Infrastructure.Repositories
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly AppDbContext _context;

        public ClienteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Cliente>> Listar()
        {
            var clientes = await _context.Clientes.AsNoTracking().OrderBy(c => c.Id).ToListAsync();
            return clientes;
        }

        public async Task<Cliente?> ObterPorId(int id)
        {
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id);
            return cliente;
        }  
        public async Task<Cliente?> ObterPorCpf(string cpf)
        {
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Cpf == cpf);
            return cliente;
        }

        public async Task<Cliente?> ObterPorEmail (string email)
        {
            var cliente = await _context.Clientes.FirstOrDefaultAsync (c => c.Email == email);
            return cliente;
        }

        public async Task Cadastrar(Cliente cliente)
        {
           await _context.Clientes.AddAsync(cliente);
           await _context.SaveChangesAsync();
       
        }

        public async Task Atualizar(Cliente cliente)
        {
            _context.Clientes.Update(cliente);
            await _context.SaveChangesAsync();
        }

    }
}
