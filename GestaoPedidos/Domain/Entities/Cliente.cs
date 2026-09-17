namespace GestaoPedidos.Domain.Entities
{
    public class Cliente
    {
        public int Id { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string Cpf { get; private set; } = string.Empty;
        public bool Ativo { get; private set; }
        public DateTime DataCadastro { get; private set; }

        protected Cliente() { }

        public Cliente(string nome, string email, string cpf)
        {
            Nome = nome.Trim();
            Email = NormalizarEmail(email);
            Cpf = NormalizarCpf(cpf);
            Ativo = true;
            DataCadastro = DateTime.UtcNow;
        }

        public void Atualizar(string nome, string email, string cpf)
        {
            Nome = nome.Trim();
            Email = NormalizarEmail(email);
            Cpf = NormalizarCpf(cpf);
        }

        public void Inativar() => Ativo = false;
        public void Ativar() => Ativo = true;

        private static string NormalizarEmail(string email)
            => email.Trim().ToLowerInvariant();

        private static string NormalizarCpf(string cpf)
            => new(cpf.Where(char.IsDigit).ToArray());
    }
}
