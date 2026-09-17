using GestaoPedidos.Domain.Enum;

namespace GestaoPedidos.Domain.Entities
{
    public class Usuario
    {
        public int Id { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string Senha { get; private set; } = string.Empty;
        public UserRole Role { get; private set; }
        public DateTime DataCadastro { get; private set; } = DateTime.UtcNow;
        public bool Ativo { get; private set; } = true;
        public Guid VersaoToken { get; private set; } = Guid.NewGuid();

        protected Usuario() { }

        public Usuario(string nome, string email, string senha, UserRole role)
        {
            Nome = nome.Trim();
            Email = NormalizarEmail(email);
            Senha = senha;
            Role = role;
        }

        public void AtualizarPerfil(string nome, string email)
        {
            Nome = nome.Trim();
            Email = NormalizarEmail(email);
        }

        public void AlterarSenha(string senhaHash)
        {
            Senha = senhaHash;
            RevogarTokens();
        }

        public void AlterarRole(UserRole role)
        {
            Role = role;
            RevogarTokens();
        }

        public void Ativar()
        {
            Ativo = true;
            RevogarTokens();
        }

        public void Inativar()
        {
            Ativo = false;
            RevogarTokens();
        }

        private void RevogarTokens() => VersaoToken = Guid.NewGuid();

        private static string NormalizarEmail(string email)
            => email.Trim().ToLowerInvariant();
    }
}
