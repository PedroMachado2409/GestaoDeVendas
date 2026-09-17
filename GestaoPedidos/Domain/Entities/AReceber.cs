using GestaoPedidos.Domain.Enum;

namespace GestaoPedidos.Domain.Entities
{
    public class AReceber
    {
        public int Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public OrigemMovimentacao OrigemMovimentacao { get; set; }
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
        public bool StBaixado { get; set; } = false;


    }
}
