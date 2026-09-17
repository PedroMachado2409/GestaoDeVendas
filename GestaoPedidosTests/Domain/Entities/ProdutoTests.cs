using FluentAssertions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Domain.Exceptions;

namespace GestaoPedidosTests.Domain.Entities
{
    [TestClass]
    public class ProdutoTests
    {
        [TestMethod]
        public void Reservar_Deve_Reduzir_Estoque_E_Aumentar_Reserva()
        {
            var produto = new Produto("Produto", "Marca", 10, 25m);

            produto.ReservarQuantidadeVenda(3);

            produto.Estoque.Should().Be(7);
            produto.QuantidadeReservada.Should().Be(3);
        }

        [TestMethod]
        public void Confirmar_Reserva_Deve_Manter_Estoque_E_Zerar_Reserva()
        {
            var produto = new Produto("Produto", "Marca", 10, 25m);
            produto.ReservarQuantidadeVenda(3);

            produto.ConfirmarReserva(3);

            produto.Estoque.Should().Be(7);
            produto.QuantidadeReservada.Should().Be(0);
        }

        [TestMethod]
        public void Liberar_Reserva_Deve_Devolver_Quantidade_Ao_Estoque()
        {
            var produto = new Produto("Produto", "Marca", 10, 25m);
            produto.ReservarQuantidadeVenda(3);

            produto.LiberarReserva(3);

            produto.Estoque.Should().Be(10);
            produto.QuantidadeReservada.Should().Be(0);
        }

        [TestMethod]
        public void Nao_Deve_Reservar_Acima_Do_Estoque()
        {
            var produto = new Produto("Produto", "Marca", 2, 25m);

            var act = () => produto.ReservarQuantidadeVenda(3);

            act.Should().Throw<ConflictException>();
        }

        [TestMethod]
        public void Nao_Deve_Liberar_Mais_Do_Que_Foi_Reservado()
        {
            var produto = new Produto("Produto", "Marca", 10, 25m);
            produto.ReservarQuantidadeVenda(2);

            var act = () => produto.LiberarReserva(3);

            act.Should().Throw<ConflictException>();
            produto.Estoque.Should().Be(8);
            produto.QuantidadeReservada.Should().Be(2);
        }
    }
}
