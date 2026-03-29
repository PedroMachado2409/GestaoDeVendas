using GestaoPedidos.Domain.Entities;

namespace GestaoPedidos.Domain.Abstractions
{
    public interface ITitulosRepository
    {
        Task CadastrarTitulo (Titulo titulos);
        Task<List<Titulo>> ListarTitulos();
        Task<Titulo> ObterTituloPorId(int id);
        Task<Titulo> ObterTituloPelaOrigem(int id);
        Task AtualizarTitulo(Titulo titulo);
        Task DeletarTitulo (int id);
    }
}
