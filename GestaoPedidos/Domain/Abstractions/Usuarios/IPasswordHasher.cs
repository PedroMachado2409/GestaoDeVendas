namespace GestaoPedidos.Domain.Abstractions.Usuarios
{
    public interface IPasswordHasher
    {
        string Hash(string senha);
        bool Verificar(string senha, string hash);
    }
}
