using BCrypt.Net;

namespace GestaoPedidos.Infrastructure.Security
{
    public class PasswordHelper
    {
        public static string HashPassword(string senha)
        {
            return BCrypt.Net.BCrypt.HashPassword(senha);
        }

        public static bool VerificarSenha(string senha, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(senha, hash);
        }
    }
}
