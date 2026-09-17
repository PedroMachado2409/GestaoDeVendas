using GestaoPedidos.Domain.Abstractions.Usuarios;

namespace GestaoPedidos.Infrastructure.Security
{
    public class BCryptPasswordHasher : IPasswordHasher
    {
        public string Hash(string senha)
            => BCrypt.Net.BCrypt.HashPassword(senha, workFactor: 12);

        public bool Verificar(string senha, string hash)
            => BCrypt.Net.BCrypt.Verify(senha, hash);
    }
}
