using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Domain.Enum;


namespace GestaoPedidos.Domain.Abstractions
{
    public interface ITituloFinanceiroRepository
    {
        Task Cadastrar(TituloFinanceiro titulo);

        Task<List<TituloFinanceiro>> Listar(int pagina, int tamanhoPagina);

        Task<TituloFinanceiro?> ObterPorId(int id);
        Task<TituloFinanceiro?> ObterPorOrigem(int? id, OrigemTitulo origemTitulo);

        Task Atualizar(TituloFinanceiro titulo);
    }
}
