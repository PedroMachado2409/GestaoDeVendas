using GestaoPedidos.Domain.Enum;

namespace GestaoPedidos.Domain.Entities
{
    public class Pessoa
    {
        public int Id { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string Cpf { get; private set; } = string.Empty;
        public bool Ativo { get; private set; }
        public DateTime DataCadastro { get; private set; }
        public TipoPessoa TipoPessoa { get; private set; }


        protected Pessoa() { }

        public Pessoa(string nome, string email, string cpf, TipoPessoa tipoPessoa)
        {
            Nome = nome.Trim();
            Email = NormalizarEmail(email);
            Cpf = NormalizarCpf(cpf);
            Ativo = true;
            DataCadastro = DateTime.UtcNow;
            TipoPessoa = tipoPessoa;
        }

        public void Atualizar(string nome, string email, string cpf, TipoPessoa tipoPessoa)
        {
            Nome = nome.Trim();
            Email = NormalizarEmail(email);
            Cpf = NormalizarCpf(cpf);
            TipoPessoa = tipoPessoa;
        }

        public void Inativar() => Ativo = false;
        public void Ativar() => Ativo = true;

        public static string NormalizarEmail(string email)
            => email.Trim().ToLowerInvariant();

        public static string NormalizarCpf(string cpf)
            => new(cpf.Where(char.IsAsciiDigit).ToArray());
    }
}
