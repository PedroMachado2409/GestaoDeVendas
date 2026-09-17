using System.Text.Json.Serialization;

namespace GestaoPedidos.Application.DTO.WhatsApp
{
    public class PerfilWhatsAppDTO
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }
}
