using System.Text.Json.Serialization;

namespace GestaoPedidos.Application.DTO.WhatsApp
{
    /// <summary>
    /// Corpo da notificação enviada pela Meta ao webhook.
    /// O formato é definido pela Cloud API do WhatsApp; os nomes em inglês
    /// vêm do contrato externo e por isso são mantidos em JsonPropertyName.
    /// </summary>
    public class WebhookWhatsAppDTO
    {
        [JsonPropertyName("object")]
        public string Object { get; set; } = string.Empty;

        [JsonPropertyName("entry")]
        public List<EntradaWhatsAppDTO> Entry { get; set; } = new();
    }
}
