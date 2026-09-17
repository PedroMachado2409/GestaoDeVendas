
namespace GestaoPedidos.Application.DTO.Pedidos
{
    public class PedidoResponseDTO
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNome { get; set; } = string.Empty;
        public int UsuarioId { get; set; }
        public string UsuarioNome { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DataCadastro { get; set; }
        public decimal ValorTotal { get; set; }

        public List<PedidoItemResponseDTO> Itens { get; set; } = new();

    }
}
