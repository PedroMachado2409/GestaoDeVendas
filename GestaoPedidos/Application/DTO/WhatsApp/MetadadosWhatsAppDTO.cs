using System.Text.Json.Serialization;

namespace GestaoPedidos.Application.DTO.WhatsApp
{
    public class MetadadosWhatsAppDTO
    {
        [JsonPropertyName("display_phone_number")]
        public string DisplayPhoneNumber { get; set; } = string.Empty;

        [JsonPropertyName("phone_number_id")]
        public string PhoneNumberId { get; set; } = string.Empty;
    }
}
