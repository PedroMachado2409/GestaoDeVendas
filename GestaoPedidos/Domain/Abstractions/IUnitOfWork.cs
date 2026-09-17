namespace GestaoPedidos.Domain.Abstractions
{
    public interface IUnitOfWork
    {
        Task<int> SalvarAlteracoes();
        Task<T> ExecutarEmTransacao<T>(Func<Task<T>> operacao);
    }
}
