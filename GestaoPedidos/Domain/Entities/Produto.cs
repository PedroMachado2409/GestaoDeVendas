using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Produtos;

namespace GestaoPedidos.Domain.Entities
{
    public class Produto
    {
        public int Id { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string Marca { get; private set; } = string.Empty;
        public decimal Preco { get; private set; }
        public int Estoque { get; private set; }
        public int QuantidadeReservada { get; private set; }
        public int QuantidadeCompradaPendente { get; private set; }
        public DateTime DataCadastro { get; private set; } = DateTime.UtcNow;
        public bool Ativo { get; private set; } = true;

        public Guid Versao { get; private set; } = Guid.NewGuid();

        protected Produto() { }

        public Produto(string nome, string marca, int estoque, decimal preco)
        {
            Nome = nome.Trim();
            Marca = marca.Trim();
            Estoque = estoque;
            Preco = preco;
        }

        public void Atualizar(string nome, string marca, int estoque, decimal preco)
        {
            if (estoque < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(estoque));
            }

            Nome = nome.Trim();
            Marca = marca.Trim();
            Estoque = estoque;
            Preco = preco;
            AtualizarVersao();
        }

        public void Ativar()
        {
            Ativo = true;
            AtualizarVersao();
        }

        public void Inativar()
        {
            Ativo = false;
            AtualizarVersao();
        }

        public void ReservarQuantidadeVenda(int quantidade)
        {
            ValidarQuantidadePositiva(quantidade);

            if (Estoque < quantidade)
            {
                throw new ConflictException(ProdutoExceptions.Produto_EstoqueInsuficiente);
            }

            QuantidadeReservada += quantidade;
            Estoque -= quantidade;
            AtualizarVersao();
        }

        public void AjusteEstoque(int quantidade)
        {
            if(quantidade == 0)
            {
                throw new BadRequestException(ProdutoExceptions.Produto_AjusteNaoPodeSerZero);
            }
            if(quantidade + Estoque < 0)
            {
                throw new BadRequestException(ProdutoExceptions.Produto_EstoqueNaoPodeSerNegativo);
            }

            Estoque += quantidade;
            AtualizarVersao();
            
        }

        public void LiberarReserva(int quantidade)
        {
            ValidarReservaDeVenda(quantidade);

            QuantidadeReservada -= quantidade;
            Estoque += quantidade;
            AtualizarVersao();
        }

        public void ConfirmarReserva(int quantidade)
        {
            ValidarReservaDeVenda(quantidade);

            QuantidadeReservada -= quantidade;
            AtualizarVersao();
        }

        public void ReservarQuantidadeCompra(int quantidade)
        {
            ValidarQuantidadePositiva(quantidade);

            QuantidadeCompradaPendente += quantidade;
            AtualizarVersao();
        }

        public void EfetivarCompra(int quantidade)
        {
            ValidarCompraPendente(quantidade);

            QuantidadeCompradaPendente -= quantidade;
            Estoque += quantidade;
            AtualizarVersao();
        }

        public void CancelarCompraPendente(int quantidade)
        {
            ValidarCompraPendente(quantidade);

            QuantidadeCompradaPendente -= quantidade;
            AtualizarVersao();
        }

        private void ValidarReservaDeVenda(int quantidade)
        {
            ValidarQuantidadePositiva(quantidade);

            if (QuantidadeReservada < quantidade)
            {
                throw new ConflictException(ProdutoExceptions.Produto_ReservaInsuficiente);
            }
        }

        private void ValidarCompraPendente(int quantidade)
        {
            ValidarQuantidadePositiva(quantidade);

            if (QuantidadeCompradaPendente < quantidade)
            {
                throw new ConflictException(ProdutoExceptions.Produto_CompraPendenteInsuficiente);
            }
        }

        private static void ValidarQuantidadePositiva(int quantidade)
        {
            if (quantidade <= 0)
            {
                throw new BadRequestException(ProdutoExceptions.Produto_QuantidadeInvalida);
            }
        }

        private void AtualizarVersao() => Versao = Guid.NewGuid();
    }

}
