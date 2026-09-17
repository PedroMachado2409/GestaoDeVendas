namespace GestaoPedidos.Domain.Entities.PrazoPagamento
{
    public class PrazoPagamentoParcela
    {
        public int Id { get; set; }
        public int Numero { get; set; }
        public int Dias {  get; set; }
        public decimal Percentual { get; set; }
        public int PrazoPagamentoId { get; set; }

        protected PrazoPagamentoParcela() { }
        internal PrazoPagamentoParcela(int numero, int dias, decimal percentual)
        {
            Numero = numero;
            Dias = dias;
            Percentual = percentual;
        }
    }
}
