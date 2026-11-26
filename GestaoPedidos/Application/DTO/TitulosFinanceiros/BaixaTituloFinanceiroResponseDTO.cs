using GestaoPedidos.Domain.Enum;

namespace GestaoPedidos.Application.DTO.TitulosFinanceiros
{
    public class BaixaTituloFinanceiroResponseDTO
    {
        public int Id { get; set; }
        public Guid OperacaoId { get; set; }
        public decimal Valor { get; set; }
        public DateOnly Data { get; set; }
        public FormaPagamento Forma { get; set; }
        public int UsuarioId { get; set; }
        public DateTime DataCadastro { get; set; }
        public bool Estornada { get; set; }
        public DateTime? DataEstorno { get; set; }
        public int? UsuarioEstornoId { get; set; }
    }
}
