using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestaoPedidos.Infrastructure.Repositories
{
    public class ConfiguracaoRepository : IConfiguracaoRepository
    {
        private readonly AppDbContext _context;

        public ConfiguracaoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task <Configuracao>ObterConfiguracao()
        {
            var configuracao = await _context.Configuracoes.FirstOrDefaultAsync(x => x.Id == 1);
            return configuracao;
        }

        public async Task AtualizarConfiguracao(Configuracao configuracao)
        {
             _context.Configuracoes.Update(configuracao);
            await _context.SaveChangesAsync();
        }
        
    }
}
