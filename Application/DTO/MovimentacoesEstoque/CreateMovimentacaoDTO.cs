using GestaoPedidos.Domain.Enum;

namespace GestaoPedidos.Application.DTO.MovimentacoesEstoque
{
    public class CreateMovimentacaoDTO
    {

        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
        public int IdOrigem { get; set; }
        public Origem NomeOrigem { get; set; }

    }
}
