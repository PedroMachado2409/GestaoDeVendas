using System.Text.Json.Serialization;

namespace GestaoPedidos.Application.DTO.WhatsApp
{
    public class TextoWhatsAppDTO
    {
        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;
    }
}
