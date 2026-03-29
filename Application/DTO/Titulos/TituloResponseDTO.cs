using GestaoPedidos.Domain.Enum;

namespace GestaoPedidos.Application.DTO.Titulos
{
    public class TituloResponseDTO
    {
        public int Id { get; set; }
        public string NomeTitulo { get; set; } = string.Empty;
        public decimal ValorTitulo { get; set; }
        public int ClienteId { get; set; }
        public string Observacao { get; set; } = string.Empty;
        public TipoTitulo Tipo { get; set; }

        public bool StBaixado { get; set; }
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
        public int? IdOrigem { get; set; } = null;
    }
}
