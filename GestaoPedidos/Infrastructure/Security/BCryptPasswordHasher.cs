using GestaoPedidos.Domain.Abstractions.Usuarios;

namespace GestaoPedidos.Infrastructure.Security
{
    public class BCryptPasswordHasher : IPasswordHasher
    {
        private const string PrefixoSha384 = "$bcrypt-sha384$";

        public string Hash(string senha)
            // O pré-hash evita truncar senhas que ultrapassam os 72 bytes do BCrypt.
            => PrefixoSha384 + BCrypt.Net.BCrypt.EnhancedHashPassword(senha, workFactor: 12);

        public bool Verificar(string senha, string hash)
        {
            if (string.IsNullOrEmpty(senha) || string.IsNullOrEmpty(hash))
            {
                return false;
            }

            try
            {
                if (hash.StartsWith(PrefixoSha384, StringComparison.Ordinal))
                {
                    return BCrypt.Net.BCrypt.EnhancedVerify(senha, hash[PrefixoSha384.Length..]);
                }

                // Mantém o login das contas criadas com o formato anterior.
                return BCrypt.Net.BCrypt.Verify(senha, hash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
    }
}
