
using GestaoPedidos.Domain.Entities.Pedidos;
using GestaoPedidos.Domain.Enum;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Pedidos;

namespace GestaoPedidosTests.Domain.Pedidos
{

    [TestClass]
    public class PedidoTests
    {
        private readonly PedidoItem _item = new PedidoItem(produtoId: 1, preco: 10m, quantidade: 10);

        private readonly Pedido _pedido;
        public PedidoTests()
        {
            _pedido = new Pedido(clienteId: 1, itens: new List<PedidoItem> { _item }, usuarioId: 1,
                valorTotal: _item.SubTotal, tipoMovimentacao: TipoMovimentacao.Saida);
        }

        [TestMethod]
        public void Cadastrar_Pedido_Com_Dados_Validos_Deve_Cadastrar()
        {
            var itens = new List<PedidoItem> { _item };
            var pedido = new Pedido(1, itens, 1, 100m, TipoMovimentacao.Saida);

            Assert.AreEqual(1, pedido.ClienteId);
            Assert.AreEqual(1, pedido.UsuarioId);
            Assert.AreEqual(100m, pedido.ValorTotal);
            Assert.AreEqual(TipoMovimentacao.Saida, pedido.TipoMovimentacao);
            Assert.AreEqual(1, pedido.Itens.Count);
            Assert.AreEqual(_item.ProdutoId, pedido.Itens.First().ProdutoId);
            Assert.AreEqual(_item.Preco, pedido.Itens.First().Preco);
            Assert.AreEqual(_item.Quantidade, pedido.Itens.First().Quantidade);
        }

        [TestMethod]
        public void Cadastrar_Pedido_Sem_Item_Deve_Lancar_Excecao()
        {
            var excecao = Assert.ThrowsException<BadRequestException>(() =>
                new Pedido(clienteId: 1, itens: null!, usuarioId: 1, valorTotal: 10, TipoMovimentacao.Saida));

            Assert.AreEqual(PedidosExceptions.Pedido_ItemObrigatório, excecao.Message);

        }

        [TestMethod]
        public void Deve_Finalizar_Pedido_Quando_Aberto()
        {
            _pedido.Finalizar();
            Assert.AreEqual(StatusPedido.Finalizado, _pedido.Status);

        }

        [TestMethod]
        public void Deve_Lancar_Excecao_Ao_Finalizar_Pedido_Ja_Finalizado()
        {
            _pedido.Finalizar();

            var excecao = Assert.ThrowsException<BadRequestException>(() =>
                _pedido.Finalizar());

            Assert.AreEqual(PedidosExceptions.Pedido_Finalizado, excecao.Message);
            Assert.AreEqual(StatusPedido.Finalizado, _pedido.Status);
        }

        [TestMethod]
        public void Deve_Lancar_Excecao_Ao_Finalizar_Pedido_Ja_Cancelado()
        {
            _pedido.Cancelar();

            var excecao = Assert.ThrowsException<BadRequestException>(() =>
                _pedido.Finalizar());

            Assert.AreEqual(PedidosExceptions.Pedido_Cancelado, excecao.Message);
            Assert.AreEqual(StatusPedido.Cancelado, _pedido.Status);
        }

        [TestMethod]
        public void Deve_Cancelar_Pedido_Quando_Aberto()
        {
            _pedido.Cancelar();
            Assert.AreEqual(StatusPedido.Cancelado, _pedido.Status);
        }

        [TestMethod]
        public void Deve_Lancar_Excecao_Ao_Cancelar_Pedido_Ja_Cancelado()
        {
            _pedido.Cancelar();

            var excecao = Assert.ThrowsException<ConflictException>(() =>
                _pedido.Cancelar());

            Assert.AreEqual(PedidosExceptions.Pedido_NaoPodeCancelar, excecao.Message);
            Assert.AreEqual(StatusPedido.Cancelado, _pedido.Status);
        }

        [TestMethod]
        public void Deve_Lancar_Excecao_Ao_Cancelar_Pedido_Ja_Finalizado()
        {
            _pedido.Finalizar();

            var excecao = Assert.ThrowsException<ConflictException>(() =>
                _pedido.Cancelar());

            Assert.AreEqual(PedidosExceptions.Pedido_NaoPodeCancelar, excecao.Message);
            Assert.AreEqual(StatusPedido.Finalizado, _pedido.Status);
        }

        [TestMethod]
        public void Deve_Lancar_Excecao_Ao_Atualizar_Pedido_Com_Status_Finalizado()
        {
            _pedido.Finalizar();
            var excecao = Assert.ThrowsException<BadRequestException>(() =>
                 _pedido.AtualizarItem(produtoId: 1, novaQuantidade: 20));

            Assert.AreEqual(PedidosExceptions.Pedido_NaoPodeSerAlterado, excecao.Message);
            Assert.AreEqual(10, _item.Quantidade);

        }


