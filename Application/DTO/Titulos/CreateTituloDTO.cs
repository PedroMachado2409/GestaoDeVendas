using GestaoPedidos.Domain.Enum;

namespace GestaoPedidos.Application.DTO.Titulos
{
    public class CreateTituloDTO
    {
        public string NomeTitulo { get; set; } = string.Empty;
        public decimal ValorTitulo { get; set; }
        public int ClienteId { get; set; }
        public string Observacao { get; set; } = string.Empty;
        public TipoTitulo Tipo { get; set; }
    }
}
