using AutoMapper;
using FluentAssertions;
using GestaoPedidos.Application.DTO.Usuarios;
using GestaoPedidos.Application.Mapper;
using GestaoPedidos.Application.UseCases.Usuarios.Commands;
using GestaoPedidos.Domain.Abstractions.Usuarios;
using GestaoPedidos.Domain.Entities;
using GestaoPedidos.Domain.Enum;
using GestaoPedidos.Domain.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace GestaoPedidosTests.Application.UseCases.Usuarios.Commands
{
    [TestClass]
    public class AutenticacaoUsuarioUseCaseTests
    {
        [TestMethod]
        public async Task Usuario_Inativo_Nao_Deve_Autenticar()
        {
            var usuario = new Usuario(
                "Usuário",
                "usuario@teste.com",
                "hash",
                UserRole.Vendedor);
            usuario.Inativar();

            var repository = new Mock<IUsuarioRepository>();
            repository
                .Setup(x => x.ObterPorEmail("usuario@teste.com"))
                .ReturnsAsync(usuario);
            var hasher = new Mock<IPasswordHasher>();
            hasher.Setup(x => x.Verificar("Senha12345", "hash")).Returns(true);
            var token = new Mock<IToken>();
            var useCase = new AutenticarUseCase(
                repository.Object,
                hasher.Object,
                token.Object);

            var act = () => useCase.Executar(new LoginRequestDTO
            {
                Email = "Usuario@Teste.com",
                Senha = "Senha12345"
            });

            await act.Should().ThrowAsync<UnauthorizedException>();
            token.Verify(x => x.GerarToken(It.IsAny<Usuario>()), Times.Never);
        }

        [TestMethod]
        public async Task Registro_Publico_Deve_Criar_Apenas_Vendedor()
        {
            var repository = new Mock<IUsuarioRepository>();
            repository
                .Setup(x => x.ObterPorEmail("usuario@teste.com"))
                .ReturnsAsync((Usuario?)null);
            repository
                .Setup(x => x.Cadastrar(It.IsAny<Usuario>()))
                .ReturnsAsync((Usuario usuario) => usuario);
            var hasher = new Mock<IPasswordHasher>();
            hasher.Setup(x => x.Hash("Senha12345")).Returns("hash");
            var config = new MapperConfiguration(
                cfg => cfg.AddProfile<UsuarioProfile>());
            var useCase = new RegistrarUsuarioUseCase(
                repository.Object,
                hasher.Object,
                config.CreateMapper());

            var resposta = await useCase.Executar(new UsuarioCreateDTO
            {
                Nome = "Usuário",
                Email = "Usuario@Teste.com",
                Senha = "Senha12345"
            });

            resposta.Role.Should().Be(UserRole.Vendedor);
            resposta.Email.Should().Be("usuario@teste.com");
            repository.Verify(x => x.Cadastrar(
                It.Is<Usuario>(u =>
                    u.Role == UserRole.Vendedor
                    && u.Senha == "hash")),
                Times.Once);
        }
    }
}
