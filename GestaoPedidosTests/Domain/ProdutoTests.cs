
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Produtos;


namespace GestaoPedidosTests.Domain
{
    [TestClass]
    public class ProdutoTests
    {

            private readonly Produto _produto = new(
            nome: "Mouse",
            marca: "Redragon",
            estoque: 10,
            preco: 20m
        );


        [TestMethod]
        public void ReservarQuantidadeVenda_ComEstoqueDisponivel_DeveAtualizarSaldoEReserva()
        {
            
            _produto.ReservarQuantidadeVenda(3);

           
            Assert.AreEqual(7, _produto.Estoque);
            Assert.AreEqual(3, _produto.QuantidadeReservada);
        }

        [TestMethod]
        public void ReservarQuantidadeVenda_ComEstoqueExato_DeveZerarSaldoDisponivel()
        {
            _produto.ReservarQuantidadeVenda(10);

            Assert.AreEqual(0, _produto.Estoque);
            Assert.AreEqual(10, _produto.QuantidadeReservada);
        }

        [TestMethod]
        public void ReservarQuantidadeVenda_ComEstoqueInsuficiente_DeveLancarExcecao()
        {
            var excecao = Assert.ThrowsException<ConflictException>(
                () => _produto.ReservarQuantidadeVenda(11));

            Assert.AreEqual(ProdutoExceptions.Produto_EstoqueInsuficiente, excecao.Message);
            Assert.AreEqual(10, _produto.Estoque);
            Assert.AreEqual(0, _produto.QuantidadeReservada);
        }

        [TestMethod]
        public void ConfirmarReservaDeVenda_Deve_AtualizarSaldoEReserva()
        {
            _produto.ReservarQuantidadeVenda(10);
            _produto.ConfirmarReserva(7);

            Assert.AreEqual(0, _produto.Estoque);
            Assert.AreEqual(3, _produto.QuantidadeReservada);
        }

        [TestMethod]
        public void CancelarReservaDeVenda_DeveAtualizarSaldoEReserva()
        {
            _produto.ReservarQuantidadeVenda(5);
            _produto.CancelarReserva(5);

            Assert.AreEqual(10, _produto.Estoque);
            Assert.AreEqual(0, _produto.QuantidadeReservada);
        }

        [TestMethod]
        public void InformarQuantidadeNegativa_ReservaDeVenda_Deve_Lancar_Excecao()
        {
            var excecao = Assert.ThrowsException<BadRequestException>(() =>
                _produto.ReservarQuantidadeVenda(-2));

            Assert.AreEqual(ProdutoExceptions.Produto_QuantidadeInvalida, excecao.Message);
            Assert.AreEqual(10, _produto.Estoque);
            Assert.AreEqual(0, _produto.QuantidadeReservada);
        }


        [TestMethod]
        public void ReservarQuantidadeCompra_DeveAtualizarRerserva()
        {
            _produto.ReservarQuantidadeCompra(2);

            Assert.AreEqual(2, _produto.QuantidadeCompradaPendente);
            Assert.AreEqual(10, _produto.Estoque);
        }

        [TestMethod]
        public void EfetivarQuantidadeDeCompra_DeveAtualizarReservaESaldo()
        {
            _produto.ReservarQuantidadeCompra(2);
            _produto.EfetivarCompra(2);

            Assert.AreEqual(0, _produto.QuantidadeCompradaPendente);
            Assert.AreEqual(12, _produto.Estoque);
        }

        [TestMethod]
        public void CancelarQuantidadeDeCompra_DeveAtualizarReserva()
        {
            _produto.ReservarQuantidadeCompra(11);
            _produto.CancelarCompraPendente(11);

            Assert.AreEqual(0, _produto.QuantidadeCompradaPendente);
            Assert.AreEqual(10, _produto.Estoque);
        }

        [TestMethod]
        public void InformarQuantidadeNegativa_Deve_Lancar_Excecao()
        {
            var excecao = Assert.ThrowsException<BadRequestException>(
                () => Produto.ValidarQuantidadePositiva(-12));

            Assert.AreEqual(ProdutoExceptions.Produto_QuantidadeInvalida, excecao.Message);

        }

