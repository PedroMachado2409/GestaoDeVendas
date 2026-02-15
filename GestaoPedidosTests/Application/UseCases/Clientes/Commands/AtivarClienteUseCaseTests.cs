using FluentAssertions;
using GestaoPedidos.Application.UseCases.Clientes.Commands;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Exceptions.Clientes;
using Microsoft.AspNetCore.Http;
using Moq;

namespace GestaoPedidosTests.Application.UseCases.Clientes.Commands
{
    [TestClass]
    public class AtivarClienteUseCaseTests
    {

        private Mock<IClienteRepository> _repositoryMock;
        private AtivarClienteUseCase _useCase;

        [TestInitialize]
        public void Setup()
        {
            _repositoryMock = new Mock<IClienteRepository>();
            _useCase = new AtivarClienteUseCase(_repositoryMock.Object);
        }

        [TestMethod]
        public async Task Deve_Ativar_Cliente_Quando_Esta_Inativo()
        {
            var cliente = new Cliente(

                "Pedro Machado",
                "pedro@gmail.com",
                "12345678910"
             );

            cliente.Inativar();
            _repositoryMock.Setup(r => r.ObterPorId(It.IsAny<int>())).ReturnsAsync(cliente);

            var result = await _useCase.Execute(1);
            result.Should().BeTrue();
            cliente.Ativo.Should().BeTrue();

            _repositoryMock.Verify(c => c.ObterPorId(1), Times.Once());
            _repositoryMock.Verify(c => c.Atualizar(cliente), Times.Once());
        }

        [TestMethod]
        public async Task Deve_Lancar_Excecao_Quando_Esta_Ativo()
        {
            var cliente = new Cliente(

                "Pedro Machado",
                "pedro@gmail.com",
                "12345678910"
             );

            _repositoryMock.Setup(r => r.ObterPorId(It.IsAny<int>())).ReturnsAsync(cliente);
            Func<Task> act = () => _useCase.Execute(1);
            var exception = await act .Should().ThrowAsync<BadHttpRequestException>();
            exception.Which.Message.Should().Be(ClientesExceptions.Cliente_JaAtivo);

            _repositoryMock.Verify(c => c.ObterPorId(1), Times.Once);
            _repositoryMock.Verify(c => c.Atualizar(cliente), Times.Never());

        }

        [TestMethod]
        public async Task Deve_Lancar_Excecao_Quando_Nao_Achar_O_Cliente()
        {
            _repositoryMock.Setup(c => c.ObterPorId(It.IsAny<int>())).ReturnsAsync((Cliente?)null);
            Func<Task> act = () => _useCase.Execute(1);
            var exception = await act.Should().ThrowAsync<BadHttpRequestException>();
            exception.Which.Message.Should().Be(ClientesExceptions.Cliente_NaoEncontrado);

            _repositoryMock.Verify(c => c.ObterPorId(1), Times.Once);
            _repositoryMock.Verify(c => c.Atualizar(It.IsAny<Cliente>()), Times.Never());

        }

    }
}
