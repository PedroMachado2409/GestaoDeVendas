using GestaoPedidos.Domain.Enum;

namespace GestaoPedidos.Application.DTO.TitulosFinanceiros
{
    public class TituloFinanceiroResponseDTO
    {
        public int Id { get; set; }
        public int PessoaId { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public TipoTituloFinanceiro Tipo { get; set; }
        public decimal ValorOriginal { get; set; }
        public decimal ValorBaixado { get; set; }
        public decimal Saldo { get; set; }
        public List<BaixaTituloFinanceiroResponseDTO> Baixas { get; set; } = new();
        public DateTime DataVencimento { get; set; }
        public DateTime DataCadastro { get; set; }
        public StatusTituloFinanceiro Status { get; set; }
        public Guid Versao { get; set; }
    }
}
