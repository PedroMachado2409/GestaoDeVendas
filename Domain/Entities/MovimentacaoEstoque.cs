using GestaoPedidos.Domain.Enum;

namespace GestaoPedidos.Domain.Entities
{
    public class MovimentacaoEstoque
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
        public int IdOrigem { get; set; }
        public TipoMovimentacao Tipo {  get; set; }
        public Origem NomeOrigem { get; set; }
        public DateTime DataMovimentacao { get; set; } = DateTime.UtcNow;

        protected MovimentacaoEstoque() { }

        public MovimentacaoEstoque (int produtoId, int quantidade, int idOrigem, Origem nomeOrigem, TipoMovimentacao tipo)
        {
            ProdutoId = produtoId;
            Quantidade = quantidade;
            IdOrigem = idOrigem;
            NomeOrigem = nomeOrigem;
            Tipo = tipo;
        }

        public void ConverterQuantidade()
        {
            if(Tipo == TipoMovimentacao.Entrada)
            {
                Quantidade = +Math.Abs(Quantidade);
            } else if(Tipo == TipoMovimentacao.Saida)
            {
                Quantidade = -Math.Abs(Quantidade);
            }
        }
    }
}
