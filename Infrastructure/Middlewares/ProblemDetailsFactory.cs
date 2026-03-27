namespace GestaoPedidos.Infrastructure.Middlewares
{
    public static class ProblemDetailsFactory
    {
        public static object Unauthorized(string detail)
            => new
            {
                title = "Não autorizado",
                status = StatusCodes.Status401Unauthorized,
                detail
            };

        public static object InternalServerError(string detail)
            => new
            {
                title = "Erro interno no servidor",
                status = StatusCodes.Status500InternalServerError,
                detail
            };

        public static object BadRequest(string detail)
            => new
            {
                title = "Requisição inválida",
                status = StatusCodes.Status400BadRequest,
                detail
            };

        public static object Forbidden(string detail)
            => new
            {
                title = "Acesso negado",
                status = StatusCodes.Status403Forbidden,
                detail
            };

        public static object NotFound(string detail)
            => new
            {
                title = "Recurso não encontrado",
                status = StatusCodes.Status404NotFound,
                detail
            };
    }
}