using System.Text.Json.Serialization;

namespace GestaoPedidos.Application.DTO.WhatsApp
{
    public class EntradaWhatsAppDTO
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("changes")]
        public List<AlteracaoWhatsAppDTO> Changes { get; set; } = new();
    }
}
