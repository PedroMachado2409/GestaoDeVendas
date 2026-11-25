

using GestaoPedidos.Domain.Entities.Pedidos;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Pedidos;

namespace GestaoPedidosTests.Domain.Pedidos
{
    [TestClass]
    public class PedidoItemTests
    {

        private readonly PedidoItem _item = new PedidoItem(produtoId: 1, preco: 10m, quantidade: 10);

        [TestMethod]
        public void Deve_Cadastrar_PedidoItem_Quando_Quantidade_For_Maior_Que_Zero()
        {
            var item = new PedidoItem(_item.ProdutoId, _item.Preco, _item.Quantidade);

            Assert.AreEqual(item.ProdutoId, _item.ProdutoId);
            Assert.AreEqual(item.Preco, _item.Preco);
            Assert.AreEqual(item.Quantidade, _item.Quantidade);

        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(-12)]
        public void Deve_Lancar_Excecao_Quando_Quantidade_For_Invalida(int quantidade)
        {
            var excecao = Assert.ThrowsException<BadRequestException>(() =>
              new PedidoItem(_item.ProdutoId, _item.Preco, quantidade));

            Assert.AreEqual(PedidosExceptions.Pedido_QuantidadeInvalida, excecao.Message);

        }

        [TestMethod]
        public void Deve_Alterar_Quantidade_Quando_For_Maior_Que_Zero()
        {
            _item.AlterarQuantidade(12);

            Assert.AreEqual(1, _item.ProdutoId);
            Assert.AreEqual(12, _item.Quantidade);
            Assert.AreEqual(10, _item.Preco);
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(-12)]
        public void Deve_Lancar_Excecao_Quando_Quantidade_Alterada_For_Invalida(int quantidade)
        {
            var excecao = Assert.ThrowsException<BadRequestException>(() =>
                _item.AlterarQuantidade(quantidade));

            Assert.AreEqual(PedidosExceptions.Pedido_QuantidadeInvalida, excecao.Message);
            Assert.AreEqual(10, _item.Quantidade);
            Assert.AreEqual(10, _item.Preco);
            Assert.AreEqual(1, _item.ProdutoId);
        }

        [TestMethod]
        public void Deve_Calcular_SubTotal_Conforme_Preco_E_Quantidade()
        {
            var item = new PedidoItem(produtoId: 1, preco: 10.50m, quantidade: 3);

            Assert.AreEqual(31.50m, item.SubTotal);
        }

        [TestMethod]
        public void Deve_Atualizar_SubTotal_Quando_Alterar_Quantidade()
        {
            _item.AlterarQuantidade(12);

            Assert.AreEqual(120m, _item.SubTotal);
        }
    }
}