        [TestMethod]
        public void ReservarQuantidadeInsuficienteDeVenda_Deve_Lancar_Excecao()
        {
            _produto.ReservarQuantidadeVenda(1);
            var excecao = Assert.ThrowsException<ConflictException>(
                () => _produto.ConfirmarReserva(20));

            Assert.AreEqual(1, _produto.QuantidadeReservada);
            Assert.AreEqual(9, _produto.Estoque);
            Assert.AreEqual(ProdutoExceptions.Produto_ReservaInsuficiente, excecao.Message);
        }

        [TestMethod]
        public void InformarQuantidadeZero_ReservaDeVenda_Deve_Lancar_Excecao()
        {
            var excecao = Assert.ThrowsException<BadRequestException>(
                () => _produto.ReservarQuantidadeVenda(0));

            Assert.AreEqual(ProdutoExceptions.Produto_QuantidadeInvalida, excecao.Message);
            Assert.AreEqual(10, _produto.Estoque);
            Assert.AreEqual(0, _produto.QuantidadeReservada);
        }

        [TestMethod]
        public void InformarQuantidadeZero_ReservaDeCompra_Deve_Lancar_Excecao()
        {
            var excecao = Assert.ThrowsException<BadRequestException>(
                () => _produto.ReservarQuantidadeCompra(0));

            Assert.AreEqual(ProdutoExceptions.Produto_QuantidadeInvalida, excecao.Message);
            Assert.AreEqual(10, _produto.Estoque);
            Assert.AreEqual(0, _produto.QuantidadeCompradaPendente);
        }

        [TestMethod]
        public void InformarQuantidadeNegativa_ReservaDeCompra_Deve_Lancar_Excecao()
        {
            var excecao = Assert.ThrowsException<BadRequestException>(
                () => _produto.ReservarQuantidadeCompra(-2));

            Assert.AreEqual(ProdutoExceptions.Produto_QuantidadeInvalida, excecao.Message);
            Assert.AreEqual(10, _produto.Estoque);
            Assert.AreEqual(0, _produto.QuantidadeCompradaPendente);
        }

        [TestMethod]
        public void CancelarReservaDeVenda_ComReservaInsuficiente_Deve_Lancar_Excecao()
        {
            _produto.ReservarQuantidadeVenda(5);

            var excecao = Assert.ThrowsException<ConflictException>(
                () => _produto.CancelarReserva(6));

            Assert.AreEqual(ProdutoExceptions.Produto_ReservaInsuficiente, excecao.Message);
            Assert.AreEqual(5, _produto.Estoque);
            Assert.AreEqual(5, _produto.QuantidadeReservada);
        }

        [TestMethod]
        public void EfetivarCompra_ComCompraPendenteInsuficiente_Deve_Lancar_Excecao()
        {
            _produto.ReservarQuantidadeCompra(5);

            var excecao = Assert.ThrowsException<ConflictException>(
                () => _produto.EfetivarCompra(6));

            Assert.AreEqual(ProdutoExceptions.Produto_CompraPendenteInsuficiente, excecao.Message);
            Assert.AreEqual(10, _produto.Estoque);
            Assert.AreEqual(5, _produto.QuantidadeCompradaPendente);
        }

        [TestMethod]
        public void CancelarCompra_ComCompraPendenteInsuficiente_Deve_Lancar_Excecao()
        {
            _produto.ReservarQuantidadeCompra(5);

            var excecao = Assert.ThrowsException<ConflictException>(
                () => _produto.CancelarCompraPendente(6));

            Assert.AreEqual(ProdutoExceptions.Produto_CompraPendenteInsuficiente, excecao.Message);
            Assert.AreEqual(10, _produto.Estoque);
            Assert.AreEqual(5, _produto.QuantidadeCompradaPendente);
        }

