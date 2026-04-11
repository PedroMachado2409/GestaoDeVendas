using GestaoPedidos.Domain.Entities;

namespace GestaoPedidos.Domain.Abstractions
{
    public interface IConfiguracaoRepository
    {
        public Task <Configuracao>ObterConfiguracao();
        public Task AtualizarConfiguracao(Configuracao configuracao);
    }
}
