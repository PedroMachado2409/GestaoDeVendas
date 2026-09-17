
using AutoMapper;
using FluentAssertions;
using GestaoPedidos.Application.DTO.Produtos;
using GestaoPedidos.Application.UseCases.Produtos.Commands;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Domain.Exceptions.Produtos;
using Microsoft.AspNetCore.Http;
using Moq;

namespace GestaoPedidosTests.Application.UseCases.Produtos.Commands
{
    [TestClass]
    public class AtualizarProdutoUseCaseTests
    {
        private Mock<IProdutoRepository> _repositoryMock = null!;
        private AtualizarProdutoUseCase _useCase = null!;
        private IMapper _mapper = null!;

        [TestInitialize]
        public void Setup()
        {
            _repositoryMock = new Mock<IProdutoRepository>();
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Produto, ProdutoResponseDTO>();
            });

            _mapper = mapperConfig.CreateMapper();
            _useCase = new AtualizarProdutoUseCase(_mapper, _repositoryMock.Object);
        }


        [TestMethod]
        public async Task Deve_Atualizar_Produto_Com_Sucesso()
        {
            var produto = new Produto("Produto", "Marca", 10, 15);
            var dto = new ProdutoUpdateDTO
            {
                Id = produto.Id,
                Nome = "Produto Teste",
                Preco = 10,
                Estoque = 15,
                Marca = "Marca Teste"
            };

            _repositoryMock.Setup(r => r.ObterPorId(dto.Id)).ReturnsAsync(produto);
            _repositoryMock.Setup(r => r.Atualizar(produto)).Returns(Task.CompletedTask);

            var resultado = await _useCase.Executar(dto);
            resultado.Should().NotBeNull();

            produto.Nome.Should().Be(dto.Nome);
            produto.Preco.Should().Be(dto.Preco);
            produto.Estoque.Should().Be(dto.Estoque);
            produto.Marca.Should().Be(dto.Marca);

            _repositoryMock.Verify(r => r.Atualizar(produto), Times.Once());
        }

        [TestMethod]
        public async Task Deve_Lancar_Exception_Quando_Nao_Achar_O_Produto()
        {
            var produto = new Produto("Produto", "Marca", 10, 15);
            var dto = new ProdutoUpdateDTO
            {
                Id = 13,
                Nome = "Produto Teste",
                Preco = 10,
                Estoque = 15,
                Marca = "Marca Teste"
            };

            _repositoryMock.Setup(r => r.ObterPorId(dto.Id)).ReturnsAsync((Produto?)null);
            Func<Task> act = () => _useCase.Executar(dto);
            var exception = await act.Should().ThrowAsync<GestaoPedidos.Domain.Exceptions.NotFoundException>();
            exception.Which.Message.Should().Be(ProdutoExceptions.Produto_NaoEncontrado);

            _repositoryMock.Verify(r => r.ObterPorId(dto.Id), Times.Once());
            _repositoryMock.Verify(r => r.Atualizar(produto), Times.Never());
        }
    }
}
