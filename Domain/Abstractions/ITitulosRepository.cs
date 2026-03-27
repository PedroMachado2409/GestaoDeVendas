using GestaoPedidos.Domain.Entities;

namespace GestaoPedidos.Domain.Abstractions
{
    public interface ITitulosRepository
    {
        Task CadastrarTitulo (Titulo titulos);
        Task<Titulo> ObterTituloPorId(int id);
        Task<Titulo> ObterTituloPelaOrigem(int id);
        Task<List<Titulo>> ListarTitulos();
        Task DeletarTitulo (int id);
    }
}
