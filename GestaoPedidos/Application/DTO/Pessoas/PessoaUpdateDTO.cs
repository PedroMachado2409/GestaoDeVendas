using GestaoPedidos.Domain.Enum;

namespace GestaoPedidos.Application.DTO.Pessoas
{
    public class PessoaUpdateDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public TipoPessoa TipoPessoa { get; set; }
    }
}
