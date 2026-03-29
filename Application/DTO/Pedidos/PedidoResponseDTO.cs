using GestaoPedidos.Domain.Entities.Pedidos;
using GestaoPedidos.Domain.Enum;

namespace GestaoPedidos.Application.DTO.Pedidos
{
    public class PedidoResponseDTO
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string Status { get; set; }
        public DateTime DataCadastro { get; set; }
        public decimal ValorTotal { get; set; }
        public TipoPedido Tipo { get; set; }
        public List<PedidoItemResponseDTO> Itens { get; set; } = new();

    }
}
