
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Domain.Enum;

namespace GestaoPedidosTests.Domain
{
    [TestClass]
    public class UsuarioTests
    {
        private readonly Usuario _usuario = new Usuario(nome: "Pedro", email: "pedro@gmail.com",
            senha: "senhaMuitoForte!12", role: UserRole.Admin);

        [TestMethod]
        public void Deve_Cadastrar_Usuario_Quando_Dados_Forem_Validos_E_Iniciar_Ativo()
        {
            var nome = _usuario.Nome;
            var email = _usuario.Email;
            var senha = _usuario.Senha;
            var role = _usuario.Role;

            var usuario = new Usuario(nome, email, senha, role);

            Assert.AreEqual(nome, usuario.Nome);
            Assert.AreEqual(email, usuario.Email);
            Assert.AreEqual(senha, usuario.Senha);
            Assert.AreEqual(role, usuario.Role);
        }

        [TestMethod]
        public void Deve_Alterar_Senha_E_Revogar_Token()
        {
            var versaoAnterior = _usuario.VersaoToken;
            _usuario.AlterarSenha("NovaSenha");

            Assert.AreNotEqual(versaoAnterior, _usuario.VersaoToken);
            Assert.AreEqual("NovaSenha", _usuario.Senha);

        }

        [TestMethod]
        public void Deve_Alterar_Role_E_Revogar_Token()
        {
            var versaoAnterior = _usuario.VersaoToken;
            _usuario.AlterarRole(UserRole.Vendedor);

            Assert.AreNotEqual(versaoAnterior, _usuario.VersaoToken);
            Assert.AreEqual(UserRole.Vendedor, _usuario.Role);

        }

        [TestMethod]
        public void Deve_Inativar_Quando_Ativo_E_Revogar_Token()
        {
            var versaoAnterior = _usuario.VersaoToken;
            _usuario.Inativar();

            Assert.AreNotEqual(versaoAnterior, _usuario.VersaoToken);
            Assert.IsFalse(_usuario.Ativo);
        }

        [TestMethod]
        public void Deve_Ativar_Quando_Inativo_E_Revogar_Token()
        {
            _usuario.Inativar();
            var versaoAnterior = _usuario.VersaoToken;

            _usuario.Ativar();

            Assert.AreNotEqual(versaoAnterior, _usuario.VersaoToken);
            Assert.IsTrue(_usuario.Ativo);
        }

        [TestMethod]
        public void Deve_Atualizar_Usuario_Quando_Dados_Validos_E_Revogar_Token()
        {
            var versaoAnterior = _usuario.VersaoToken;
            _usuario.AtualizarPerfil("   Fernanda  ", "Nanda@gmail.com");

            Assert.AreNotEqual(versaoAnterior, _usuario.VersaoToken);
            Assert.AreEqual("Fernanda", _usuario.Nome);
            Assert.AreEqual("nanda@gmail.com", _usuario.Email);

        }

    }
}
