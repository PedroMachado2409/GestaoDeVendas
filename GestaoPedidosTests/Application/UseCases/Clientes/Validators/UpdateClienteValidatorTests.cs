using AutoMapper;
using FluentAssertions;
using GestaoPedidos.Application.DTO.Clientes;
using GestaoPedidos.Application.Validators.Clientes;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Exceptions.Clientes;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestaoPedidosTests.Application.UseCases.Clientes.Validators
{
    [TestClass]
    public class UpdateClienteValidatorTests
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
        public async Task Deve_Editar_Quando_Os_Dados_Forem_Validos()
        {
            var dto = new ClienteUpdateDTO
            {
                Id = 1,
                Nome = "Pedro Machado",
                Email = "Pedro@gmail.com",
                Cpf = "12345678910"
            };

            var validator = new ClienteUpdateValidator();
            var resultado = validator.Validate(dto);
            resultado.IsValid.Should().BeTrue();
            resultado.Errors.Should().BeEmpty();    

        }

        [TestMethod]
        public async Task Deve_Lancar_Excecao_Ao_Editar_Sem_Nome()
        {
            var dto = new ClienteUpdateDTO
            {
                Id = 1,
                Nome = "",
                Email = "Pedro@gmail.com",
                Cpf = "12345678910"
            };

            var validator = new ClienteUpdateValidator();
            var resultado = validator.Validate(dto);
            resultado.IsValid.Should().BeFalse();
            resultado.Errors.Should().ContainSingle(e => e.PropertyName == "Nome" && e.ErrorMessage == ClientesExceptions.Cliente_NomeObrigatorio);
        }

        [TestMethod]
        public async Task Deve_Lancar_Excecao_Ao_Editar_Sem_Cpf()
        {
            var dto = new ClienteUpdateDTO
            {
                Id = 1,
                Nome = "Pedro",
                Email = "Pedro@gmail.com",
                Cpf = ""
            };

            var validator = new ClienteUpdateValidator();
            var resultado = validator.Validate(dto);
            resultado.IsValid.Should().BeFalse();
            resultado.Errors.Should().ContainSingle(e => e.PropertyName == "Cpf" && e.ErrorMessage == ClientesExceptions.Cliente_CpfObrigatorio);
        }

        [TestMethod]
        public async Task Deve_Lancar_Excecao_Ao_Editar_Com_Cpf_Invalido()
        {
            var dto = new ClienteUpdateDTO
            {
                Id = 1,
                Nome = "Pedro",
                Email = "Pedro@gmail.com",
                Cpf = "123"
            };

            var validator = new ClienteUpdateValidator();
            var resultado = validator.Validate(dto);
            resultado.IsValid.Should().BeFalse();
            resultado.Errors.Should().ContainSingle(e => e.PropertyName == "Cpf" && e.ErrorMessage == ClientesExceptions.Cliente_CpfInvalido);
        }

        [TestMethod]
        public async Task Deve_Lancar_Excecao_Ao_Editar_Com_Email_Invalido()
        {
            var dto = new ClienteUpdateDTO
            {
                Id = 1,
                Nome = "Pedro",
                Email = "Pedro",
                Cpf = "12345678910"
            };

            var validator = new ClienteUpdateValidator();
            var resultado = validator.Validate(dto);
            resultado.IsValid.Should().BeFalse();
            resultado.Errors.Should().ContainSingle(e => e.PropertyName == "Email" && e.ErrorMessage == ClientesExceptions.Cliente_EmailInvalido);
        }


        [TestMethod]
        public async Task Deve_Lancar_Excecao_Ao_Editar_Sem_Email()
        {
            var dto = new ClienteUpdateDTO
            {
                Id = 1,
                Nome = "Pedro",
                Email = "",
                Cpf = "12345678910"
            };

            var validator = new ClienteUpdateValidator();
            var resultado = validator.Validate(dto);
            resultado.IsValid.Should().BeFalse();
            resultado.Errors.Should().ContainSingle(e => e.PropertyName == "Email" && e.ErrorMessage == ClientesExceptions.Cliente_EmailObrigatorio);
        }


    }
}
