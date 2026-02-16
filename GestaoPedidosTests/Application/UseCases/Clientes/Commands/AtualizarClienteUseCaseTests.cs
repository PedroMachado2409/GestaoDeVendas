using AutoMapper;
using FluentAssertions;
using GestaoPedidos.Application.DTO.Clientes;
using GestaoPedidos.Application.UseCases.Clientes.Commands;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Exceptions.Clientes;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestaoPedidosTests.Application.UseCases.Clientes.Commands
{

    [TestClass]
    public class AtualizarClienteUseCaseTests
    {
        private Mock<IClienteRepository> _repositoryMock;
        private AtualizarClienteUseCase _useCase;
        private IMapper _mapper;

        [TestInitialize]
        public void Setup()
        {
            _repositoryMock = new Mock<IClienteRepository>();
            var mapperConfig = new MapperConfiguration(cfg => 
            {
                cfg.CreateMap<Cliente, ClienteResponseDTO>();
            });

            _mapper = mapperConfig.CreateMapper();
            _useCase = new AtualizarClienteUseCase(_repositoryMock.Object, _mapper);
        }

        [TestMethod]
        public async Task Deve_Atualizar_Cliente_Quando_Dados_Forem_Validos()
        {
            var cliente = new Cliente("Pedro", "Pedro@gmail.com", "1234567810");

            var dto = new ClienteUpdateDTO
            {
                Id = cliente.Id,
                Nome = "Pedro Atualizado",
                Email = "Pedro@gmail2.com",
                Cpf = "12345678999"
            };

            _repositoryMock.Setup(r => r.ObterPorId(dto.Id)).ReturnsAsync(cliente);
            _repositoryMock.Setup(r => r.ObterPorCpf(dto.Cpf)).ReturnsAsync((Cliente?) null);
            _repositoryMock.Setup(r => r.ObterPorEmail(dto.Email)).ReturnsAsync((Cliente?)null);
            _repositoryMock.Setup(r => r.Atualizar(cliente)).Returns(Task.CompletedTask);

            var resultado = await _useCase.Execute(dto);
            resultado.Should().NotBeNull();

            cliente.Nome.Should().Be(dto.Nome);
            cliente.Email.Should().Be(dto.Email);
            cliente.Cpf.Should().Be(dto.Cpf);

            _repositoryMock.Verify(r => r.Atualizar(cliente), Times.Once());

        }
        [TestMethod]
        public async Task Nao_Deve_Atualizar_Quando_Cpf_Ja_Existir()
        {
            var clienteEditado = new Cliente("Pedro", "Pedro@gmail.com", "1234567810");
            var clienteExistente = new Cliente("Lucas", "Lucas@gmail.com", "10987654321");

            typeof(Cliente).GetProperty("Id")!.SetValue(clienteEditado, 1);
            typeof(Cliente).GetProperty("Id")!.SetValue(clienteExistente, 2);

            var dto = new ClienteUpdateDTO
            {
                Id = clienteEditado.Id,
                Nome = "PedroAtualizado",
                Cpf = clienteExistente.Cpf,
                Email = "Pedro@gmail.com"
            };

            _repositoryMock.Setup(r => r.ObterPorId(dto.Id))
                .ReturnsAsync(clienteEditado);

            _repositoryMock.Setup(r => r.ObterPorCpf(dto.Cpf))
                .ReturnsAsync(clienteExistente);

            Func<Task> act = () => _useCase.Execute(dto);

            var exception = await act.Should().ThrowAsync<BadHttpRequestException>();
            exception.Which.Message.Should().Be(ClientesExceptions.Cliente_CpfExistente);

            _repositoryMock.Verify(r => r.ObterPorId(dto.Id), Times.Once());
            _repositoryMock.Verify(r => r.ObterPorCpf(dto.Cpf), Times.Once());
            _repositoryMock.Verify(r => r.Atualizar(clienteEditado), Times.Never());
        }

        [TestMethod]
        public async Task Nao_Deve_Atualizar_Quando_Email_Ja_Existir()
        {
            var clienteEditado = new Cliente("Pedro", "Pedro@gmail.com", "1234567810");
            var clienteExistente = new Cliente("Lucas", "Lucas@gmail.com", "10987654321");

            typeof(Cliente).GetProperty("Id")!.SetValue(clienteEditado, 1);
            typeof(Cliente).GetProperty("Id")!.SetValue(clienteExistente, 2);

            var dto = new ClienteUpdateDTO
            {
                Id = clienteEditado.Id,
                Nome = "PedroAtualizado",
                Cpf = "1234567810",
                Email = clienteExistente.Email,
            };

            _repositoryMock.Setup(r => r.ObterPorId(dto.Id)).ReturnsAsync(clienteEditado);
            _repositoryMock.Setup(r => r.ObterPorCpf(dto.Cpf)).ReturnsAsync(clienteEditado);
            _repositoryMock.Setup(r => r.ObterPorEmail(dto.Email)).ReturnsAsync(clienteExistente);

            Func<Task> act = () => _useCase.Execute(dto);
            var exception = await act.Should().ThrowAsync<BadHttpRequestException>();
            exception.Which.Message.Should().Be(ClientesExceptions.Cliente_EmailExistente);

            _repositoryMock.Verify(r => r.ObterPorId(dto.Id), Times.Once());
            _repositoryMock.Verify(r => r.ObterPorCpf(dto.Cpf), Times.Once());
            _repositoryMock.Verify(r => r.ObterPorEmail(dto.Email), Times.Once());
            _repositoryMock.Verify(r => r.Atualizar(clienteEditado), Times.Never());


        }
    }
}
