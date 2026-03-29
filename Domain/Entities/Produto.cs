using GestaoPedidos.Domain.Enum;
using GestaoPedidos.Domain.Exceptions;

namespace GestaoPedidos.Domain.Entities
{
    public class Produto
    {
        public int Id { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string Marca { get; private set; } = string.Empty;
        public decimal Preco { get; private set; }
        public int Estoque { get; private set; } = 0;
        public int QuantidadeReservadaParaVenda { get; private set; } = 0;
        public int QuantidadeEmCompra { get; private set; } = 0;
        public DateTime DataCadastro { get; private set; } = DateTime.UtcNow;
        public bool Ativo { get; private set; } = true;
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
                throw new BadRequestException("Quantidade inválida.");

            if (Estoque < quantidade)
                throw new BadRequestException("Estoque insuficiente.");

            Estoque -= quantidade;
        }

        public void AumentarEstoque(int quantidade)
        {
            if (quantidade <= 0)
                throw new BadRequestException("Quantidade invalida");

            Estoque += quantidade;
        }
        public void FinalizarEstoquePedido(TipoPedido tipoPedido, int quantidade)
        {
            if (tipoPedido == TipoPedido.Venda)
            {
                QuantidadeReservadaParaVenda -= quantidade;
            }
            else if (tipoPedido == TipoPedido.Compra) 
            {
                QuantidadeEmCompra -= quantidade;
                AumentarEstoque(quantidade);
            }
        }

        public void ReservarQuantidades(TipoPedido tipoPedido, int quantidade)
        {
            if(tipoPedido == TipoPedido.Venda)
            {
                QuantidadeReservadaParaVenda += quantidade;
                BaixarEstoque(quantidade);
            } else if (tipoPedido == TipoPedido.Compra)
            {
                QuantidadeEmCompra += quantidade;
            }
        }

        public void CancelarQuantidades(TipoPedido tipoPedido, StatusPedido status, int quantidade)
        {
            if (tipoPedido == TipoPedido.Venda)
            {
                QuantidadeReservadaParaVenda -= quantidade;
                AumentarEstoque(quantidade);
            }
            else if (tipoPedido == TipoPedido.Compra)
            {
                QuantidadeEmCompra -= quantidade;
                if(status == StatusPedido.Finalizado)
                {
                    BaixarEstoque(quantidade);
                }
            }
        }
    }
}
