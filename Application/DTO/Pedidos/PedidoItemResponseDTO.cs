namespace GestaoPedidos.Application.DTO.Pedidos
{
    public class PedidoItemResponseDTO
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public decimal Preco { get; set; }
        public int Quantidade { get; set; }
        public decimal SubTotal { get; set; }
    }
}
