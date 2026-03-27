using GestaoPedidos.Domain.Entities;

namespace GestaoPedidos.Domain.Abstractions.Usuarios
{
    public interface IToken
    {
        string GerarToken (Usuario usuario);
    }
}