        [TestMethod]
        public void ReservarQuantidadeVenda_ComReservasAcumuladas_DeveAtualizarSaldoEReserva()
        {
            _produto.ReservarQuantidadeVenda(2);
            _produto.ReservarQuantidadeVenda(3);

            Assert.AreEqual(5, _produto.Estoque);
            Assert.AreEqual(5, _produto.QuantidadeReservada);
        }

        [TestMethod]
        public void ReservarQuantidadeCompra_ComReservasAcumuladas_DeveAtualizarCompraPendente()
        {
            _produto.ReservarQuantidadeCompra(2);
            _produto.ReservarQuantidadeCompra(3);

            Assert.AreEqual(5, _produto.QuantidadeCompradaPendente);
            Assert.AreEqual(10, _produto.Estoque);
        }

        [TestMethod]
        public void AjustarEstoque_ComQuantidadePositiva_DeveAumentarSaldo()
        {
            _produto.AjusteEstoque(5);

            Assert.AreEqual(15, _produto.Estoque);
        }

        [TestMethod]
        public void AjustarEstoque_ComQuantidadeNegativaPermitida_DeveDiminuirSaldo()
        {
            _produto.AjusteEstoque(-3);

            Assert.AreEqual(7, _produto.Estoque);
        }

        [TestMethod]
        public void AjustarEstoque_ComReducaoExata_DeveZerarSaldo()
        {
            _produto.AjusteEstoque(-10);

            Assert.AreEqual(0, _produto.Estoque);
        }

        [TestMethod]
        public void AjustarEstoque_ComQuantidadeZero_Deve_Lancar_Excecao()
        {
            var excecao = Assert.ThrowsException<BadRequestException>(
                () => _produto.AjusteEstoque(0));

            Assert.AreEqual(ProdutoExceptions.Produto_AjusteNaoPodeSerZero, excecao.Message);
            Assert.AreEqual(10, _produto.Estoque);
        }

        [TestMethod]
        public void AjustarEstoque_ComReducaoAcimaDoSaldo_Deve_Lancar_Excecao()
        {
            var excecao = Assert.ThrowsException<BadRequestException>(
                () => _produto.AjusteEstoque(-11));

            Assert.AreEqual(ProdutoExceptions.Produto_EstoqueNaoPodeSerNegativo, excecao.Message);
            Assert.AreEqual(10, _produto.Estoque);
        }

        [TestMethod]
        public void AtualizarProduto_ComDadosValidos_DeveAtualizarDados()
        {
            _produto.Atualizar("Teclado", "Logitech", 15, 50m);

            Assert.AreEqual("Teclado", _produto.Nome);
            Assert.AreEqual("Logitech", _produto.Marca);
            Assert.AreEqual(15, _produto.Estoque);
            Assert.AreEqual(50m, _produto.Preco);
        }

        [TestMethod]
        public void AtualizarProduto_ComEstoqueNegativo_Deve_Lancar_Excecao()
        {
            var versaoAnterior = _produto.Versao;

            var excecao = Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => _produto.Atualizar("Teclado", "Logitech", -1, 50m));

            Assert.AreEqual("estoque", excecao.ParamName);
            Assert.AreEqual("Mouse", _produto.Nome);
            Assert.AreEqual("Redragon", _produto.Marca);
            Assert.AreEqual(10, _produto.Estoque);
            Assert.AreEqual(20m, _produto.Preco);
            Assert.AreEqual(versaoAnterior, _produto.Versao);
        }

        [TestMethod]
        public void InativarProduto_DeveAtualizarStatus()
        {
            _produto.Inativar();

            Assert.IsFalse(_produto.Ativo);
        }

        [TestMethod]
        public void AtivarProduto_ComProdutoInativo_DeveAtualizarStatus()
        {
            _produto.Inativar();
            _produto.Ativar();

            Assert.IsTrue(_produto.Ativo);
        }

        [TestMethod]
        public void ReservarQuantidadeVenda_DeveAtualizarVersao()
        {
            var versaoAnterior = _produto.Versao;

            _produto.ReservarQuantidadeVenda(3);

            Assert.AreNotEqual(versaoAnterior, _produto.Versao);
        }





    }
}
