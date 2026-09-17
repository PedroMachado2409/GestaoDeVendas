using GestaoPedidos.Domain.Enum;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Produtos;

namespace GestaoPedidos.Domain.Entities
{
    /// <summary>
    /// Registro imutável de uma alteração de estoque. É trilha de auditoria:
    /// depois de gravada, uma movimentação não é editada nem apagada — um erro
    /// é corrigido por uma movimentação nova, em sentido contrário.
    ///
    /// ProdutoNome é copiado de propósito. A trilha precisa dizer como o produto
    /// se chamava no momento do movimento, e não como ele se chama hoje.
    /// </summary>
    public class MovimentacaoEstoque
    {
        public int Id { get; private set; }
        public string Observacao { get; private set; } = string.Empty;
        public int ProdutoId { get; private set; }
        public Produto? Produto { get; private set; }
        public string ProdutoNome { get; private set; } = string.Empty;
        public int Quantidade { get; private set; }
        public int? IdOrigem { get; private set; }
        public OrigemMovimentacao OrigemMovimentacao { get; private set; }
        public TipoMovimentacao TipoMovimentacao { get; private set; }
        public DateTime DataMovimentacao { get; private set; } = DateTime.UtcNow;

        protected MovimentacaoEstoque() { }

        public MovimentacaoEstoque(
            int produtoId,
            string produtoNome,
            int quantidade,
            int idOrigem,
            OrigemMovimentacao origemMovimentacao,
            TipoMovimentacao tipoMovimentacao,
            string observacao)
        {
            if (quantidade <= 0)
            {
                throw new BadRequestException(ProdutoExceptions.Produto_QuantidadeInvalida);
            }

            ProdutoId = produtoId;
            ProdutoNome = produtoNome.Trim();
            Quantidade = quantidade;
            IdOrigem = idOrigem;
            OrigemMovimentacao = origemMovimentacao;
            TipoMovimentacao = tipoMovimentacao;
            Observacao = observacao.Trim();
            DataMovimentacao = DateTime.UtcNow;
        }
    }
}
