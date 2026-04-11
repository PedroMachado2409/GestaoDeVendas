using GestaoPedidos.Domain.Enum;

namespace GestaoPedidos.Domain.Entities
{
    public class MovimentoFinanceiro
    {
        public int Id { get; set; }
        public string Observacao { get; set; } = string.Empty;
        public int TituloId { get; set; }
        public decimal Valor {  get; set; }
        public Origem TipoOrigem { get; set; }
        public int OrigemId { get; set; }
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

        protected MovimentoFinanceiro() { }

        public MovimentoFinanceiro (string observacao, int tituloId, decimal valor, int origemId, Origem origem)
        {
            Observacao = observacao;
            TituloId = tituloId;
            Valor = valor;
            OrigemId = origemId;
            TipoOrigem = origem;
        }
    }
}
