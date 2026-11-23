using GestaoPedidos.Domain.Enum;

namespace GestaoPedidos.Application.DTO.Pessoas
{
    public class PessoaResponseDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public DateTime DataCadastro { get; set; }
        public TipoPessoa TipoPessoa { get; set; }
    }
}
