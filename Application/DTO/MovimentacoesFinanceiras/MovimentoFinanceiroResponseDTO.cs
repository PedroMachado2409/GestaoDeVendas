using GestaoPedidos.Domain.Enum;

namespace GestaoPedidos.Application.DTO.MovimentacoesFinanceiras
{
    public class MovimentoFinanceiroResponseDTO
    {
        public int Id { get; set; }
        public string Observacao { get; set; } = string.Empty;
        public int TituloId { get; set; }
        public decimal Valor { get; set; }
        public Origem TipoOrigem { get; set; }
        public int OrigemId { get; set; }
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    }
}
