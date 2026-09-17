using System.Text.Json.Serialization;

namespace GestaoPedidos.Application.DTO.WhatsApp
{
    public class ContatoWhatsAppDTO
    {
        [JsonPropertyName("profile")]
        public PerfilWhatsAppDTO? Profile { get; set; }

        [JsonPropertyName("wa_id")]
        public string WaId { get; set; } = string.Empty;

        [JsonPropertyName("user_id")]
        public string? UserId { get; set; }
    }
}
