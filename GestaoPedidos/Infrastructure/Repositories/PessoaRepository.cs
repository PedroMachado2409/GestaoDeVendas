using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestaoPedidos.Infrastructure.Repositories
{
    public class PessoaRepository : IPessoaRepository
    {
        private readonly AppDbContext _context;

        public PessoaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Pessoa>> Listar()
        {
            var pessoas = await _context.Pessoas.AsNoTracking().OrderBy(c => c.Id).ToListAsync();
            return pessoas;
        }

        public async Task<Pessoa?> ObterPorId(int id)
        {
            var pessoa = await _context.Pessoas.FirstOrDefaultAsync(c => c.Id == id);
            return pessoa;
        }
        public async Task<Pessoa?> ObterPorCpf(string cpf)
        {
            var pessoa = await _context.Pessoas.FirstOrDefaultAsync(c => c.Cpf == cpf);
            return pessoa;
        }

        public async Task<Pessoa?> ObterPorEmail(string email)
        {
            var pessoa = await _context.Pessoas.FirstOrDefaultAsync(c => c.Email == email);
            return pessoa;
        }

        public async Task Cadastrar(Pessoa pessoa)
        {
            await _context.Pessoas.AddAsync(pessoa);
            await _context.SaveChangesAsync();

        }

        public async Task Atualizar(Pessoa pessoa)
        {
            _context.Pessoas.Update(pessoa);
            await _context.SaveChangesAsync();
        }

    }
}
