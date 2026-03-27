using GestaoPedidos.Domain.Enum;

namespace GestaoPedidos.Domain.Entities
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email {  get; set; } = string.Empty;
        public string Senha {  get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
        public bool Ativo { get; set; } = true;
        protected Usuario () { }

        public Usuario (string nome, string email, string senha, UserRole userRole)
        {
            Nome = nome;
            Email = email;
            Senha = senha;
            Role = userRole;
        }

        public void Atualizar(string nome, string email, UserRole userRole)
        {
            Nome = nome;
            Email = email;
            Role = userRole;
        }

        public void Ativar() => Ativo = true; 
        public void Inativar() => Ativo = false;


    }
}
