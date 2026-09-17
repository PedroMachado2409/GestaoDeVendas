

using FluentAssertions;
using GestaoPedidos.Application.UseCases.Produtos.Commands;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Domain.Exceptions.Produtos;
using Microsoft.AspNetCore.Http;
using Moq;

namespace GestaoPedidosTests.Application.UseCases.Produtos.Commands
{
    [TestClass]
    public class InativarProdutoUseCaseTests
    {
        private Mock<IProdutoRepository> _repositoryMock = null!;
        private InativarProdutoUseCase _useCase = null!;

        [TestInitialize]
        public void Setup()
        {
            _repositoryMock = new Mock<IProdutoRepository>();
            _useCase = new InativarProdutoUseCase(_repositoryMock.Object);
        }

        [TestMethod]
        public async Task Deve_Inativar_Produto_Quando_Esta_Ativo()
        {
            var produto = new Produto("Produto Teste", "Marca Teste", 10, 15);
            _repositoryMock.Setup(r => r.ObterPorId(It.IsAny<int>())).ReturnsAsync(produto);
            var result = await _useCase.Executar(1);
            result.Should().BeTrue();
            produto.Ativo.Should().BeFalse();

            _repositoryMock.Verify(r => r.ObterPorId(1), Times.Once());
            _repositoryMock.Verify(r => r.Atualizar(produto), Times.Once());
        }

        [TestMethod]
        public async Task Deve_Lancar_Execao_Quando_Estiver_Inativo()
        {
            var produto = new Produto("Produto Teste", "Marca Teste", 10, 15);
            produto.Inativar();
            _repositoryMock.Setup(r => r.ObterPorId(It.IsAny<int>())).ReturnsAsync(produto);
            Func<Task> act = () => _useCase.Executar(1);
            var exception = await act.Should().ThrowAsync<GestaoPedidos.Domain.Exceptions.BadRequestException>();
            exception.Which.Message.Should().Be(ProdutoExceptions.Produto_JaInativo);

            _repositoryMock.Verify(r => r.ObterPorId(1), Times.Once());
            _repositoryMock.Verify(r => r.Atualizar(produto), Times.Never());
        }

        [TestMethod]
        public async Task Deve_Lancar_Execao_Quando_Nao_Achar_O_Produto()
        {
            var produto = new Produto("Produto Teste", "Marca Teste", 10, 15);
            produto.Inativar();
            _repositoryMock.Setup(r => r.ObterPorId(It.IsAny<int>())).ReturnsAsync((Produto?)null);
            Func<Task> act = () => _useCase.Executar(1);
            var exception = await act.Should().ThrowAsync<GestaoPedidos.Domain.Exceptions.NotFoundException>();
            exception.Which.Message.Should().Be(ProdutoExceptions.Produto_NaoEncontrado);

            _repositoryMock.Verify(r => r.ObterPorId(1), Times.Once());
            _repositoryMock.Verify(r => r.Atualizar(produto), Times.Never());
        }

    }
}
