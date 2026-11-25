

using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Domain.Enum;

namespace GestaoPedidosTests.Domain
{
    [TestClass]
    public class PessoaTests
    {

        private readonly Pessoa _pessoa = new(
            nome: "Pedro",
            email: "PedrO@GMail.com",
            cpf: "12345678910",
            tipoPessoa: TipoPessoa.Cliente
           );

        [TestMethod]
        public void Deve_Cadastrar_Pessoa_Quando_Dados_Forem_Validos_E_Iniciar_Ativa()
        {
            var nome = "Maria";
            var email = "maria@exemplo.com";
            var cpf = "52998224725";
            var tipoPessoa = TipoPessoa.Fornecedor;


            var pessoa = new Pessoa(nome, email, cpf, tipoPessoa);

            Assert.AreEqual(nome, pessoa.Nome);
            Assert.AreEqual(email, pessoa.Email);
            Assert.AreEqual(cpf, pessoa.Cpf);
            Assert.AreEqual(tipoPessoa, pessoa.TipoPessoa);
            Assert.IsTrue(pessoa.Ativo);
        }

        [TestMethod]
        public void Deve_Normalizar_Email_Quando_Tiver_Letras_Maiusculas()
        {
            var email = "  MARIA@EXEMPLO.COM  ";
            var emailNormalizado = Pessoa.NormalizarEmail(email);

            Assert.AreEqual("maria@exemplo.com", emailNormalizado);
        }

        [TestMethod]
        public void Deve_Normalizar_Cpf_Quando_Tiver_Mascara()
        {
            var cpf = "111.456.321-12 ";
            var cpfNormalizado = Pessoa.NormalizarCpf(cpf);
            Assert.AreEqual("11145632112", cpfNormalizado);
        }

        [TestMethod]
        public void Deve_Inativar_Pessoa_Quando_Esta_Ativa()
        {
            _pessoa.Inativar();
            Assert.IsFalse(_pessoa.Ativo);
        }

        [TestMethod]
        public void Deve_Ativar_Pessoa_Quando_Esta_Inativo()
        {
            _pessoa.Inativar();
            _pessoa.Ativar();

            Assert.IsTrue(_pessoa.Ativo);
        }

        [TestMethod]
        public void Deve_Atualizar_Pessoa_Quando_Dados_Forem_Validos()
        {
            _pessoa.Atualizar("Carlos", "Carlos@gmail.com", "03251228095", TipoPessoa.Fornecedor);

            Assert.AreEqual("Carlos", _pessoa.Nome);
            Assert.AreEqual("carlos@gmail.com", _pessoa.Email);
            Assert.AreEqual("03251228095", _pessoa.Cpf);
            Assert.AreEqual(TipoPessoa.Fornecedor, _pessoa.TipoPessoa);
        }

    }
}
