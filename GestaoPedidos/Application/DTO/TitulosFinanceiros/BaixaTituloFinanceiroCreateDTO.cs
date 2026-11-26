using System.Text.Json.Serialization;
using GestaoPedidos.Domain.Enum;

namespace GestaoPedidos.Application.DTO.TitulosFinanceiros
{
    public class BaixaTituloFinanceiroCreateDTO
    {
        public Guid OperacaoId { get; set; }
        public decimal Valor { get; set; }
        public DateOnly Data { get; set; }
        [JsonRequired]
        public FormaPagamento Forma { get; set; }
    }
}
