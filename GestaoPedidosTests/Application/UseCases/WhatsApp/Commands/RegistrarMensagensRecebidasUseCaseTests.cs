using FluentAssertions;
using GestaoPedidos.Application.DTO.WhatsApp;
using GestaoPedidos.Application.UseCases.WhatsApp.Commands;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace GestaoPedidosTests.Application.UseCases.WhatsApp.Commands
{
    [TestClass]
    public class RegistrarMensagensRecebidasUseCaseTests
    {
        private Mock<IMensagemWhatsAppRepository> _repositoryMock = null!;
        private RegistrarMensagensRecebidasUseCase _useCase = null!;

        [TestInitialize]
        public void Setup()
        {
            _repositoryMock = new Mock<IMensagemWhatsAppRepository>();

            _repositoryMock
                .Setup(r => r.AdicionarSeNaoExistirAsync(
                    It.IsAny<MensagemWhatsApp>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _useCase = new RegistrarMensagensRecebidasUseCase(
                _repositoryMock.Object,
                NullLogger<RegistrarMensagensRecebidasUseCase>.Instance);
        }

        private static WebhookWhatsAppDTO MontarWebhook(
            params MensagemRecebidaWhatsAppDTO[] mensagens)
            => new()
            {
                Object = "whatsapp_business_account",
                Entry =
                [
                    new EntradaWhatsAppDTO
                    {
                        Id = "entrada-1",
                        Changes =
                        [
                            new AlteracaoWhatsAppDTO
                            {
                                Field = "messages",
                                Value = new ValorWhatsAppDTO
                                {
                                    Metadata = new MetadadosWhatsAppDTO
                                    {
                                        PhoneNumberId = "123456789"
                                    },
                                    Contacts =
                                    [
                                        new ContatoWhatsAppDTO
                                        {
                                            WaId = "5521999999999",
                                            Profile = new PerfilWhatsAppDTO { Name = "Pedro" }
                                        }
                                    ],
                                    Messages = [.. mensagens]
                                }
                            }
                        ]
                    }
                ]
            };

        private static MensagemRecebidaWhatsAppDTO MensagemDeTexto(
            string id = "wamid.1",
            string texto = "Bom dia",
            string timestamp = "1757246400")
            => new()
            {
                Id = id,
                From = "5521999999999",
                Type = "text",
                Timestamp = timestamp,
                Text = new TextoWhatsAppDTO { Body = texto }
            };

        [TestMethod]
        public async Task Deve_Registrar_A_Mensagem_De_Texto_Recebida()
        {
            MensagemWhatsApp? gravada = null;

            _repositoryMock
                .Setup(r => r.AdicionarSeNaoExistirAsync(
                    It.IsAny<MensagemWhatsApp>(),
                    It.IsAny<CancellationToken>()))
                .Callback<MensagemWhatsApp, CancellationToken>((m, _) => gravada = m)
                .ReturnsAsync(true);

            var registradas = await _useCase.Executar(MontarWebhook(MensagemDeTexto()));

            registradas.Should().Be(1);
            gravada.Should().NotBeNull();
            gravada!.IdMensagemWhatsApp.Should().Be("wamid.1");
            gravada.NumeroRemetente.Should().Be("5521999999999");
            gravada.NomeRemetente.Should().Be("Pedro");
            gravada.Conteudo.Should().Be("Bom dia");
            gravada.PhoneNumberId.Should().Be("123456789");
            gravada.DataMensagem.Should().Be(
                DateTimeOffset.FromUnixTimeSeconds(1757246400).UtcDateTime);
        }

        [TestMethod]
        public async Task Deve_Registrar_Todas_As_Mensagens_Do_Lote()
        {
            var webhook = MontarWebhook(
                MensagemDeTexto("wamid.1"),
                MensagemDeTexto("wamid.2"),
                MensagemDeTexto("wamid.3"));

            var registradas = await _useCase.Executar(webhook);

            registradas.Should().Be(3);
        }

        [TestMethod]
        public async Task Nao_Deve_Contar_Mensagem_Que_Ja_Estava_Registrada()
        {
            _repositoryMock
                .Setup(r => r.AdicionarSeNaoExistirAsync(
                    It.IsAny<MensagemWhatsApp>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var registradas = await _useCase.Executar(MontarWebhook(MensagemDeTexto()));

            registradas.Should().Be(0);
        }

        [TestMethod]
        public async Task Deve_Guardar_Conteudo_Vazio_Quando_A_Mensagem_Nao_For_Texto()
        {
            MensagemWhatsApp? gravada = null;

            _repositoryMock
                .Setup(r => r.AdicionarSeNaoExistirAsync(
                    It.IsAny<MensagemWhatsApp>(),
                    It.IsAny<CancellationToken>()))
                .Callback<MensagemWhatsApp, CancellationToken>((m, _) => gravada = m)
                .ReturnsAsync(true);

            var imagem = MensagemDeTexto();
            imagem.Type = "image";
            imagem.Text = null;

            await _useCase.Executar(MontarWebhook(imagem));

            gravada.Should().NotBeNull();
            gravada!.TipoMensagem.Should().Be("image");
            gravada.Conteudo.Should().BeEmpty();
        }

        [TestMethod]
        public async Task Deve_Descartar_A_Mensagem_Invalida_Sem_Perder_As_Demais()
        {
            var invalida = MensagemDeTexto("wamid.invalida", timestamp: "nao-e-numero");

            var registradas = await _useCase.Executar(
                MontarWebhook(invalida, MensagemDeTexto("wamid.valida")));

            registradas.Should().Be(1);

            _repositoryMock.Verify(
                r => r.AdicionarSeNaoExistirAsync(
                    It.Is<MensagemWhatsApp>(m => m.IdMensagemWhatsApp == "wamid.valida"),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _repositoryMock.Verify(
                r => r.AdicionarSeNaoExistirAsync(
                    It.Is<MensagemWhatsApp>(m => m.IdMensagemWhatsApp == "wamid.invalida"),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Nao_Deve_Gravar_Nada_Quando_A_Notificacao_Nao_Trouxer_Mensagem()
        {
            var webhook = MontarWebhook();
            webhook.Entry[0].Changes[0].Value.Messages = null;

            var registradas = await _useCase.Executar(webhook);

            registradas.Should().Be(0);

            _repositoryMock.Verify(
                r => r.AdicionarSeNaoExistirAsync(
                    It.IsAny<MensagemWhatsApp>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [TestMethod]
        public async Task Deve_Ignorar_Notificacao_Sem_Entradas()
        {
            var registradas = await _useCase.Executar(new WebhookWhatsAppDTO());

            registradas.Should().Be(0);
        }

        [TestMethod]
        public async Task Deve_Deixar_O_Nome_Vazio_Quando_O_Contato_Nao_Vier_Na_Notificacao()
        {
            MensagemWhatsApp? gravada = null;

            _repositoryMock
                .Setup(r => r.AdicionarSeNaoExistirAsync(
                    It.IsAny<MensagemWhatsApp>(),
                    It.IsAny<CancellationToken>()))
                .Callback<MensagemWhatsApp, CancellationToken>((m, _) => gravada = m)
                .ReturnsAsync(true);

            var webhook = MontarWebhook(MensagemDeTexto());
            webhook.Entry[0].Changes[0].Value.Contacts = null;

            await _useCase.Executar(webhook);

            gravada.Should().NotBeNull();
            gravada!.NomeRemetente.Should().BeEmpty();
        }
    }
}
