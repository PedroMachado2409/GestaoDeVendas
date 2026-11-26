using System.Text.Json.Serialization;
using GestaoPedidos.Domain.Enum;

namespace GestaoPedidos.Application.DTO.TitulosFinanceiros
{
    public class TituloFinanceiroCreateDTO
    {
        public int PessoaId { get; set; }
        public string Descricao { get; set; } = string.Empty;
        [JsonRequired]
        public TipoTituloFinanceiro Tipo { get; set; }
        public decimal ValorOriginal { get; set; }
        public DateTime DataVencimento { get; set; }
    }
}
