using LawPlatform.Utilities.Enums;
using System.Text.Json.Serialization;

namespace LawPlatform.Entities.DTO.Payment
{
    public class ConsultationPaymentRequest
    {
        public Guid ConsultationId { get; set; }
        public string PaymentType { get; set; } = "PAY_BY_INSTALMENTS";
        public string SuccessUrl { get; set; }
        public string FailureUrl { get; set; }
        public string CancelUrl { get; set; }
    }

    public class PaymentOptionsRequest
    {
        public CountryCodes Country { get; set; } // SA, AE, KW, etc.
        public decimal Amount { get; set; }
        public Currency Currency { get; set; } // SAR, AED, KWD
        public string PhoneNumber { get; set; }
        public bool IsVip { get; set; }
    }

    public class PaymentOptionsResponse
    {
        [JsonPropertyName("has_available_payment_options")]
        public bool has_available_payment_options { get; set; }

        [JsonPropertyName("single_checkout_enabled")]
        public bool single_checkout_enabled { get; set; }

        [JsonPropertyName("available_payment_labels")]
        public List<PaymentType> available_payment_labels { get; set; } = new List<PaymentType>();
    }

    public class PaymentType
    {
        [JsonPropertyName("payment_type")]
        public string payment_type { get; set; }

        [JsonPropertyName("description_en")]
        public string description_en { get; set; }

        [JsonPropertyName("description_ar")]
        public string description_ar { get; set; }

        [JsonPropertyName("instalments")]
        public int instalments { get; set; }
    }

}