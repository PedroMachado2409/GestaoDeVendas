namespace GestaoPedidos.Application.DTO.Produtos
{
    public class ProdutoResponseDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public decimal Preco { get; set; }
        public string Marca { get; set; } 
        public int Estoque { get; set; } 
        public int QuantidadeReservadaParaVenda { get; set; }
        public DateTime DataCadastro { get; set; } 
        public bool Ativo { get; set; } = true;
    }
}
