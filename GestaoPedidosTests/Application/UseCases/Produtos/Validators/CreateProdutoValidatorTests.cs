using AutoMapper;
using GestaoPedidos.Application.DTO.Produtos;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using Moq;
using FluentAssertions;
using GestaoPedidos.Application.Validators.Produtos;
using GestaoPedidos.Domain.Exceptions.Produtos;
namespace GestaoPedidosTests.Application.UseCases.Produtos.Validators
{
    [TestClass]
    public class CreateProdutoValidatorTests
    {
        private Mock<IProdutoRepository> _repositoryMock;
        private IMapper _mapper;

        [TestInitialize]
        public void Setup()
        {
            _repositoryMock = new Mock<IProdutoRepository>();
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Produto, ProdutoResponseDTO>();
            });

            _mapper = mapperConfig.CreateMapper();
        }

        [TestMethod]
        public void Deve_Cadastrar_Com_Sucesso_Quando_Dados_Forem_Validos()
        {
            var dto = new ProdutoCreateDTO
            {
                Nome = "Produto Teste",
                Estoque = 10,
                Marca = "Marca Teste",
                Preco = 15
            };

            var validator = new ProdutoCreateValidator();
            var resultado = validator.Validate(dto);
            resultado.IsValid.Should().BeTrue();
            resultado.Errors.Should().BeEmpty();
        }

        [TestMethod]
        public void Nao_Deve_Cadastrar_Sem_Nome()
        {
            var dto = new ProdutoCreateDTO
            {
                Nome = "",
                Estoque = 10,
                Marca = "Marca Teste",
                Preco = 15
            };

            var validator = new ProdutoCreateValidator();
            var resultado = validator.Validate(dto);
            resultado.IsValid.Should().BeFalse();
            resultado.Errors.Should().ContainSingle(e => e.PropertyName == "Nome" 
            && e.ErrorMessage == ProdutoExceptions.Produto_NomeObrigatorio);
        }

        [TestMethod]
        public void Nao_Deve_Cadastrar_Com_Estoque_Negativo()
        {
            var dto = new ProdutoCreateDTO
            {
                Nome = "Produto Teste",
                Estoque = -1,
                Marca = "Marca Teste",
                Preco = 15
            };

            var validator = new ProdutoCreateValidator();
            var resultado = validator.Validate(dto);
            resultado.IsValid.Should().BeFalse();
            resultado.Errors.Should().ContainSingle(e => e.PropertyName == "Estoque"
            && e.ErrorMessage == ProdutoExceptions.Produto_EstoqueObrigatorio);
        }

        [TestMethod]
        public void Nao_Deve_Cadastrar_Sem_Marca()
        {
            var dto = new ProdutoCreateDTO
            {
                Nome = "Produto Teste",
                Estoque = 10,
                Marca = "",
                Preco = 15
            };

            var validator = new ProdutoCreateValidator();
            var resultado = validator.Validate(dto);
            resultado.IsValid.Should().BeFalse();
            resultado.Errors.Should().ContainSingle(e => e.PropertyName == "Marca"
            && e.ErrorMessage == ProdutoExceptions.Produto_MarcaObrigatorio);
        }

        [TestMethod]
        public void Nao_Deve_Cadastrar_Com_Preco_negativo()
        {
            var dto = new ProdutoCreateDTO
            {
                Nome = "Produto Teste",
                Estoque = 10,
                Marca = "Marca Teste",
                Preco = -10
            };

            var validator = new ProdutoCreateValidator();
            var resultado = validator.Validate(dto);
            resultado.IsValid.Should().BeFalse();
            resultado.Errors.Should().ContainSingle(e => e.PropertyName == "Preco"
            && e.ErrorMessage == ProdutoExceptions.Produto_PrecoInvalido);
        }

        [TestMethod]
        public void Nao_Deve_Cadastrar_Com_Preco_Zerado()
        {
            var dto = new ProdutoCreateDTO
            {
                Nome = "Produto Teste",
                Estoque = 10,
                Marca = "Marca Teste",
                Preco = 0
            };

            var validator = new ProdutoCreateValidator();
            var resultado = validator.Validate(dto);
            resultado.IsValid.Should().BeFalse();
            resultado.Errors.Should().ContainSingle(e => e.PropertyName == "Preco"
            && e.ErrorMessage == ProdutoExceptions.Produto_PrecoInvalido);
        }
    }
}
