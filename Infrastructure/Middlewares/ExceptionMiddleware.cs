using System.Text.Json;
using GestaoPedidos.Domain.Exceptions;

namespace GestaoPedidos.Infrastructure.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger,
            IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (AppException ex)
            {
                _logger.LogWarning(ex, "Erro de aplicação");

                await HandleAppException(context, ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acesso não autorizado");

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                var problem = ProblemDetailsFactory.Unauthorized(ex.Message);

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(problem));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno");

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                var detail = _env.IsDevelopment()
                    ? ex.Message
                    : "Ocorreu um erro inesperado.";

                var problem = ProblemDetailsFactory.InternalServerError(detail);

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(problem));
            }
        }

        private static async Task HandleAppException(
            HttpContext context,
            AppException ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex.StatusCode;

            var problem = ex.StatusCode switch
            {
                StatusCodes.Status400BadRequest
                    => ProblemDetailsFactory.BadRequest(ex.Message),

                StatusCodes.Status404NotFound
                    => ProblemDetailsFactory.NotFound(ex.Message),

                StatusCodes.Status403Forbidden
                    => ProblemDetailsFactory.Forbidden(ex.Message),

                _ => ProblemDetailsFactory.InternalServerError(ex.Message)
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(problem));
        }
    }
}