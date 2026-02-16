using AutoMapper;
using FluentAssertions;
using GestaoPedidos.Application.DTO.Clientes;
using GestaoPedidos.Application.UseCases.Clientes.Commands;
using GestaoPedidos.Domain.Abstractions;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Exceptions.Clientes;
using Microsoft.AspNetCore.Http;
using Moq;


namespace GestaoPedidosTests.Application.UseCases.Clientes.Commands
{
    [TestClass]
    public class CadastrarClientesUseCaseTests
    {
        private Mock<IClienteRepository> _repositoryMock;
        private IMapper _mapper;
        private CadastrarClienteUseCase _useCase;

        [TestInitialize]
        public void Setup()
        {
            _repositoryMock = new Mock<IClienteRepository>();
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Cliente, ClienteResponseDTO>();
            });

            _mapper = mapperConfig.CreateMapper();
            _useCase = new CadastrarClienteUseCase(_repositoryMock.Object, _mapper);
        }

        [TestMethod]
        public async Task Deve_Cadastrar_Quando_Os_Dados_Forem_Validos()
        {
            var dto = new ClienteCreateDTO
            {
                Nome = "Pedro Machado",
                Cpf = "12345678910",
                Email = "pedro@gmail.com"
            };

             _repositoryMock.Setup(c => c.ObterPorCpf(dto.Cpf))
                .ReturnsAsync((Cliente?)null);

            _repositoryMock.Setup(c => c.ObterPorEmail(dto.Email))
                .ReturnsAsync((Cliente?)null);

            _repositoryMock.Setup(c => c.Cadastrar(It.IsAny<Cliente>()))
                .Returns(Task.CompletedTask);

            var result = await _useCase.Execute(dto);

            result.Should().NotBeNull();
            result.Nome.Should().Be(dto.Nome);
            result.Cpf.Should().Be(dto.Cpf);
            result.Email.Should().Be(dto.Email);

            _repositoryMock.Verify(r => r.ObterPorCpf(dto.Cpf), Times.Once());
            _repositoryMock.Verify(r => r.ObterPorEmail(dto.Email), Times.Once());
            _repositoryMock.Verify(r => r.Cadastrar(
                    It.Is<Cliente>(c =>
                        c.Nome == dto.Nome &&
                        c.Email == dto.Email &&
                        c.Cpf == dto.Cpf
                    )
                ), Times.Once());

        }

        [TestMethod]
        public async Task Nao_Deve_Cadastrar_Quando_Cpf_Ja_Existir()
        {
            var dto = new ClienteCreateDTO
            {
                Nome = "Pedro Machado",
                Cpf = "12345678910",
                Email = "pedro@gmail.com"
            };

            //Simula ter encontrado um cliente
            _repositoryMock.Setup(c => c.ObterPorCpf(dto.Cpf))
                .ReturnsAsync(new Cliente(
                    "ClienteExistente",
                    "clienteJaExiste",
                    "12345678910"
                 ));

            Func<Task> act = () => _useCase.Execute(dto);

            var exception = await act.Should().ThrowAsync<BadHttpRequestException>();
            exception.Which.Message.Should().Be(ClientesExceptions.Cliente_CpfExistente);

            _repositoryMock.Verify(r => r.ObterPorEmail(dto.Email), Times.Never);
            _repositoryMock.Verify(r => r.Cadastrar(It.IsAny<Cliente>()), Times.Never);
            

        }

        [TestMethod]
        public async Task Nao_Deve_Cadastrar_Quando_Email_Ja_Existir()
        {
            var dto = new ClienteCreateDTO
            {
                Nome = "Pedro Machado",
                Cpf = "12345678910",
                Email = "pedro@gmail.com"
            };

            _repositoryMock.Setup(c => c.ObterPorCpf(dto.Cpf))
                .ReturnsAsync((Cliente?)null);

            _repositoryMock.Setup(c => c.ObterPorEmail(dto.Email))
                .ReturnsAsync(new Cliente(
                    "ClienteExistente",
                    "12345678913",
                    "pedro@gmail.com"
                 ));

            Func<Task> act = () => _useCase.Execute(dto);
            var exception = await act.Should().ThrowAsync<BadHttpRequestException>();
            exception.Which.Message.Should().Be(ClientesExceptions.Cliente_EmailExistente);

            _repositoryMock.Verify(r => r.ObterPorCpf(dto.Cpf), Times.Once());
            _repositoryMock.Verify(r => r.Cadastrar(It.IsAny<Cliente>()), Times.Never);

        }
    }
}
