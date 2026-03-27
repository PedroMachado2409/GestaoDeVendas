namespace GestaoPedidos.Application.DTO.Pedidos
{
    public class AtualizarPedidoRequestDTO
    {
        public int Id { get; set; }
        public List<CriarPedidoItemRequestDTO> Itens { get; set; } = new();
    }
}