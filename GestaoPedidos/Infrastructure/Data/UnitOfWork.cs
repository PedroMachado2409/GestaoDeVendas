using GestaoPedidos.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GestaoPedidos.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public Task<int> SalvarAlteracoes()
            => _context.SaveChangesAsync();

        public async Task<T> ExecutarEmTransacao<T>(Func<Task<T>> operacao)
        {
            if (_context.Database.CurrentTransaction is not null)
            {
                return await operacao();
            }

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transacao = await _context.Database.BeginTransactionAsync();

                try
                {
                    var resultado = await operacao();
                    await transacao.CommitAsync();
                    return resultado;
                }
                catch
                {
                    await transacao.RollbackAsync();
                    throw;
                }
            });
        }
    }
}
