using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GestaoPedidos.Infrastructure.Repositories
{
    public class MensagemWhatsAppRepository : IMensagemWhatsAppRepository
    {
        private const string ViolacaoDeUnicidade = "23505";

        private readonly AppDbContext _context;

        public MensagemWhatsAppRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AdicionarSeNaoExistirAsync(
            MensagemWhatsApp mensagem,
            CancellationToken cancellationToken = default)
        {
            var jaRegistrada = await _context.MensagensWhatsApp
                .AsNoTracking()
                .AnyAsync(
                    m => m.IdMensagemWhatsApp == mensagem.IdMensagemWhatsApp,
                    cancellationToken);

            if (jaRegistrada)
            {
                return false;
            }

            await _context.MensagensWhatsApp.AddAsync(mensagem, cancellationToken);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (DbUpdateException excecao) when (EhViolacaoDeUnicidade(excecao))
            {
                // A Meta reentrega o mesmo webhook quando não recebe 200 a tempo,
                // e duas entregas podem chegar em paralelo. A consulta acima não
                // fecha essa janela; o índice único fecha. Reentrega é sucesso,
                // não conflito: devolver erro faria a Meta retentar sem fim.
                _context.Entry(mensagem).State = EntityState.Detached;
                return false;
            }
        }

        private static bool EhViolacaoDeUnicidade(DbUpdateException excecao)
            => excecao.InnerException is PostgresException postgres
               && postgres.SqlState == ViolacaoDeUnicidade;
    }
}
