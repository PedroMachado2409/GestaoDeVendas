using AutoMapper;
using FluentAssertions;
using GestaoPedidos.Application.DTO.Produtos;
using GestaoPedidos.Application.UseCases.Produtos.Commands;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Moq;


namespace GestaoPedidosTests.Application.UseCases.Produtos.Commands
{

    [TestClass]
    public class CadastrarProdutoUseCaseTests
    {
        private Mock<IProdutoRepository> _repositoryMock = null!;
        private IMapper _mapper = null!;
        private CadastrarProdutoUseCase _useCase = null!;

        [TestInitialize]
        public void Setup()
        {
            _repositoryMock = new Mock<IProdutoRepository>();
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Produto, ProdutoResponseDTO>();
            });

            _mapper = mapperConfig.CreateMapper();
            _useCase = new CadastrarProdutoUseCase(_repositoryMock.Object, _mapper);
        }

        [TestMethod]
        public async Task Deve_Cadastrar_Produto_Quando_Dados_Forem_Validos()
        {
            var dto = new ProdutoCreateDTO
            {
                Nome = "Produto Teste",
                Estoque = 10,
                Marca = "Marca de Teste",
                Preco = 140
            };

            var result = await _useCase.Executar(dto);
            result.Should().NotBeNull();
            result.Nome.Should().Be(dto.Nome);
            result.Preco.Should().Be(dto.Preco);
            result.Estoque.Should().Be(dto.Estoque);
            result.Marca.Should().Be(dto.Marca);

            _repositoryMock.Verify(p => p.Cadastrar(It.Is<Produto>(p =>
                p.Nome == dto.Nome &&
                p.Estoque == dto.Estoque &&
                p.Marca == dto.Marca &&
                p.Preco == dto.Preco
            )), Times.Once());

        }


    }
}
