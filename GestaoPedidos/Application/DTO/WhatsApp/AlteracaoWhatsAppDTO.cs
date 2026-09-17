using System.Text.Json.Serialization;

namespace GestaoPedidos.Application.DTO.WhatsApp
{
    public class AlteracaoWhatsAppDTO
    {
        [JsonPropertyName("value")]
        public ValorWhatsAppDTO Value { get; set; } = new();

        [JsonPropertyName("field")]
        public string Field { get; set; } = string.Empty;
    }
}
