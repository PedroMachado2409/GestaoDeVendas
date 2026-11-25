using System.Text.Json.Serialization;
using GestaoPedidos.Domain.Enum;

namespace GestaoPedidos.Application.DTO.Pedidos
{
    public class CriarPedidoRequestDTO
    {
        public int PessoaId { get; set; }
        [JsonRequired]
        public TipoMovimentacao tipoMovimentacao { get; set; }
        public List<CriarPedidoItemRequestDTO> Itens { get; set; } = new();

    }
}
