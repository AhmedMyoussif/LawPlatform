namespace LawPlatform.Entities.DTO.Payment
{
    public class TamaraCheckoutResponse
    {
        public string checkout_id { get; set; }
        public string checkout_url { get; set; }
        public string order_id { get; set; }
        public string status { get; set; }
    }

    public class TamaraErrorResponse
    {
        public string error { get; set; }
        public string error_code { get; set; }
        public string message { get; set; }
        public List<string> errors { get; set; }
    }

    public class TamaraOrderResponse
    {
        public string order_id { get; set; }
        public string order_reference_id { get; set; }
        public string order_number { get; set; }
        public string description { get; set; }
        public OrderConsumer consumer { get; set; }
        public string status { get; set; }
        public Address shipping_address { get; set; }
        public Address billing_address { get; set; }
        public List<OrderItemResponse> items { get; set; }
        public string payment_type { get; set; }
        public int instalments { get; set; }
        public MoneyAmount total_amount { get; set; }
        public MoneyAmount shipping_amount { get; set; }
        public MoneyAmount tax_amount { get; set; }
        public DiscountResponse discount_amount { get; set; }
        public MoneyAmount captured_amount { get; set; }
        public MoneyAmount refunded_amount { get; set; }
        public MoneyAmount canceled_amount { get; set; }
        public MoneyAmount paid_amount { get; set; }
        public string settlement_status { get; set; }
        public DateTime? settlement_date { get; set; }
        public string created_at { get; set; }
        public MoneyAmount wallet_prepaid_amount { get; set; }
        public TransactionsInfo transactions { get; set; }
        public bool processing { get; set; }
        public string store_code { get; set; }
        public AdditionalData additional_data { get; set; }
    }

    public class OrderConsumer
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string phone_number { get; set; }
        public string national_id { get; set; }
        public string date_of_birth { get; set; }
        public bool? is_first_order { get; set; }
    }

    public class OrderItemResponse
    {
        public string reference_id { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public string sku { get; set; }
        public int quantity { get; set; }
        public MoneyAmount tax_amount { get; set; }
        public MoneyAmount total_amount { get; set; }
        public MoneyAmount unit_price { get; set; }
        public MoneyAmount discount_amount { get; set; }
        public string image_url { get; set; }
        public string item_url { get; set; }
    }

    public class DiscountResponse
    {
        public string name { get; set; }
        public MoneyAmount amount { get; set; }
    }

    public class TransactionsInfo
    {
        public List<object> cancels { get; set; }
        public List<object> captures { get; set; }
        public List<object> refunds { get; set; }
    }

    public class AdditionalData
    {
        public bool single_checkout { get; set; }
        public string checkout_variance { get; set; }
        public bool pay_now_cvv_disabled { get; set; }
        public decimal pay_now_cashback_value { get; set; }
        public bool from_in_store_payment_link { get; set; }
        public decimal pay_now_cashback_percentage { get; set; }
    }

    public class TamaraCancelOrderRequest
    {
        public MoneyAmount total_amount { get; set; }
        public MoneyAmount shipping_amount { get; set; }
        public MoneyAmount tax_amount { get; set; }
        public MoneyAmount discount_amount { get; set; }
        public List<CancelOrderItem> items { get; set; }
    }

    public class CancelOrderItem
    {
        public string name { get; set; }
        public string type { get; set; }
        public string reference_id { get; set; }
        public string sku { get; set; }
        public int quantity { get; set; }
        public MoneyAmount discount_amount { get; set; }
        public MoneyAmount tax_amount { get; set; }
        public MoneyAmount unit_price { get; set; }
        public MoneyAmount total_amount { get; set; }
    }

    public class TamaraCancelOrderResponse
    {
        public string cancel_id { get; set; }
        public string order_id { get; set; }
        public string status { get; set; }
        public List<MoneyAmount> canceled_amount { get; set; }
    }

    public class TamaraAuthorizeOrderResponse
    {
        public string order_id { get; set; }
        public string status { get; set; }
        public string order_expiry_time { get; set; }
        public string payment_type { get; set; }
        public bool auto_captured { get; set; }
        public List<MoneyAmount> authorized_amount { get; set; }
        public string capture_id { get; set; }
    }

    public class TamaraWebhookNotification
    {
        public string order_id { get; set; }
        public string order_reference_id { get; set; }
        public string order_status { get; set; }
        public List<object> data { get; set; }
    }
}
