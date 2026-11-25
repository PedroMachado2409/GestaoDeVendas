using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Domain.Enum;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.Produtos;

namespace GestaoPedidosTests.Domain
{
    [TestClass]
    public class MovimentacaoEstoqueTests
    {
        private readonly MovimentacaoEstoque _movimentacao = new MovimentacaoEstoque(
            produtoId: 1, produtoNome: "Mouse", quantidade: 10, idOrigem: 1,
            origemMovimentacao: OrigemMovimentacao.PedidoDeVenda,
            tipoMovimentacao: TipoMovimentacao.Saida, observacao: "Venda de produtos");

        [TestMethod]
        public void Deve_Cadastrar_Movimentacao_Com_Dados_Validos_E_Preencher_Data()
        {
            var antes = DateTime.UtcNow;

            var movimentacao = new MovimentacaoEstoque(
                produtoId: _movimentacao.ProdutoId, produtoNome: _movimentacao.ProdutoNome,
                quantidade: _movimentacao.Quantidade, idOrigem: 1,
                origemMovimentacao: _movimentacao.OrigemMovimentacao,
                tipoMovimentacao: _movimentacao.TipoMovimentacao, observacao: _movimentacao.Observacao);

            var depois = DateTime.UtcNow;

            Assert.AreEqual(_movimentacao.ProdutoId, movimentacao.ProdutoId);
            Assert.AreEqual(_movimentacao.ProdutoNome, movimentacao.ProdutoNome);
            Assert.AreEqual(_movimentacao.Quantidade, movimentacao.Quantidade);
            Assert.AreEqual(_movimentacao.IdOrigem, movimentacao.IdOrigem);
            Assert.AreEqual(_movimentacao.OrigemMovimentacao, movimentacao.OrigemMovimentacao);
            Assert.AreEqual(_movimentacao.TipoMovimentacao, movimentacao.TipoMovimentacao);
            Assert.AreEqual(_movimentacao.Observacao, movimentacao.Observacao);
            Assert.IsTrue(movimentacao.DataMovimentacao >= antes);
            Assert.IsTrue(movimentacao.DataMovimentacao <= depois);
            Assert.AreEqual(DateTimeKind.Utc, movimentacao.DataMovimentacao.Kind);
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(-12)]
        public void Deve_Lancar_Excecao_Quando_Quantidade_For_Invalida(int quantidade)
        {
            var excecao = Assert.ThrowsException<BadRequestException>(() =>
                new MovimentacaoEstoque(
                    produtoId: _movimentacao.ProdutoId, produtoNome: _movimentacao.ProdutoNome,
                    quantidade: quantidade, idOrigem: 1,
                    origemMovimentacao: _movimentacao.OrigemMovimentacao,
                    tipoMovimentacao: _movimentacao.TipoMovimentacao, observacao: _movimentacao.Observacao));

            Assert.AreEqual(ProdutoExceptions.Produto_QuantidadeInvalida, excecao.Message);
        }

        [TestMethod]
        public void Deve_Remover_Espacos_Do_Nome_Do_Produto_Ao_Cadastrar_Movimentacao()
        {
            var movimentacao = new MovimentacaoEstoque(
                produtoId: _movimentacao.ProdutoId, produtoNome: "   Mouse   ",
                quantidade: _movimentacao.Quantidade, idOrigem: 1,
                origemMovimentacao: _movimentacao.OrigemMovimentacao,
                tipoMovimentacao: _movimentacao.TipoMovimentacao, observacao: _movimentacao.Observacao);

            Assert.AreEqual("Mouse", movimentacao.ProdutoNome);
        }

        [TestMethod]
        public void Deve_Remover_Espacos_Da_Observacao_Ao_Cadastrar_Movimentacao()
        {
            var movimentacao = new MovimentacaoEstoque(
                produtoId: _movimentacao.ProdutoId, produtoNome: _movimentacao.ProdutoNome,
                quantidade: _movimentacao.Quantidade, idOrigem: 1,
                origemMovimentacao: _movimentacao.OrigemMovimentacao,
                tipoMovimentacao: _movimentacao.TipoMovimentacao, observacao: "   Venda de produtos   ");

            Assert.AreEqual("Venda de produtos", movimentacao.Observacao);
        }
    }
}
