namespace GestaoPedidos.Domain.Exceptions
{
    public class ConflictException : AppException
    {
        public ConflictException(string message)
            : base(message, StatusCodes.Status409Conflict) { }
    }
}
