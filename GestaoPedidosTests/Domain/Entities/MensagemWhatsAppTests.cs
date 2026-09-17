using FluentAssertions;
using GestaoPedidos.Domain.Entities;

namespace GestaoPedidosTests.Domain.Entities
{
    [TestClass]
    public class MensagemWhatsAppTests
    {
        private static readonly DateTime DataValida =
            new(2026, 9, 7, 12, 0, 0, DateTimeKind.Utc);

        private static MensagemWhatsApp Criar(
            string idMensagem = "wamid.HBgL",
            string numeroRemetente = "5521999999999",
            string nomeRemetente = "Pedro",
            string tipoMensagem = "text",
            string conteudo = "Bom dia",
            DateTime? dataMensagem = null,
            string phoneNumberId = "123456789")
            => new(
                idMensagem,
                numeroRemetente,
                nomeRemetente,
                tipoMensagem,
                conteudo,
                dataMensagem ?? DataValida,
                phoneNumberId);

        [TestMethod]
        public void Deve_Criar_Quando_Os_Dados_Forem_Validos()
        {
            var mensagem = Criar();

            mensagem.IdMensagemWhatsApp.Should().Be("wamid.HBgL");
            mensagem.NumeroRemetente.Should().Be("5521999999999");
            mensagem.NomeRemetente.Should().Be("Pedro");
            mensagem.Conteudo.Should().Be("Bom dia");
            mensagem.DataMensagem.Should().Be(DataValida);
        }

        [TestMethod]
        public void Deve_Aceitar_Nome_E_Conteudo_Vazios()
        {
            var mensagem = Criar(nomeRemetente: "", conteudo: "");

            mensagem.NomeRemetente.Should().BeEmpty();
            mensagem.Conteudo.Should().BeEmpty();
        }

        [TestMethod]
        public void Nao_Deve_Criar_Sem_Identificador_Da_Meta()
        {
            var acao = () => Criar(idMensagem: "   ");

            acao.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void Nao_Deve_Criar_Sem_Numero_Do_Remetente()
        {
            var acao = () => Criar(numeroRemetente: "");

            acao.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void Nao_Deve_Criar_Quando_O_Conteudo_Exceder_O_Limite_Da_Coluna()
        {
            var excedente = new string('a', MensagemWhatsApp.TamanhoMaximoConteudo + 1);

            var acao = () => Criar(conteudo: excedente);

            acao.Should().Throw<ArgumentOutOfRangeException>();
        }

        [TestMethod]
        public void Nao_Deve_Criar_Com_Data_Fora_De_Utc()
        {
            var acao = () => Criar(
                dataMensagem: new DateTime(2026, 9, 7, 12, 0, 0, DateTimeKind.Local));

            acao.Should().Throw<ArgumentException>();
        }
    }
}
