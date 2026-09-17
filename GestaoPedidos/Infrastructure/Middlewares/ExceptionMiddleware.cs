using GestaoPedidos.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestaoPedidos.Infrastructure.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (AppException exception)
            {
                _logger.LogWarning(
                    exception,
                    "Falha de negócio em {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                await EscreverProblema(
                    context,
                    exception.StatusCode,
                    TituloParaStatus(exception.StatusCode),
                    exception.Message);
            }
            catch (DbUpdateConcurrencyException exception)
            {
                _logger.LogWarning(exception, "Conflito de concorrência ao persistir dados");

                await EscreverProblema(
                    context,
                    StatusCodes.Status409Conflict,
                    "Conflito de concorrência",
                    "Os dados foram alterados por outra operação. Atualize a consulta e tente novamente.");
            }
            catch (DbUpdateException exception)
            {
                _logger.LogError(exception, "Falha de integridade ao persistir dados");

                await EscreverProblema(
                    context,
                    StatusCodes.Status409Conflict,
                    "Conflito de dados",
                    "A operação viola uma regra de integridade dos dados.");
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Erro não tratado em {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                var detalhe = _environment.IsDevelopment()
                    ? exception.Message
                    : "Ocorreu um erro inesperado.";

                await EscreverProblema(
                    context,
                    StatusCodes.Status500InternalServerError,
                    "Erro interno no servidor",
                    detalhe);
            }
        }

        private static async Task EscreverProblema(
            HttpContext context,
            int statusCode,
            string titulo,
            string detalhe)
        {
            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = titulo,
                Detail = detalhe,
                Instance = context.Request.Path
            };
            problem.Extensions["traceId"] = context.TraceIdentifier;

            await context.Response.WriteAsJsonAsync(problem);
        }

        private static string TituloParaStatus(int statusCode)
            => statusCode switch
            {
                StatusCodes.Status400BadRequest => "Requisição inválida",
                StatusCodes.Status401Unauthorized => "Não autorizado",
                StatusCodes.Status403Forbidden => "Acesso negado",
                StatusCodes.Status404NotFound => "Recurso não encontrado",
                StatusCodes.Status409Conflict => "Conflito de dados",
                _ => "Erro na aplicação"
            };
    }
}
