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
                var commitIniciado = false;

                try
                {
                    await using var transacao = await _context.Database.BeginTransactionAsync();

                    var resultado = await operacao();
                    commitIniciado = true;
                    await transacao.CommitAsync();
                    return resultado;
                }
                catch (Exception exception)
                {
                    _context.ChangeTracker.Clear();

                    if (commitIniciado)
                    {
                        throw new InvalidOperationException(
                            "Não foi possível confirmar o resultado da transação. Consulte o resultado antes de repetir a operação.",
                            exception);
                    }

                    throw;
                }
            });
        }
    }
}
