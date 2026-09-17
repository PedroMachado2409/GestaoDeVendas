namespace GestaoPedidos.Application.DTO.Produtos
{
    public class ProdutoUpdateDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public string Marca { get; set; } = string.Empty;
        public int Estoque { get; set; }
    }
}
