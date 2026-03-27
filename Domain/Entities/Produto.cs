namespace GestaoPedidos.Domain.Entities
{
    public class Produto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public int Estoque { get; set; } = 0;
        public int QuantidadeReservada { get; set; } = 0;
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
        public bool Ativo { get; set; } = true;
        protected Produto() { }

        public Produto (string nome, string marca, int estoque, decimal preco)
        {
            Nome = nome;
            Marca = marca;
            Estoque = estoque;
            Preco = preco;
        }
        public void Atualizar(string nome, string marca, int estoque, decimal preco)
        {
            Nome = nome;
            Marca = marca;
            Estoque = estoque;
            Preco = preco;
        }

        public void Ativar () => Ativo = true;
        public void Inativar () => Ativo = false;

        public void BaixarEstoque(int quantidade)
        {
            if (quantidade <= 0)
                throw new Exception("Quantidade inválida.");

            if (Estoque < quantidade)
                throw new Exception("Estoque insuficiente.");

            Estoque -= quantidade;
        }

        public void ReservarQuantidade(int quantidade)
        {
            {
                if (quantidade <= 0)
                    throw new Exception("Quantidade inválida.");

                if (Estoque < quantidade)
                    throw new Exception("Estoque insuficiente.");

                QuantidadeReservada += quantidade;
                Estoque -= quantidade;
            }
        }

        public void CancelarReservaDeQuantidade(int quantidade)
        {
            if (quantidade <= 0)
                throw new Exception("Quantidade inválida.");

            QuantidadeReservada -= quantidade;
            Estoque += quantidade;
        }
    }
}
