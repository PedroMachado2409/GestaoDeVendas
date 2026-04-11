using GestaoPedidos.Domain.Enum;

namespace GestaoPedidos.Application.DTO.MovimentacoesEstoque
{
    public class MovimentacaoResponseDTO
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
        public int IdOrigem { get; set; }
        public Origem NomeOrigem { get; set; }
        public DateTime DataMovimentacao { get; set; } = DateTime.UtcNow;
    }
}
