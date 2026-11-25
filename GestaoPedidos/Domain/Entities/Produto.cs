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
            ValidarDados(nome, marca, estoque, preco);
            Nome = nome.Trim();
            Marca = marca.Trim();
            Estoque = estoque;
            Preco = preco;
            QuantidadeReservada = 0;
            QuantidadeCompradaPendente = 0;
        }

        public void Atualizar(string nome, string marca, int estoque, decimal preco)
        {
            ValidarDados(nome, marca, estoque, preco);
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

            var novaReserva = SomarQuantidades(QuantidadeReservada, quantidade);
            QuantidadeReservada = novaReserva;
            Estoque -= quantidade;
            AtualizarVersao();
        }

        public void AjusteEstoque(int quantidade)
        {
            if (quantidade == 0)
            {
                throw new BadRequestException(ProdutoExceptions.Produto_AjusteNaoPodeSerZero);
            }
            var novoEstoque = (long)Estoque + quantidade;
            if (novoEstoque < 0)
            {
                throw new BadRequestException(ProdutoExceptions.Produto_EstoqueNaoPodeSerNegativo);
            }

            Estoque = SomarQuantidades(Estoque, quantidade);
            AtualizarVersao();

        }

        public void CancelarReserva(int quantidade)
        {
            ValidarReservaDeVenda(quantidade);

            var novoEstoque = SomarQuantidades(Estoque, quantidade);
            QuantidadeReservada -= quantidade;
            Estoque = novoEstoque;
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

            QuantidadeCompradaPendente = SomarQuantidades(QuantidadeCompradaPendente, quantidade);
            AtualizarVersao();
        }

        public void EfetivarCompra(int quantidade)
        {
            ValidarCompraPendente(quantidade);

            var novoEstoque = SomarQuantidades(Estoque, quantidade);
            QuantidadeCompradaPendente -= quantidade;
            Estoque = novoEstoque;
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

        public static void ValidarQuantidadePositiva(int quantidade)
        {
            if (quantidade <= 0)
            {
                throw new BadRequestException(ProdutoExceptions.Produto_QuantidadeInvalida);
            }
        }

        private static void ValidarDados(string nome, string marca, int estoque, decimal preco)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(nome);
            ArgumentException.ThrowIfNullOrWhiteSpace(marca);
            ArgumentOutOfRangeException.ThrowIfNegative(estoque);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(preco);
        }

        private static int SomarQuantidades(int saldo, int quantidade)
        {
            var resultado = (long)saldo + quantidade;
            if (resultado > int.MaxValue)
            {
                throw new BadRequestException("A quantidade ultrapassa o limite suportado pelo estoque.");
            }

            return (int)resultado;
        }

        private void AtualizarVersao() => Versao = Guid.NewGuid();
    }

}
