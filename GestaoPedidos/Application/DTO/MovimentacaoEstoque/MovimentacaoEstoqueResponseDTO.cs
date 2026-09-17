using GestaoPedidos.Domain.Enum;

namespace GestaoPedidos.Application.DTO.MovimentacaoEstoque
{
    public class MovimentacaoEstoqueResponseDTO
    {
        public int Id { get; set; }
        public string Observacao { get; set; } = string.Empty;
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public int IdOrigem { get; set; }
        public OrigemMovimentacao OrigemMovimentacao { get; set; }
        public TipoMovimentacao TipoMovimentacao { get; set; }
        public DateTime DataMovimentacao { get; set; }
    }
}
