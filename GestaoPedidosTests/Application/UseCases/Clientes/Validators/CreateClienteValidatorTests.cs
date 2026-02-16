using AutoMapper;
using FluentAssertions;
using GestaoPedidos.Application.DTO.Clientes;
using GestaoPedidos.Application.UseCases.Clientes.Commands;
using GestaoPedidos.Application.Validators.Clientes;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Exceptions.Clientes;
using Moq;

namespace GestaoPedidosTests.Application.UseCases.Clientes.Validators
{

    [TestClass]
    public class CreateClienteValidatorTests
    {
        private Mock<IClienteRepository> _repositoryMock;
        private IMapper _mapper;



        [TestInitialize]
        public void Setup()
        {
            _repositoryMock = new Mock<IClienteRepository>();
            var MapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Cliente, ClienteResponseDTO>();
            });

            _mapper = MapperConfig.CreateMapper();
          
        }

        [TestMethod]
        public async Task Deve_Validar_Com_Sucesso_Quando_Dados_Validos()
        {
            var dto = new ClienteCreateDTO
            {
                Nome = "Pedro Machado",
                Email = "Pedro@gmail.com",
                Cpf = "12345678910"
            };

            var validator = new ClienteCreateValidator();
            var resultado = validator.Validate(dto);

            resultado.IsValid.Should().BeTrue();
            resultado.Errors.Should().BeEmpty();
        }

        [TestMethod]
        public async Task Nao_Deve_Cadastrar_Sem_Email()
        {
            var dto = new ClienteCreateDTO
            {
                Nome = "Pedro Machado",
                Email = "",
                Cpf = "12345678910"
            };

            var validator = new ClienteCreateValidator();
            var resultado = validator.Validate(dto);
            resultado.IsValid.Should().BeFalse();

            resultado.Errors.Should().ContainSingle(e =>
            e.PropertyName == "Email" && e.ErrorMessage == ClientesExceptions.Cliente_EmailObrigatorio);
        }

        [TestMethod]
        public async Task Nao_Deve_Cadastrar_Com_Email_Invalido()
        {
            var dto = new ClienteCreateDTO
            {
                Nome = "Pedro Machado",
                Email = "Pedro",
                Cpf = "12345678910"
            };

            var validator = new ClienteCreateValidator();
            var resultado = validator.Validate(dto);
            resultado.IsValid.Should().BeFalse();
            resultado.Errors.Should().ContainSingle(e => e.PropertyName == "Email" && e.ErrorMessage == ClientesExceptions.Cliente_EmailInvalido);

        }

        [TestMethod]
        public async Task Nao_Deve_Cadastrar_Sem_Cpf()
        {
            var dto = new ClienteCreateDTO
            {
                Nome = "Pedro Machado",
                Email = "Pedro@gmail.com",
                Cpf = ""
            };

            var validator = new ClienteCreateValidator();
            var resultado = validator.Validate(dto);
            resultado.IsValid.Should().BeFalse();
            resultado.Errors.Should().ContainSingle(e => e.PropertyName == "Cpf" && e.ErrorMessage == ClientesExceptions.Cliente_CpfObrigatorio);
        }

        [TestMethod]
        public async Task Nao_Deve_Cadastrar_Com_Cpf_Invalido()
        {
            var dto = new ClienteCreateDTO
            {
                Nome = "Pedro Machado",
                Email = "Pedro@gmail.com",
                Cpf = "123"
            };

            var validator = new ClienteCreateValidator();
            var resultado = validator.Validate(dto);
            resultado.IsValid.Should().BeFalse();
            resultado.Errors.Should().ContainSingle(e => e.PropertyName == "Cpf" && e.ErrorMessage == ClientesExceptions.Cliente_CpfInvalido);
        }

        [TestMethod]
        public async Task Nao_Deve_Cadastrar_Sem_Nome()
        {
            var dto = new ClienteCreateDTO
            {
                Nome = "",
                Email = "Pedro@gmail.com",
                Cpf = "12345678910"
            };

            var validator = new ClienteCreateValidator();
            var resultado = validator.Validate(dto);
            resultado.IsValid.Should().BeFalse();
            resultado.Errors.Should().ContainSingle(e => e.PropertyName == "Nome" && e.ErrorMessage == ClientesExceptions.Cliente_NomeObrigatorio);

        }

    }
}
