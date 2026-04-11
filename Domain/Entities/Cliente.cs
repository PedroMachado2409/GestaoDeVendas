

public class Cliente
{
    public int Id { get; private set; } 
    public string Nome { get; private set; } 
    public string Email { get; private set; }
    public string Cpf { get; private set; }
    public bool Ativo { get; private set; } = true;
    public DateTime DataCadastro { get; private set; } = DateTime.UtcNow;

    protected Cliente() { }

    public Cliente(string nome, string email, string cpf)
    {
        Nome = nome;
        Email = email;
        Cpf = cpf;
        Ativo = true;
        DataCadastro = DateTime.UtcNow;
    }

    public void Atualizar(string nome, string email, string cpf)
    {
        Nome = nome;
        Email = email;
        Cpf = cpf;
    }

    public void Inativar() => Ativo = false;
    public void Ativar() => Ativo = true;
}