using FluentAssertions;
using FluentValidation;
using GestaoPedidos.Application.DTO.Usuarios;
using GestaoPedidos.Application.Validators.Usuarios;
using GestaoPedidos.Infrastructure.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace GestaoPedidosTests.Infrastructure.Filters
{
    [TestClass]
    public class FluentValidationFilterTests
    {
        [TestMethod]
        public async Task Entrada_Invalida_Deve_Retornar_ProblemDetails_Sem_Executar_Acao()
        {
            using var provider = CriarServiceProvider();
            using var scope = provider.CreateScope();
            var httpContext = new DefaultHttpContext
            {
                RequestServices = scope.ServiceProvider,
                TraceIdentifier = "trace-teste"
            };
            var actionContext = new ActionContext(
                httpContext,
                new RouteData(),
                new ActionDescriptor());
            var executingContext = new ActionExecutingContext(
                actionContext,
                [],
                new Dictionary<string, object?>
                {
                    ["dto"] = new UsuarioCreateDTO()
                },
                new object());
            var nextCalled = false;

            await new FluentValidationFilter().OnActionExecutionAsync(
                executingContext,
                () =>
                {
                    nextCalled = true;
                    return Task.FromResult(new ActionExecutedContext(
                        actionContext,
                        [],
                        new object()));
                });

            nextCalled.Should().BeFalse();
            var result = executingContext.Result
                .Should()
                .BeOfType<BadRequestObjectResult>()
                .Subject;
            var problem = result.Value
                .Should()
                .BeOfType<ValidationProblemDetails>()
                .Subject;
            problem.Errors.Keys.Should().Contain(["Nome", "Email", "Senha"]);
            problem.Extensions["traceId"].Should().Be("trace-teste");
        }

        [TestMethod]
        public async Task Entrada_Valida_Deve_Executar_Acao()
        {
            using var provider = CriarServiceProvider();
            using var scope = provider.CreateScope();
            var httpContext = new DefaultHttpContext
            {
                RequestServices = scope.ServiceProvider
            };
            var actionContext = new ActionContext(
                httpContext,
                new RouteData(),
                new ActionDescriptor());
            var executingContext = new ActionExecutingContext(
                actionContext,
                [],
                new Dictionary<string, object?>
                {
                    ["dto"] = new UsuarioCreateDTO
                    {
                        Nome = "Usuário Teste",
                        Email = "usuario@teste.com",
                        Senha = "Senha12345"
                    }
                },
                new object());
            var nextCalled = false;

            await new FluentValidationFilter().OnActionExecutionAsync(
                executingContext,
                () =>
                {
                    nextCalled = true;
                    return Task.FromResult(new ActionExecutedContext(
                        actionContext,
                        [],
                        new object()));
                });

            nextCalled.Should().BeTrue();
            executingContext.Result.Should().BeNull();
        }

        private static ServiceProvider CriarServiceProvider()
            => new ServiceCollection()
                .AddScoped<IValidator<UsuarioCreateDTO>, UsuarioCreateValidator>()
                .BuildServiceProvider();
    }
}
