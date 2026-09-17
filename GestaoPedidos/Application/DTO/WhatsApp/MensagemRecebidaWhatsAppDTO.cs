using System.Text.Json.Serialization;

namespace GestaoPedidos.Application.DTO.WhatsApp
{
    public class MensagemRecebidaWhatsAppDTO
    {
        [JsonPropertyName("from")]
        public string From { get; set; } = string.Empty;

        [JsonPropertyName("from_user_id")]
        public string? FromUserId { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public TextoWhatsAppDTO? Text { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
    }
}
