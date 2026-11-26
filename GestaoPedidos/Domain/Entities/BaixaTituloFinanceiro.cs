using GestaoPedidos.Domain.Enum;

namespace GestaoPedidos.Domain.Entities
{
    public class BaixaTituloFinanceiro
    {
        public int Id { get; private set; }
        public int TituloFinanceiroId { get; private set; }
        public Guid OperacaoId { get; private set; }
        public decimal Valor { get; private set; }
        public DateOnly Data { get; private set; }
        public FormaPagamento Forma { get; private set; }
        public int UsuarioId { get; private set; }
        public DateTime DataCadastro { get; private set; } = DateTime.UtcNow;
        public bool Estornada { get; private set; }
        public DateTime? DataEstorno { get; private set; }
        public int? UsuarioEstornoId { get; private set; }

        protected BaixaTituloFinanceiro() { }

        internal BaixaTituloFinanceiro(Guid operacaoId, decimal valor, DateOnly data, FormaPagamento forma, int usuarioId)
        {
            OperacaoId = operacaoId;
            Valor = valor;
            Data = data;
            Forma = forma;
            UsuarioId = usuarioId;
        }

        internal void Estornar(int usuarioId)
        {
            Estornada = true;
            DataEstorno = DateTime.UtcNow;
            UsuarioEstornoId = usuarioId;
        }
    }
}
