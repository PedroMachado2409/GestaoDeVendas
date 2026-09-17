using System.Text.Json.Serialization;

namespace GestaoPedidos.Application.DTO.WhatsApp
{
    public class ValorWhatsAppDTO
    {
        [JsonPropertyName("messaging_product")]
        public string MessagingProduct { get; set; } = string.Empty;

        [JsonPropertyName("metadata")]
        public MetadadosWhatsAppDTO Metadata { get; set; } = new();

        [JsonPropertyName("contacts")]
        public List<ContatoWhatsAppDTO>? Contacts { get; set; }

        [JsonPropertyName("messages")]
        public List<MensagemRecebidaWhatsAppDTO>? Messages { get; set; }
    }
}