        [TestMethod]
        public void Deve_Lancar_Excecao_Ao_Atualizar_Pedido_Com_Status_Cancelado()
        {
            _pedido.Cancelar();
            var excecao = Assert.ThrowsException<BadRequestException>(() =>
                 _pedido.AtualizarItem(produtoId: 1, novaQuantidade: 20));

            Assert.AreEqual(PedidosExceptions.Pedido_NaoPodeSerAlterado, excecao.Message);
            Assert.AreEqual(10, _item.Quantidade);

        }

        [TestMethod]
        public void Deve_Lancar_Excecao_Quando_Item_Nao_Existir_No_Pedido()
        {
            var excecao = Assert.ThrowsException<NotFoundException>(() =>
                 _pedido.AtualizarItem(produtoId: 999, novaQuantidade: 10));

            Assert.AreEqual(PedidosExceptions.Pedido_NaoEncontrado, excecao.Message);

        }

        [TestMethod]
        public void Deve_Iniciar_Pedido_Com_Status_Aberto()
        {
            Assert.AreEqual(StatusPedido.Aberto, _pedido.Status);
        }

        [TestMethod]
        public void Cadastrar_Pedido_Com_Lista_De_Itens_Vazia_Deve_Lancar_Excecao()
        {
            var itens = new List<PedidoItem>();

            var excecao = Assert.ThrowsException<BadRequestException>(() =>
                new Pedido(clienteId: 1, itens: itens, usuarioId: 1,
                    valorTotal: 0m, tipoMovimentacao: TipoMovimentacao.Saida));

            Assert.AreEqual(PedidosExceptions.Pedido_ItemObrigatório, excecao.Message);
        }

        [TestMethod]
        public void Deve_Atualizar_Quantidade_Do_Item_E_ValorTotal_Quando_Pedido_Estiver_Aberto()
        {
            _pedido.AtualizarItem(produtoId: 1, novaQuantidade: 12);

            Assert.AreEqual(12, _pedido.Itens.Single().Quantidade);
            Assert.AreEqual(120m, _pedido.Itens.Single().SubTotal);
            Assert.AreEqual(120m, _pedido.ValorTotal);
            Assert.AreEqual(1, _pedido.Itens.Count);
            Assert.AreEqual(StatusPedido.Aberto, _pedido.Status);
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(-12)]
        public void Deve_Remover_Item_E_Atualizar_ValorTotal_Quando_Quantidade_For_Zero_Ou_Negativa(int quantidade)
        {
            var outroItem = new PedidoItem(produtoId: 2, preco: 20m, quantidade: 3);
            var itens = new List<PedidoItem> { _item, outroItem };

            var pedido = new Pedido(clienteId: 1, itens: itens, usuarioId: 1,
                valorTotal: 160m, tipoMovimentacao: TipoMovimentacao.Saida);

            pedido.AtualizarItem(produtoId: 1, novaQuantidade: quantidade);

            Assert.AreEqual(1, pedido.Itens.Count);
            Assert.AreEqual(outroItem.ProdutoId, pedido.Itens.Single().ProdutoId);
            Assert.AreEqual(3, outroItem.Quantidade);
            Assert.AreEqual(60m, pedido.ValorTotal);
            Assert.AreEqual(StatusPedido.Aberto, pedido.Status);
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(-12)]
        public void Deve_Lancar_Excecao_E_Manter_Dados_Ao_Tentar_Remover_Ultimo_Item(int quantidade)
        {
            var excecao = Assert.ThrowsException<BadRequestException>(() =>
                _pedido.AtualizarItem(produtoId: 1, novaQuantidade: quantidade));

            Assert.AreEqual(PedidosExceptions.Pedido_ItemObrigatório, excecao.Message);
            Assert.AreEqual(1, _pedido.Itens.Count);
            Assert.AreEqual(_item.ProdutoId, _pedido.Itens.Single().ProdutoId);
            Assert.AreEqual(10, _item.Quantidade);
            Assert.AreEqual(100m, _pedido.ValorTotal);
            Assert.AreEqual(StatusPedido.Aberto, _pedido.Status);
        }


        [TestMethod]
        public void Deve_Preservar_Pedido_Quando_Item_Original_For_Alterado()
        {
            _item.AlterarQuantidade(20);

            Assert.AreEqual(10, _pedido.Itens.Single().Quantidade);
            Assert.AreEqual(100m, _pedido.ValorTotal);
        }

        [TestMethod]
        public void Deve_Impedir_Alteracao_Direta_De_Item_Vinculado_Ao_Pedido()
        {
            Assert.ThrowsException<BadRequestException>(() =>
                _pedido.Itens.Single().AlterarQuantidade(20));

            Assert.AreEqual(10, _pedido.Itens.Single().Quantidade);
            Assert.AreEqual(100m, _pedido.ValorTotal);
        }

        [TestMethod]
        public void Deve_Preservar_Item_E_Total_Quando_Pedido_Estiver_Finalizado()
        {
            _pedido.Finalizar();

            Assert.ThrowsException<BadRequestException>(() =>
                _pedido.Itens.Single().AlterarQuantidade(20));

            Assert.AreEqual(10, _pedido.Itens.Single().Quantidade);
            Assert.AreEqual(100m, _pedido.ValorTotal);
            Assert.AreEqual(StatusPedido.Finalizado, _pedido.Status);
        }
    }
}
