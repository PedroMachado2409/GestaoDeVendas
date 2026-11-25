using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Domain.Enum;
using GestaoPedidos.Domain.Exceptions;
using GestaoPedidos.Domain.Exceptions.TituloFinanceiro;

namespace GestaoPedidosTests.Domain
{
    [TestClass]
    public class TituloFinanceiroTests
    {
        private readonly TituloFinanceiro _titulo = new(pessoaId: 1, descricao: "Venda de produtos",
            tipo: TipoTituloFinanceiro.Receber, valorOriginal: 300m, dataVencimento: new DateOnly(2026, 12, 10));

        [TestMethod]
        [DataRow(TipoTituloFinanceiro.Receber)]
        [DataRow(TipoTituloFinanceiro.Pagar)]
        public void Deve_Criar_Titulo_Aberto_Quando_Dados_Forem_Validos(TipoTituloFinanceiro tipo)
        {
            var antes = DateTime.UtcNow;
            var vencimento = new DateOnly(2026, 12, 10);

            var titulo = new TituloFinanceiro(1, "  Venda de produtos  ", tipo, 300m, vencimento);

            Assert.AreEqual(1, titulo.PessoaId);
            Assert.AreEqual("Venda de produtos", titulo.Descricao);
            Assert.AreEqual(tipo, titulo.Tipo);
            Assert.AreEqual(300m, titulo.ValorOriginal);
            Assert.AreEqual(vencimento, titulo.DataVencimento);
            Assert.AreEqual(StatusTituloFinanceiro.Aberto, titulo.Status);
            Assert.AreNotEqual(Guid.Empty, titulo.Versao);
            Assert.IsTrue(titulo.DataCadastro >= antes && titulo.DataCadastro <= DateTime.UtcNow);
            Assert.AreEqual(DateTimeKind.Utc, titulo.DataCadastro.Kind);
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public void Deve_Lancar_Excecao_Quando_Descricao_For_Vazia(string? descricao)
        {
            var excecao = Assert.ThrowsException<BadRequestException>(() =>
                new TituloFinanceiro(1, descricao!, _titulo.Tipo,
                    _titulo.ValorOriginal, _titulo.DataVencimento));

            Assert.AreEqual(TItuloExceptions.Titulo_DescricaoInvalida, excecao.Message);
        }

        [TestMethod]
        public void Deve_Lancar_Excecao_Quando_Descricao_Ultrapassar_Limite()
        {
            var excecao = Assert.ThrowsException<BadRequestException>(() =>
                new TituloFinanceiro(1, new string('a', 201), _titulo.Tipo,
                    _titulo.ValorOriginal, _titulo.DataVencimento));

            Assert.AreEqual(TItuloExceptions.Titulo_DescricaoInvalida, excecao.Message);
        }

        [TestMethod]
        public void Deve_Aceitar_Descricao_Com_200_Caracteres_Apos_Remover_Espacos()
        {
            var descricao = new string('a', 200);

            var titulo = new TituloFinanceiro(1, "  " + descricao + "  ", _titulo.Tipo,
                _titulo.ValorOriginal, _titulo.DataVencimento);

            Assert.AreEqual(descricao, titulo.Descricao);
        }

        [TestMethod]
        public void Deve_Lancar_Excecao_Quando_Tipo_For_Invalido()
        {
            var excecao = Assert.ThrowsException<BadRequestException>(() =>
                new TituloFinanceiro(1, _titulo.Descricao, (TipoTituloFinanceiro)999,
                    _titulo.ValorOriginal, _titulo.DataVencimento));

            Assert.AreEqual(TItuloExceptions.Titulo_TipoInvalido, excecao.Message);
        }

        [TestMethod]
        public void Deve_Lancar_Excecao_Quando_Valor_For_Invalido()
        {
            foreach (var valor in new[] { 0m, -1m, 10.001m, 10_000_000_000_000_000m })
            {
                var excecao = Assert.ThrowsException<BadRequestException>(() =>
                    new TituloFinanceiro(1, _titulo.Descricao, _titulo.Tipo,
                        valor, _titulo.DataVencimento));

                Assert.AreEqual(TItuloExceptions.Titulo_ValorInvalido, excecao.Message);
            }
        }

        [TestMethod]
        public void Deve_Aceitar_Valor_No_Limite_Monetario()
        {
            var titulo = new TituloFinanceiro(1, _titulo.Descricao, _titulo.Tipo,
                9_999_999_999_999_999.99m, _titulo.DataVencimento);

            Assert.AreEqual(9_999_999_999_999_999.99m, titulo.ValorOriginal);
        }

        [TestMethod]
        public void Deve_Permitir_Cadastro_De_Titulo_Com_Vencimento_Passado()
        {
            var vencimento = new DateOnly(2020, 1, 1);

            var titulo = new TituloFinanceiro(1, _titulo.Descricao, _titulo.Tipo, 300m, vencimento);

            Assert.AreEqual(vencimento, titulo.DataVencimento);
            Assert.AreEqual(StatusTituloFinanceiro.Aberto, titulo.Status);
        }

        [TestMethod]
        public void Deve_Cancelar_Titulo_Aberto_E_Atualizar_Versao()
        {
            var versaoAnterior = _titulo.Versao;

            _titulo.Cancelar();

            Assert.AreEqual(StatusTituloFinanceiro.Cancelado, _titulo.Status);
            Assert.AreNotEqual(versaoAnterior, _titulo.Versao);
            Assert.AreEqual(300m, _titulo.ValorOriginal);
        }

        [TestMethod]
        public void Deve_Lancar_Excecao_E_Preservar_Dados_Quando_Titulo_Ja_Estiver_Cancelado()
        {
            _titulo.Cancelar();
            var versaoAnterior = _titulo.Versao;

            var excecao = Assert.ThrowsException<ConflictException>(() =>
                _titulo.Cancelar());

            Assert.AreEqual(TItuloExceptions.Titulo_JaCancelado, excecao.Message);
            Assert.AreEqual(StatusTituloFinanceiro.Cancelado, _titulo.Status);
            Assert.AreEqual(versaoAnterior, _titulo.Versao);
            Assert.AreEqual(300m, _titulo.ValorOriginal);
        }
    }
}
